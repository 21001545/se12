using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client
{
	public class WorldInputFSM : FiniteStateMachine<object>
	{
		private World _world;

		public World getWorld()
		{
			return _world;
		}

		public static WorldInputFSM create(World world)
		{
			WorldInputFSM fsm = new WorldInputFSM();
			fsm.init(world);
			return fsm;
		}

		private void init(World world)
		{
			base.init();

			_world = world;

			createStates();
			start();
		}

		public override void start()
		{
			base.start();

			changeState(WorldInputStateType.wait);
		}

		private void createStates()
		{
			registerState<WorldInputState_Wait>();
			registerState<WorldInputState_TouchObject>();
			registerState<WorldInputState_PinchZoom>();
		}

		private void registerState<T>() where T : WorldInputStateBehaviour, new()
		{
			registerState(WorldInputStateBehaviour.create<T>(this));
		}
	}
}
