using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObjectsCache
{
    public class StorageSettings : ScriptableObject
    {
        public int cacheOverload = 1000;
        public float clearDelay = 1700;
        public int baseClearSize = 1;
        public float inactiveTime = 120;

        public static StorageSettings LastSettings;
        private void OnEnable()
        {
            LastSettings = this;
        }
    }
}
