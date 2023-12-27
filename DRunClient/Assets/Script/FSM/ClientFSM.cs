using Assets.Script_DRun.FSM.State;
using DRun.Client;
using Festa.Client.Module.FSM;
using System;
using System.Collections.Generic;

namespace Festa.Client
{
	public class ClientFSM : FiniteStateMachine<Object>
	{
		public static ClientFSM create()
		{
			ClientFSM fsm = new ClientFSM();
			fsm.init();
			return fsm;
		}

		protected override void init()
		{
			base.init();

			this._logDebug = true;

			createStates();
			createSequences();

			changeState(ClientStateType.startup);
		}

		private void createStates()
		{
			registerState<StateFirebaseLogin>();
			registerState<StateServerLogin>();
            registerState<StateInitAccount>();
            registerState<StateSleep>();
			registerState<StateStartup>();
			registerState<StateHealthDevice>();
			registerState<StateLocationDevice>();
			//registerState<StateRunning>();
			registerState<StateLoadRefData>();
			registerState<StateSelectServer>();
			registerState<StateProcessHealthLog>();
			registerState<StateEndLoading>();
			registerState<StateRegisterPush>();
			registerState<StateStartLocalChatData>();
			registerState<StateFirebasePush>();
			registerState<StateFirebaseApp>();
			registerState<StateBecomeActive>();
			registerState<StateInitMapBox>();
			registerState<StateQueryChatRoomList>();
			registerState<StatePauseTrip>();
#if UNITY_ANDROID
			registerState<StateCheckPermissionAndroid>();
#elif UNITY_IPHONE
			registerState<StateCheckPermissioniOS>();
#endif

			// drun
			registerState<StateCheckSignIn>();
			registerState<StateSignIn>();
			registerState<StateSignUp>();
			registerState<StateResetPassword>();
			registerState<StateLogin>();
			registerState<StateRun>();
			registerState<StateExpireDailyStepReward>();
			registerState<StateClaimWeeklyReward>();
			registerState<StateContinueRunning>();
			registerState<StateProcessNFT>();
			registerState<StateReadToayMarathonRecord>();
			registerState<StateReadRemoteConfig>();
			registerState<StateCheckForceUpdate>();

		}

		private void createSequences()
		{
			List<int> seq = new List<int>();

			seq.Add(ClientStateType.startup);
#if !UNITY_EDITOR
			seq.Add(ClientStateType.firebase_app);			// 5%
#endif

			seq.Add(ClientStateType.read_remote_config);
			seq.Add(ClientStateType.check_force_update);
			seq.Add(ClientStateType.init_health_device);	// 12%
			seq.Add(ClientStateType.init_location_device);	// 13%
			seq.Add(ClientStateType.init_mapbox);			// 14%

			seq.Add(ClientStateType.select_server);			// 30%
			seq.Add(ClientStateType.load_refdata);          // 31%

#if !UNITY_EDITOR
#if UNITY_ANDROID
			seq.Add(ClientStateType.check_permission_android); // 35%
#elif UNITY_IPHONE
			seq.Add(ClientStateType.check_permission_ios);	// 35%
#endif
#endif

			seq.Add(ClientStateType.check_signin);          // 40%
			seq.Add(ClientStateType.signin);                // 41%
			seq.Add(ClientStateType.login);                 // 50%
			seq.Add(ClientStateType.process_health_log);    // 51%
			seq.Add(ClientStateType.continue_running);            // 80%
			seq.Add(ClientStateType.expire_daily_step_reward); // 90%
			seq.Add(ClientStateType.claim_weekly_reward);   // 95%
			seq.Add(ClientStateType.process_nft);           // 96%;
			seq.Add(ClientStateType.read_today_marathon_record); // 97%
			seq.Add(ClientStateType.end_loading);           // 100%
			seq.Add(ClientStateType.run);

			

//#if !UNITY_EDITOR
//			seq.Add(ClientStateType.firebase_login);		// 40%
//#endif
//			seq.Add(ClientStateType.server_login);			// 41%
//            seq.Add(ClientStateType.init_account);			// 50%

//            seq.Add(ClientStateType.process_health_log);    // 51%
//			seq.Add(ClientStateType.pause_trip);			// 80%
//			//eq.Add(ClientStateType.process_walklevel);
//			seq.Add(ClientStateType.start_local_chatdata);	// 80%
//			seq.Add(ClientStateType.query_chatroom_list);	// 81%
//#if !UNITY_EDITOR
//			seq.Add(ClientStateType.firebase_push);			// 100%
//#endif
//			seq.Add(ClientStateType.end_loading);			// 100%
//			seq.Add(ClientStateType.running);

			//---------------------------------------------------------------
			for(int i = 0; i < seq.Count; ++i)
			{
				ClientStateBehaviour behaviour = (ClientStateBehaviour)base.getState(seq[i]);

				if (i + 1 < seq.Count)
				{
					behaviour.setNextState(seq[i + 1]);
				}
			}
		}

		private void registerState<T>() where T : ClientStateBehaviour, new()
		{
			registerState(ClientStateBehaviour.create<T>());
		}

	}
}
