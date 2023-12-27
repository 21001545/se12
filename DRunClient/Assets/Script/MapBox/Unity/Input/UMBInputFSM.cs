using Festa.Client.Module;
using Festa.Client.Module.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace Festa.Client.MapBox
{
	public class UMBInputFSM : FiniteStateMachine<object>
	{
		private UnityMapBox _mapBox;
		private AbstractInputModule _inputModule;

		private UnityAction<UMBActor> _onClickActor;

		public UnityAction<UMBActor> OnClickActor
		{
			get { return _onClickActor; }
			set { _onClickActor = value; }
		}

		public static UMBInputFSM create(UnityMapBox mapBox)
		{
			UMBInputFSM input = new UMBInputFSM();
			input.init(mapBox);
			return input;
		}

		private void init(UnityMapBox mapBox)
		{
			base.init();

			//_logDebug = true;
			_mapBox = mapBox;
			_inputModule = _mapBox.getInputModule();

			createStates();
		}

		private void createStates()
		{
			registerState<UMBInputState_Wait>();
			registerState<UMBInputState_Scroll>();
			registerState<UMBInputState_Sleep>();
			registerState<UMBInputState_Pinch>();
			registerState<UMBInputState_ClickActor>();
			registerState<UMBInputState_MouseRotate>();
			changeState(UMBInputStateType.wait);
		}

		private void registerState<T>() where T : UMBInputStateBehaviour, new()
		{
			registerState(UMBInputStateBehaviour.create<T>(_mapBox));
		}
	}
}
