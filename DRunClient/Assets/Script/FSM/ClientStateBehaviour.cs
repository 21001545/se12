using Festa.Client.Module.FSM;

namespace Festa.Client
{
	public abstract class ClientStateBehaviour : StateBehaviour<object>
	{
		protected ClientNetwork _network;
		protected ClientDataManager _data;
		protected ClientViewModel _viewModel;
		protected int _nextState;

		public static ClientStateBehaviour create<T>() where T : ClientStateBehaviour, new()
		{
			T state = new T();
			state.init();
			return state;
		}

		public void setNextState(int next)
		{
			_nextState = next;
		}

		protected virtual void init()
		{
			_network = ClientMain.instance.getNetwork();
			_data = ClientMain.instance.getData();
			_viewModel = ClientMain.instance.getViewModel();
			_nextState = ClientStateType.sleep;
		}

		protected virtual void changeToNextState()
		{
			_owner.changeState(_nextState);
		}

		protected virtual void changeToDesignatedState(int state)
		{
			_owner.changeState(state);
		}
	}
}
