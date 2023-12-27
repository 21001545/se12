using DRun.Client.Run;
using DRun.Client.ViewModel;
using Festa.Client;
using Festa.Client.Module.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRun.Client.Running
{
	public class RecorderFSM : FiniteStateMachine<object>
	{
		private RunningViewModel ViewModel => ClientMain.instance.getViewModel().Running;

		public static RecorderFSM create()
		{
			RecorderFSM fsm = new RecorderFSM();
			fsm.init();
			return fsm;
		}

		protected override void init()
		{
			base.init();

			_logDebug = true;

			createStates();
			changeState(StateType.none);
		}

		private void registerState<T>() where T : RecorderStateBehaviour, new()
		{
			registerState(RecorderStateBehaviour.create<T>());
		}

		private void createStates()
		{
			registerState<StateNone>();
			registerState<StateInit>();
			registerState<StateWaitGPS>();
			registerState<StateWaitFirstMove>();
			registerState<StatePaused>();
			registerState<StateTracking>();
			registerState<StateEnd>();
			registerState<StateContinueFromLocalData>();
			registerState<StateWriteRunningLog>();
			registerState<StateFailWriteRunningLog>();
		}

		protected override void EnterState(StateBehaviour<object> state, StateBehaviour<object> prev_state, object param)
		{
			ViewModel.PrevStatus = prev_state == null ? 0 : prev_state.getType();
			ViewModel.Status = state.getType();

			base.EnterState(state, prev_state, param);
		}
	}
}
