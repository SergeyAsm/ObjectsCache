using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ObjectsCache
{
    public class InnerStorage : MonoBehaviour
    {
        private Dictionary<GameObject, CacheQueue> cache = new Dictionary<GameObject, CacheQueue>();

        internal GameObject InnerInstantiate(Scene targetScene, GameObject prefab)
        {
            var curCache = GetCacheByPrefab(prefab);
            if (TryGetObject(curCache, prefab, out var curObject))
            {
                SceneManager.MoveGameObjectToScene(curObject, targetScene);
                return curObject;
            }
            curObject = MakeCouple(curCache, prefab);
            SceneManager.MoveGameObjectToScene(curObject, targetScene);
            return curObject;
        }
        internal GameObject InnerInstantiate(Scene targetScene, GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var result = InnerInstantiate(targetScene,prefab);
            result.transform.SetPositionAndRotation(position, rotation);
            return result;
        }
        internal void InnerDestroy(GameObject obj)
        {
            if (obj)
            {
                if (obj.TryGetComponent<PrefabStorage>(out var prefabStorage))
                {
                    if (obj.activeSelf)
                    {
                        var curCache = GetCacheByPrefab(prefabStorage.GetPrefab());
                        obj.SetActive(false);
                        SceneManager.MoveGameObjectToScene(obj, gameObject.scene);
                        obj.transform.SetParent(default);
                        obj.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                        if (obj.TryGetComponent<Rigidbody>(out var rb))
                        {
                            rb.velocity = Vector3.zero;
                            rb.angularVelocity = Vector3.zero;
                        }
                        //some other clear operations
                        curCache.AddAgain(obj);
                    }
                }
                else
                {
                    Debug.LogWarning("Object not cacheable : " + obj.name);
                    GameObject.Destroy(obj);
                }

            }
        }

        private bool TryGetObject(CacheQueue cache, GameObject prefab, out GameObject curObject)
        {
            if (cache.TryDequeue(out curObject))
            {
                Debug.Assert(curObject, "Manageable object " + prefab.name + " has been destroyed!");
                return true;
            }
            return false;
        }
        private GameObject MakeCouple(CacheQueue ourCache, GameObject prefab)
        {
            var result = MakeNew(ourCache, prefab);
            ourCache.Add(result);

            result = MakeNew(ourCache, prefab);
            ourCache.Add(result);

            //ourCache.IncTotal();
            if (TryGetObject(ourCache, prefab, out var curObject))
            {
                return curObject;
            }
            throw new MissingComponentException("Storage can't make object " + prefab.name + " for some reason");
        }
        private GameObject MakeNew(CacheQueue ourCache, GameObject prefab)
        {
            if (prefab.activeSelf)
            {
                prefab.SetActive(false);
            }
            var result = GameObject.Instantiate(prefab);
            SceneManager.MoveGameObjectToScene(result, gameObject.scene);
            if (result.TryGetComponent<PrefabStorage>(out var pStorage))
            {
                pStorage.SetPrefab(prefab);
            }
            else
            {
                result.AddComponent<PrefabStorage>().SetPrefab(prefab);
            }
            return result;
        }

        private CacheQueue GetCacheByPrefab(GameObject prefab)
        {
            if (cache.TryGetValue(prefab, out var result))
            {
                return result;
            }
            var newList = new CacheQueue(prefab.name,StorageSettings.LastSettings);
            cache.Add(prefab, newList);
            return newList;
        }

        public void Update()
        {
            var expired = new List<GameObject>();
            foreach (var item in cache)
            {
                item.Value.Update();
                if (item.Value.expired)
                {
                    expired.Add(item.Key);
                }
            }
            foreach (var item in expired)
            {
                cache.Remove(item);
            }
        }
        private void OnEnable()
        {
            
        }
        private void OnDisable()
        {
            PrintStatistics();
        }
        internal void PrintStatistics()
        {
            Debug.Log("Cache storage statistics:");

            foreach (var item in cache)
            {
                Debug.Log(item.Value.name + " cache = " + item.Value.GetCount() + " total = " + item.Value.GetTotalCount());
            }
            Debug.Log("-------------------------");
        }
    }
}
