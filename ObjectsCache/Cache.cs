using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ObjectsCache
{

    /**
     * Менеджер обьектов.
     * 
     * Использовать вместо стандартного создания/удаления обьектов(GameObject.Instantiate).
     * 
     * Требования для использования:
     * С создания/удаления, логика обьектов меняется на включение/выключение-
     * у вас должны быть определены методы OnEnable/OnDisable с соответствующим функционалом, Start и Awake стоит избегать
     * 
     * Основная идея - сделать хранилище обьектов максимально независимым от загруженных или незагруженых сцен
     * На данный момент,архитектура следующая:
     * Есть синглтон,через который мы обращаемся к элементам хранилища,
     * он создается "по требованию", если это нужно, или если элемент, на который он ссылался - удален.
     * Это-обьект хранилища(GameObject, находящийся в сцене, которую хранилище создало для себя), который содержит очередь хранимых элементов,
     * также этот обьект отвечает за апдейты хранилища
     * 
     * Хранилище может сброситься в одном случае - при полной перезагрузке сцены(чего стоит избегать).
     * Для подгрузки сцен, лучше использовать соответствующий менеджер из соседнего пакета(GameSceneManagement)
     */
    /// <summary>
    /// For main thread only!
    /// 
    /// Maked object disabled - enable them manually!
    /// 
    /// Use constructor/destructor behaviour for cacheable objects in OnEnable/OnDisable methods!
    /// </summary>

    public class Cache
    {
 
        /// //////////////////////////////////////////////////////////////////////////////////////////////////////
        private static InnerStorage innerInstance;

        private static InnerStorage instance
        {
            get {
                if (!innerInstance)
                {
                    InitStorage();
                }
                return innerInstance;
            } 
        }
        private static void InitStorage()
        {
            var storageScene = SceneManager.CreateScene("Cache");
            var storageGO = new GameObject("CacheManager");
            SceneManager.MoveGameObjectToScene(storageGO, storageScene);
            innerInstance = storageGO.AddComponent<InnerStorage>();
        }
        public static GameObject Instantiate(GameObject prefab,Vector3 position, Quaternion rotation)
        {
            var result = instance.InnerInstantiate(SceneManager.GetActiveScene(),prefab, position, rotation);
            return result;
        }
        public static GameObject Instantiate(Scene scene,GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var result = instance.InnerInstantiate(scene, prefab, position, rotation);
            return result;
        }
        public static GameObject Instantiate(GameObject prefab)
        {
            var result = instance.InnerInstantiate(SceneManager.GetActiveScene(), prefab);
            return result;
        }
        public static GameObject Instantiate(Scene scene,GameObject prefab)
        {
            var result = instance.InnerInstantiate(scene, prefab);
            return result;
        }
        public static void Destroy(GameObject obj)
        {
            instance.InnerDestroy(obj);
        }
    }
}
