using RoR2;
using RoR2.Audio;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace LOP
{
    public static class LOPUtil
    {
        public static void DestroyImmediateSafe(UnityEngine.Object obj, bool allowDestroyingAssets = false)
        {
#if UNITY_EDITOR
            GameObject.DestroyImmediate(obj, allowDestroyingAssets);
#else
            GameObject.Destroy(obj);
#endif
        }

        public static void Shuffle<T>(this Xoroshiro128Plus rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.RangeInt(0, n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }

        }
    }
}
