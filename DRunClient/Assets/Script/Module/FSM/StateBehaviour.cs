using System.Collections;
using System.Collections.Generic;

namespace Festa.Client.Module.FSM
{
	public abstract class StateBehaviour<T> where T : class
	{
		protected FiniteStateMachine<T> _owner;

		public abstract int getType();
		public virtual string getName()
		{
			return GetType().Name;
		}

		public FiniteStateMachine<T> getOwner()
		{
			return _owner;
		}

		public virtual void setOwner(FiniteStateMachine<T> owner)
		{
			_owner = owner;
		}

		public virtual void onEnter(StateBehaviour<T> prev_state,T param)
		{

		}

		public virtual void onExit(StateBehaviour<T> next_state)
		{

		}
		
		public virtual void update()
		{
			
		}

        public virtual void start()
        {

        }
	}
}