using DRun.Client;
using DRun.Client.Module;
using DRun.Client.NFT;
using DRun.Client.Running;
using Festa.Client.LocalDB;
using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using System.Globalization;
using UnityEngine;

namespace Festa.Client
{
	public class ClientMain : MonoBehaviour
	{
		public static ClientMain instance = null;

		public Transform root;

		// 여기 잠시만 얌전히 있어라
		public BasicModeLevelImage basicModeLevelImage;

		private ClientNetwork _network;
		private ClientFSM _fsm;
		private ClientDataManager _data;
		private ClientHealthManager _health;
		//private ClientLocationManager _location;
		private ClientMapRevealManager _mapReveal;
		private ClientViewModel _viewModel;
		private MultiThreadWorker _multiThreadWorker;
		private UIPanelNavigationStack _panelNavigationStack;
		private MBTileCache _mbTileCache;   // 일단 여기 있어라
		private MBStyleCache _mbStyleCache;
		private LandTileCache _landTileCache;
		private ClientProfileCacheManager _profileCache;
		private LocalChatDataManager _localChatData;
		private ClientFirebasePushManager _pushManager;
		private LocaleManager _localeManager;
		private TextureCache _textureCache;
		private RunningRecorder _runningRecorder;
		private AbstractGPSTracker _gpsTracker;
		private NFTMetadataCacheManager _nftMetadataCache;

		public ClientNetwork getNetwork()
		{
			return _network;
		}

		public ClientFSM getFSM()
		{
			return _fsm;
		}

		public ClientDataManager getData()
		{
			return _data;
		}

		public ClientHealthManager getHealth()
		{
			return _health;
		}

		//public ClientLocationManager getLocation()
		//{
		//	return _location;
		//}

		public ClientMapRevealManager getMapReveal()
		{
			return _mapReveal;
		}

		public ClientViewModel getViewModel()
		{
			return _viewModel;
		}

		public MultiThreadWorker getMultiThreadWorker()
		{
			return _multiThreadWorker;
		}

		public ClientProfileCacheManager getProfileCache()
		{
			return _profileCache;
		}

		public MBTileCache getMBTileCache()
		{
			return _mbTileCache;
		}

		public LandTileCache getLandTileCache()
		{
			return _landTileCache;
		}

		public MBStyleCache getMBStyleCache()
		{
			return _mbStyleCache;
		}

		public LocalChatDataManager getLocalChatData()
		{
			return _localChatData;
		}

		public ClientFirebasePushManager getPushManager()
		{
			return _pushManager;
		}

		public LocaleManager getLocale()
		{
			return _localeManager;
		}

		public TextureCache getTextureCache()
		{
			return _textureCache;
		}

		public UIPanelNavigationStack getPanelNavigationStack()
		{
			return _panelNavigationStack;
		}

		public RunningRecorder getRunningRecorder()
		{
			return _runningRecorder;
		}

		public AbstractGPSTracker getGPSTracker()
		{
			return _gpsTracker;
		}

		public NFTMetadataCacheManager getNFTMetadataCache()
		{
			return _nftMetadataCache;
		}

		private void Awake()
		{
			instance = this;

			TouchScreenKeyboard.Android.consumesOutsideTouches = false;

#if !UNITY_EDITOR
			Application.targetFrameRate = System.Math.Max(60,Screen.currentResolution.refreshRate);
			Debug.Log("targetFrameRate : " + Application.targetFrameRate);
#endif

			initNativeModule();
			initUIStyleDefine();
			initTouchScreenKeyboardUtil();
			initMultiThreadWorker();
			initNetwork();
			initClientData();
			initViewModel();
			initHealth();
			initLocation();
			initMapBox();
			initProfileCacheManager();
			initLocalChatData();
			initFirebasePushManager();
			initLocaleManager();
			initGPSTracker();
			initRunningRecorder();
			initTextureCache();
			initPanelNavigationStack();
			initNFTMetadataCache();
			initSingletons();

			initFSM();

			UILoading.getInstance().open();

#if UNITY_EDITOR
			test();
#endif
		}

		private void initNativeModule()
		{
			NativeModule.initialize();
		}

		private void initUIStyleDefine()
		{
			UIStyleDefine.init();
		}

		private void initTouchScreenKeyboardUtil()
		{
			TouchScreenKeyboardUtil.createInstance();
		}

		private void initSingletons()
		{
			SingletonInitializer initializer = SingletonInitializer.create();
			initializer.run(root);
		}

		private void initFSM()
		{
			_fsm = ClientFSM.create();

		}

		private void initHealth()
		{
			_health = ClientHealthManager.create();
		}

		private void initLocation()
		{
			//_location = ClientLocationManager.create();
			_mapReveal = ClientMapRevealManager.create();
		}

		private void initMapBox()
		{
			MBStyleDefine.initStatic();
			_mbTileCache = MBTileCache.create();
			_landTileCache = LandTileCache.create();
			_mbStyleCache = MBStyleCache.create();
		}

		private void initClientData()
		{
			_data = ClientDataManager.create();
		}

		private void initViewModel()
		{
			_viewModel = ClientViewModel.create();
		}

		private void initNetwork()
		{
			_network = ClientNetwork.create(gameObject);
		}

		private void initMultiThreadWorker()
		{
			_multiThreadWorker = MultiThreadWorker.create();
		}

		private void initProfileCacheManager()
		{
			_profileCache = ClientProfileCacheManager.create();
		}

		private void initLocalChatData()
		{
			_localChatData = LocalChatDataManager.create();
		}

		private void initFirebasePushManager()
		{
			_pushManager = ClientFirebasePushManager.create();
		}

		private void initLocaleManager()
		{
			_localeManager = LocaleManager.create();

			Debug.Log($"current country code : {_localeManager.getCountryCode()}");
		}

		private void initTextureCache()
		{
			_textureCache = TextureCache.create();
		}

		private void initPanelNavigationStack()
		{
			_panelNavigationStack = UIPanelNavigationStack.create();
		}

		private void initRunningRecorder()
		{
			_runningRecorder = RunningRecorder.create();
		}

		private void initGPSTracker()
		{
			_gpsTracker = AbstractGPSTracker.create();
		}

		private void initNFTMetadataCache()
		{
			_nftMetadataCache = NFTMetadataCacheManager.create();
		}

		private void Update()
		{
			_network.update();
			_textureCache.update();
			UIManager.getInstance().update();
			UMBOfflineRenderer.getInstance().update();
			MainThreadDispatcher.flush();
			TouchScreenKeyboardUtil.getInstance().update();
		}

		private void OnDestroy()
		{
			_multiThreadWorker.stop();
			NativeModule.release();
		}

		private void FixedUpdate()
		{
			_fsm.run();
			UIManager.getInstance().updateFixed();
			MainThreadDispatcher.flushFixedUpdate();
		}

		private void OnApplicationPause(bool pause)
		{
			#if UNITY_ANDROID || UNITY_EDITOR
				onPause(pause);
			#endif
		}

		private void OnApplicationFocus(bool focus)
		{
			#if UNITY_IOS && !UNITY_EDITOR
				onPause(focus == false);
			#endif
		}

		private void onPause(bool pause)
		{
			if (pause == false && _fsm != null)
			{
				if (_fsm.getCurrentState() != null &&
					_fsm.getCurrentState().getType() == ClientStateType.run)
				{
					_fsm.changeState(ClientStateType.become_active);
				}
			}
		}

		/*
		function exponentialInterpolation(input, base, lowerValue, upperValue)
		{
			const difference = upperValue - lowerValue;
			const progress = input - lowerValue;

			if (difference === 0)
			{
				return 0;
			}
			else if (base === 1)
			{
				return progress / difference;
			}
			else
			{
				return (Math.pow(base, progress) - 1) / (Math.pow(base, difference) - 1);
			}
		}*/


		private void test()
		{
			//double value = 0.0000001;

			//Debug.Log(value.ToString(CultureInfo.InvariantCulture));
			//Debug.Log(value.ToString("F20"));
			//Debug.Log(value.ToString("N",CultureInfo.InvariantCulture));
			//Debug.Log(StringUtil.TrimAllZeroWithinFloatingPoints(value.ToString("F20")));

			//double[] value_tests = new double[]
			//{
			//	0.123456789,
			//	123123.123123,
			//	123123,
			//	0.000001
			//};

			//foreach(double v in value_tests)
			//{
			//	double ceiled = System.Math.Ceiling(v * 10000.0) / 10000.0;
			//	Debug.Log($"{v.ToString("F20").TrimAllZeroWithinFloatingPoints()} => {ceiled.ToString("F20").TrimAllZeroWithinFloatingPoints()}");
			//}

			//string[] trim_tests = new string[]
			//{
			//	"100.0",
			//	"10000000",
			//	"1.234123400",
			//	"0.00000100000000",
			//};
			
			//foreach(string v in trim_tests)
			//{
			//	Debug.Log($"{v} => {v.TrimAllZeroWithinFloatingPoints()}");
			//}

		}
	}
}
