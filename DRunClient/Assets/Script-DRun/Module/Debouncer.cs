using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Festa.Client
{
    /// <summary>
    /// logic 호출을 지연시켜 1번만 호출 해줌.
    /// </summary>
    public static class Debouncer
    {
        private static Coroutine handle;
        
        /// <summary>
        /// 이전 실행을 전부 무시하고, delay 이후 executor 를 단 1번 호출해줌. 1초안에 200 번 호출 -> delay 후에 단 1번만 호출.
        /// </summary>
        /// <param name="mono">Coroutine 용</param>
        /// <param name="executor">실행할 콜백</param>
        /// <param name="delay">콜백 지연실행 시점</param>
        public static void debounce(MonoBehaviour mono, UnityAction executor, float delay)
        {
            if (handle != null)
            {
                mono.StopCoroutine(handle);
                handle = null;
            }
            
            handle = mono.StartCoroutine(_());

            IEnumerator _()
            {
                yield return new WaitForSeconds(delay);
                executor();

                mono.StopCoroutine(handle);
                handle = null;
            }
        }
    }
}