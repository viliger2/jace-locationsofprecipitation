using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using RoR2.ExpansionManagement;

using System;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace LOP {
    //Need neb to review this
    public class SetupDLC3AccessNode : MonoBehaviour
    {
        [Tooltip("Boss fight override component.")]
        [SerializeField]
        private SolusFight solusFight;
        [Tooltip("Portal Spawner for Conduit Canyon.")]
        [SerializeField]
        private PortalSpawner ccPortalSpawner;
        [Tooltip("Portal Spawner for Solutional Haunt.")]
        [SerializeField]
        private PortalSpawner shPortalSpawner;
        [Tooltip("Portal Spawner for Computational Exchange.")]
        [SerializeField]
        private PortalSpawner cePortalSpawner;
        [Tooltip("Mission Controller for the Access Nodes, this needs to be disabled initially.")]
        [SerializeField]
        private AccessCodesMissionController acMissionController;

        public void Start()
        {
            //Access Node
            GameObject accessNodeAsset = Addressables.LoadAssetAsync<GameObject>(Constants.AssetGUIDS.accessNodeGUID).WaitForCompletion();
            GameObject accessNode = GameObject.Instantiate(accessNodeAsset, this.transform);
            NetworkServer.Spawn(accessNode);

            AccessCodesNodeData nodeData;
            nodeData.node = accessNode;
            nodeData.id = 0;

            AccessCodesNodeData[] nodeDataArray = new AccessCodesNodeData[1];
            nodeDataArray[0] = nodeData;

            acMissionController.nodes = nodeDataArray;

            //Expansion Requirement
            ExpansionDef dlc3 = Addressables.LoadAssetAsync<ExpansionDef>(Constants.AssetGUIDS.dlc3ExpansionGUID).WaitForCompletion();
            acMissionController.requiredExpansion = dlc3;
            solusFight.requiredExpansion = dlc3;
            ccPortalSpawner.requiredExpansion = dlc3;
            cePortalSpawner.requiredExpansion = dlc3;
            shPortalSpawner.requiredExpansion = dlc3;

            //Portal Spawner Cards
            InteractableSpawnCard ccPortalCard = Addressables.LoadAssetAsync<InteractableSpawnCard>(Constants.AssetGUIDS.iscHardwareProgPortalGUID).WaitForCompletion();
            ccPortalSpawner.portalSpawnCard = ccPortalCard;

            InteractableSpawnCard shPortalCard = Addressables.LoadAssetAsync<InteractableSpawnCard>(Constants.AssetGUIDS.iscHardwareProgPortalHauntGUID).WaitForCompletion();
            shPortalSpawner.portalSpawnCard = shPortalCard;

            InteractableSpawnCard cePortalCard = Addressables.LoadAssetAsync<InteractableSpawnCard>(Constants.AssetGUIDS.iscSolusShopPortalGUID).WaitForCompletion();
            cePortalSpawner.portalSpawnCard = cePortalCard;

            acMissionController.gameObject.SetActive(true);
        }
    }
}
