using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drun.Client
{
    internal class YielderDummyComponent : MonoBehaviour {}
    
    public static class Yielder
    {
        private static MonoBehaviour __sharedMono;

        /// <summary>
        /// 2022-12-27 윤상
        /// static 한 MonoBehaviour 가 필요할 때 사용.
        /// </summary>
        public static MonoBehaviour SharedMono
        {
            get
            {
                if (__sharedMono != null)
                    return __sharedMono;

                var sharedMonoHolder = new GameObject(nameof(Yielder))
                {
                    // 보여줄 필요 없음.
                    hideFlags = HideFlags.HideAndDontSave | HideFlags.NotEditable | HideFlags.HideInHierarchy
                };

                UnityEngine.Object.DontDestroyOnLoad(sharedMonoHolder);

                __sharedMono = sharedMonoHolder.AddComponent<YielderDummyComponent>();

                return __sharedMono;
            }
        }

        private static Dictionary<float, WaitForSeconds> wfs = new();

        public static WaitForSeconds seconds(float seconds)
        {
	        if (wfs.TryGetValue(seconds, out var cachedWfs)) 
				return cachedWfs;

	        WaitForSeconds newWfs = new(seconds);
	        wfs.Add(seconds, newWfs);

	        return newWfs;
        }

        public static Coroutine run(IEnumerator coroutineFn) => 
	        SharedMono.StartCoroutine(coroutineFn);

        public static void kill(Coroutine handle) => SharedMono.StopCoroutine(handle);


        //private static Coroutine _pauseTempHandle;
        //public static void pause(Coroutine handle)
        //{
	       // _pauseTempHandle = handle;


        //}

        // public static void tick(Func<IEnumerator> action)
        // {
        //     var sharedMono = SharedMono;
        //     if (sharedMono == null)
        //         return;
        //             
        //     sharedMono.StartCoroutine(action());
        // }
    }
}