using AOT;
using Festa.Client.Module;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_IPHONE

namespace Festa.Client
{
    public class CMPedometerPlugin
    {
        private static CMPedometerPlugin _instance;

        public delegate void _NativeInitCallback(bool result);
        public delegate void _NativeStepCallback(int caller_id, bool reuslt, float step);

        [DllImport("__Internal")]
        static extern void initializeCMPedometerPlugin(_NativeInitCallback callback);

        [DllImport("__Internal")]
        static extern void cmp_readStepCountLive(int caller_id, _NativeStepCallback callback);

        [DllImport("__Internal")]
        static extern void cmp_readStepCountPast(long begin, long end, int caller_id, _NativeStepCallback callback);

        [MonoPInvokeCallback(typeof(_NativeInitCallback))]
        private static void NativeInitCallback(bool result)
		{
            MainThreadDispatcher.dispatch(() => {
                _instance._currentInitCallback(result);
            });
		}

        [MonoPInvokeCallback(typeof(_NativeStepCallback))]
        private static void NativeStepCallback(int caller_id,bool result,float step)
		{
            UnityAction<bool, float> callback;
            if(_instance._stepCallbacks.TryGetValue(caller_id, out callback))
			{
                MainThreadDispatcher.dispatch(() => {
                    callback(result, step);
                });
			}
		}

        private UnityAction<bool> _currentInitCallback;
        private Dictionary<int, UnityAction<bool, float>> _stepCallbacks;


        public static CMPedometerPlugin create()
		{
            CMPedometerPlugin plugin = new CMPedometerPlugin();
            plugin.init();
            _instance = plugin;
            return plugin;
		}

        private void init()
		{

		}

        public void initPlugin(UnityAction<bool> callback)
		{
            _currentInitCallback = callback;
            _stepCallbacks = new Dictionary<int, UnityAction<bool, float>>();
            initializeCMPedometerPlugin(NativeInitCallback);
		}

        private int makeCallerID()
		{
            return EncryptUtil.makeHashCode(TimeUtil.unixTimestampUtcNow().ToString() + UnityEngine.Random.value.ToString());
		}

        public void queryStepCountLive(UnityAction<bool,float> callback)
		{
            int caller_id = makeCallerID();
            UnityAction<bool,float> internal_callback = (result, step) =>{
                callback(result, step);
                _stepCallbacks.Remove(caller_id);
            };

            _stepCallbacks.Add(caller_id, internal_callback);
            cmp_readStepCountLive(caller_id,NativeStepCallback);
		}

        public void queryStepCountPast(long begin,long end,UnityAction<bool,float> callback)
		{
            int caller_id = makeCallerID();
            UnityAction<bool,float> internal_callback = (result, step) => {
                callback(result, step);
                _stepCallbacks.Remove(caller_id);
            };

            _stepCallbacks.Add(caller_id, internal_callback);
            cmp_readStepCountPast(begin, end, caller_id, NativeStepCallback);
		}
    }
}

#endif