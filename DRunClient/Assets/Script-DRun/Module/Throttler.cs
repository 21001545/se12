using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Festa.Client {
    public class Throttler {
        private readonly MonoBehaviour _monoProvider;
        public Throttler(MonoBehaviour monoProvider) => _monoProvider = monoProvider;

        public UnityAction throttle(UnityAction executor, float delay) {
            Coroutine handle = null;
            bool isBlocking = false;

            return () => {
                var waitYield = new WaitForSeconds(delay);

                if (handle == null) 
                    handle = _monoProvider.StartCoroutine(_());

                IEnumerator _() {
                    isBlocking = true;
                    while (isBlocking)
                        yield return waitYield;
                    
                    executor();
                    isBlocking = false;
                    
                    _monoProvider.StopCoroutine(handle);
                    handle = null;
                }
            };
        }
    }
}
