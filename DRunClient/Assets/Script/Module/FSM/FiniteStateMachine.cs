using System.Collections;
using System.Collections.Generic;

namespace Festa.Client.Module.FSM
{
	public class FiniteStateMachine<T> where T : class
	{
		protected Dictionary<int, StateBehaviour<T>> _states;
		protected StateBehaviour<T> _cur_state;
		protected StateBehaviour<T> _next_state;
		protected T _next_state_param;
		protected bool _logDebug;

		protected FiniteStateMachine()
		{
		}

		//public static FiniteStateMachine<T> create()
		//{
		//	FiniteStateMachine<T> fsm = new FiniteStateMachine<T>();
		//	fsm.init();
		//	return fsm;
		//}

		public StateBehaviour<T> getCurrentState()
		{
			return _cur_state;
		}

		protected virtual void init()
		{
			_states = new Dictionary<int,StateBehaviour<T>>();
			_cur_state = null;
			_next_state = null;
			_next_state_param = null;
			_logDebug = false;
		}

        public virtual void start()
        {
            var d_enum = _states.GetEnumerator();
            while(d_enum.MoveNext())
            {
                d_enum.Current.Value.start();
            }
        }

		public virtual void registerState(StateBehaviour<T> state)
		{
			state.setOwner(this);
			_states.Add( state.getType(), state);
		}

		protected virtual StateBehaviour<T> getState(int type)
		{
			StateBehaviour<T> state;
			if( _states.TryGetValue( type, out state) == false)
			{
				return null;
			}

			return state;
		}

		public virtual void run()
		{
			if( _cur_state != _next_state)
			{
				if( _cur_state != null)
				{
					ExitState( _cur_state, _next_state);
				}

				StateBehaviour<T> prev_state = _cur_state;
				_cur_state = _next_state;

				if( _cur_state != null)
				{
					T param = _next_state_param;
					EnterState( _cur_state, prev_state, param);
				}
			}

			if( _cur_state != null)
			{
				_cur_state.update();
			}
		}

		protected virtual void ExitState(StateBehaviour<T> state,StateBehaviour<T> next_state)
		{
			if(_logDebug)
			{
				Logger.log(string.Format("{0}: onExit[{1}][{2}]", GetType().Name, state.getType(), state.getName()));
			}

			state.onExit( next_state);
		}

		protected virtual void EnterState(StateBehaviour<T> state,StateBehaviour<T> prev_state,T param)
		{
			if( _logDebug)
			{
				Logger.log(string.Format("{0}: onEnter[{1}][{2}]", GetType().Name, state.getType(), state.getName()));
			}

			state.onEnter( prev_state, param);
		}

		public virtual void changeState(int type,T param = null)
		{
			_next_state = getState(type);
			_next_state_param = param;

			if( _next_state == null)
            {
				throw new System.Exception("can't find state type : " + type);
            }

			if( _logDebug)
			{
				Logger.log( string.Format("{0}: reserve changeState[{1}][{2}]", GetType().Name, _next_state.getType(), _next_state.getName()));
			}
		}
	}
}