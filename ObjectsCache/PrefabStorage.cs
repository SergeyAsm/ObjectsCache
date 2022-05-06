using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObjectsCache
{
    public class PrefabStorage : MonoBehaviour
    {
        private GameObject prefab;
        public void SetPrefab(GameObject prefab)
        {
            this.prefab = prefab;
        }
        public GameObject GetPrefab()
        {
            return prefab;
        }
    }
}
