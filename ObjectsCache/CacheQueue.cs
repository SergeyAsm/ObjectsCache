using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace ObjectsCache
{
    public class CacheQueue
    {
        private float nextClear = 0;
        private int clearSize = 0;
        private int totalCount = 0;
        private float lastActivity = 0;

        public bool expired = false;

        private static float defaultClearDelay = 1000;//17
        private static int defaultBaseClearSize = 1;
        private static int defaultCacheOverload = 1000;
        private static float defaultInactiveTime = 120;
        
        private StorageSettings settings;

        public float clearDelay { get => settings?settings.clearDelay:defaultClearDelay; }
        public int baseClearSize { get => settings ? settings.baseClearSize : defaultBaseClearSize; }
        public int cacheOverload { get => settings ? settings.cacheOverload : defaultCacheOverload; }
        public float inactiveTime { get => settings ? settings.inactiveTime : defaultInactiveTime; }

        public string name;
        private ConcurrentQueue<GameObject> queue = new ConcurrentQueue<GameObject>();

        public CacheQueue(string templateName,StorageSettings settings)
        {
            this.name = templateName;
            this.settings = settings;
            nextClear = clearDelay;
            clearSize = baseClearSize;
            totalCount = 0;
        }
        public int GetTotalCount()
        {
            return totalCount;
        }
        public void IncTotal()
        {
            totalCount++;
        }
        public int GetCount()
        {
            return queue.Count;
        }
        public bool TryDequeue(out GameObject curObject)
        {
            nextClear = Time.time + clearDelay;
            lastActivity = Time.time;
            return queue.TryDequeue(out curObject);
        }
        public void Add(GameObject obj)
        {
            totalCount++;
            clearSize = baseClearSize;
            nextClear = Time.time + clearDelay;
            lastActivity = Time.time;
            queue.Enqueue(obj);
            Debug.Assert(queue.Count < cacheOverload);
        }
        public void AddAgain(GameObject obj)
        {
            nextClear = Time.time + clearDelay;
            lastActivity = Time.time;
            queue.Enqueue(obj);
            Debug.Assert(queue.Count < cacheOverload);
        }
        public void Update()
        {
            expired = (Time.time - lastActivity > inactiveTime) && (queue.Count == 0);

            if (queue.Count < clearSize)
            {
                clearSize = Mathf.Max(baseClearSize, clearSize-1);
                return;
            }

            if (Time.time > nextClear)
            {
                for (int i = 0; i < clearSize; i++)
                {
                    if (queue.TryDequeue(out var objToDestroy))
                    {
                        GameObject.Destroy(objToDestroy);
                        totalCount--;
                    }
                    else
                    {
                        break;
                    }
                }
                clearSize++;
                nextClear = Time.time + clearDelay;
            }
        }
        public void ApplySettings(StorageSettings settings)
        {
            this.settings = settings;
        }
    }

}
