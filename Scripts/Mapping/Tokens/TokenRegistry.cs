using System;
using System.Linq;
using UnityEngine;

namespace Gameplay
{
    public class TokenRegistry : MonoBehaviour
    {
        public BrandPrefabEntry[] entries;

        public GameObject GetPrefab(BrandType brand)
        {
            return (from entry in entries where entry.brand == brand select entry.prefab).FirstOrDefault();
        }

        [Serializable]
        public struct BrandPrefabEntry
        {
            public BrandType brand;
            public GameObject prefab;
        }
    }
}