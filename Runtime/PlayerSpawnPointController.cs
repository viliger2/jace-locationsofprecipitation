using System;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using RoR2.Navigation;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace LOP
{
    public class PlayerSpawnPointController : MonoBehaviour
    {
        public enum SpawnPointMode
        {
            WithinRange,
            NearestNode,
            Static
        }

        public enum SpawnPointOrder
        {
            LinearWrap,
            PingPong,
            ClusterOnFirstValidPOI,
            Random
        }

        [Tooltip("List of referenced Transforms to create player spawn points.")]
        [SerializeField] public Transform[] spawnPointPOIs;
        [Tooltip("Mode of creating player spawn points. WithinRange creates spawn points at ground nodes around a POI within a range between minDistance and maxDistance. NearestNode creates a spawn point at the nearest ground node to the POI. Static creates a spawn point at the POI.")]
        [SerializeField] public SpawnPointMode spawnPointMode;
        [SerializeField] public float minDistance = 10f;
        [SerializeField] public float maxDistance = 40f;
        [Tooltip("Shuffle the POIs at the start of creating spawn points")]
        [SerializeField] public bool initialRandomization = true;

        //Should only serialize in editor when spawnPointMode == WithinRange
        [SerializeField] public bool createPointPerPlayer = false;

        //Should only serialize in editor when createPointPerPlayer = true && spawnPointMode == WithinRange
        [SerializeField] public SpawnPointOrder order;

        private NodeGraph groundNodes;

        private void OnEnable()
        {
            SceneDirector.onPreGeneratePlayerSpawnPointsServer += SceneDirector_onPreGeneratePlayerSpawnPointsServer;
        }

        private void OnDisable()
        {
            SceneDirector.onPreGeneratePlayerSpawnPointsServer -= SceneDirector_onPreGeneratePlayerSpawnPointsServer;
        }

        private void SceneDirector_onPreGeneratePlayerSpawnPointsServer(SceneDirector sceneDirector, ref Action generationMethod)
        {
            generationMethod = GeneratePlayerSpawnPointsServer;
        }
        private void GeneratePlayerSpawnPointsServer()
        {
            if (!SceneInfo.instance)
            {
                LOPLog.Error($"There is no SceneInfo instance, spawn points cannot be generated.");
                return;
            }

            if (spawnPointPOIs.Length == 0)
            {
                LOPLog.Error($"No spawn point POIs populated in {this}, spawn points cannot be generated.");
                return;
            }

            groundNodes = SceneInfo.instance.groundNodes;
            if (!groundNodes && spawnPointMode != SpawnPointMode.Static)
            {
                LOPLog.Error($"There are no ground nodes in this scene. Make sure there is a ground node asset or spawnPointMode is set to Static.");
                return;
            }

            if (initialRandomization)
            {
                RoR2Application.rng.Shuffle(spawnPointPOIs);
            }

            if (createPointPerPlayer)
            {
                if(spawnPointMode != SpawnPointMode.WithinRange)
                {
                    LOPLog.Warning($"Creating Spawn points per player is incompatible with any SpawnPointMode that isn't WithinRange. This is to prevent players from spawning on top of each other.");
                    GenerateSpawnPointsPerTransform();
                }
                GenerateSpawnPointsPerPlayer();
            }
            else
            {
                GenerateSpawnPointsPerTransform();
            }
        }

        private void GenerateSpawnPointsPerTransform()
        {
            for (int i = 0; i < spawnPointPOIs.Length; i++)
            {
                if (!spawnPointPOIs[i].gameObject.activeSelf)
                {
                    return;
                }

                if(spawnPointMode == SpawnPointMode.Static)
                {
                    SpawnPoint.AddSpawnPoint(spawnPointPOIs[i].position, spawnPointPOIs[i].rotation);
                    continue;
                }

                NodeGraph.NodeIndex spawnNode = NodeGraph.NodeIndex.invalid;
                if (spawnPointMode == SpawnPointMode.NearestNode)
                {
                    spawnNode = groundNodes.FindClosestNode(spawnPointPOIs[i].position, HullClassification.Human);
                }
                else
                {
                    var allNodes = groundNodes.FindNodesInRange(spawnPointPOIs[i].position, minDistance, maxDistance, HullMask.Human);
                    if (allNodes.Count > 0)
                    {
                        spawnNode = allNodes[UnityEngine.Random.Range(0, allNodes.Count)];
                    }
                }
                if (spawnNode == NodeGraph.NodeIndex.invalid)
                {

                    continue;
                }

                if (groundNodes.GetNodePosition(spawnNode, out Vector3 position))
                {
                    SpawnPoint.AddSpawnPoint(position, spawnPointPOIs[i].rotation);
                }
            }
        }

        private void GenerateSpawnPointsPerPlayer()
        {
            if (order == SpawnPointOrder.ClusterOnFirstValidPOI)
            {
                GenerateSpawnPointCluster();
            }
            else
            {
                GeneratePlayerSpawnPointsInteration();
            }
        }

        private void GenerateSpawnPointCluster()
        {
            int playerCount = Run.instance.participatingPlayerCount;
            NodeGraph.NodeIndex spawnNode = NodeGraph.NodeIndex.invalid;

            int validPOIIndex = 0;
            List<NodeGraph.NodeIndex> allNodes = new List<NodeGraph.NodeIndex>();

            for (validPOIIndex = 0; validPOIIndex < spawnPointPOIs.Length; validPOIIndex++)
            {
                allNodes = groundNodes.FindNodesInRange(spawnPointPOIs[validPOIIndex].position, minDistance, maxDistance, HullMask.Human);
                if (allNodes.Count >= 1) break;
                else
                {
                    LOPLog.Warning($"Cannot find a valid node to create a spawn point cluster around POI: {spawnPointPOIs[validPOIIndex].name}");
                }
            }

            if (allNodes.Count < 1)
            {
                LOPLog.Error($"Cannot create any valid spawn point clusters. No spawn points generated.");
                return;
            }

            for (int i = 0; i < playerCount; i++)
            {
                spawnNode = allNodes[UnityEngine.Random.Range(0, allNodes.Count)];
                if (spawnNode == NodeGraph.NodeIndex.invalid)
                {
                    continue;
                }
                if (groundNodes.GetNodePosition(spawnNode, out Vector3 position))
                {
                    SpawnPoint.AddSpawnPoint(position, spawnPointPOIs[validPOIIndex].rotation);
                }
            }
        }

        private void GeneratePlayerSpawnPointsInteration()
        {
            int playerCount = Run.instance.participatingPlayerCount;
            NodeGraph.NodeIndex spawnNode = NodeGraph.NodeIndex.invalid;

            int POIIndex = 0;
            int pingPongState = 1;
            for (int i = 0; i < playerCount; i++)
            {
                var allNodes = groundNodes.FindNodesInRange(spawnPointPOIs[POIIndex].position, minDistance, maxDistance, HullMask.Human);
                if (allNodes.Count > 0)
                {
                    spawnNode = allNodes[Run.instance.stageRng.RangeInt(0, allNodes.Count)];
                }
                if (spawnNode == NodeGraph.NodeIndex.invalid)
                {
                    LOPLog.Warning($"Cannot find a valid node to create a spawn point around POI: {spawnPointPOIs[POIIndex].name}");
                    continue;
                }
                if (groundNodes.GetNodePosition(spawnNode, out Vector3 position))
                {
                    SpawnPoint.AddSpawnPoint(position, spawnPointPOIs[POIIndex].rotation);
                }

                switch (order)
                {
                    case SpawnPointOrder.LinearWrap:
                        POIIndex++;
                        POIIndex = POIIndex % spawnPointPOIs.Length;
                        break;
                    case SpawnPointOrder.PingPong:
                        int nextIndex = POIIndex + pingPongState;
                        if (nextIndex == -1 || nextIndex == spawnPointPOIs.Length)
                        {
                            pingPongState *= -1;
                        }
                        POIIndex += pingPongState;
                        break;
                    case SpawnPointOrder.Random:
                        POIIndex = Run.instance.stageRng.RangeInt(0, allNodes.Count);
                        break;
                }
            }
        }
    }
}
