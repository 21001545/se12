using Festa.Client.Module;
using Festa.Client.Module.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Festa.Client
{
	public abstract class WorldInputStateBehaviour : StateBehaviour<object>
	{
		protected World _world;
		protected AbstractInputModule _inputModule;
		protected Camera _camera;
		protected WorldCameraControl _cameraControl;

		public static WorldInputStateBehaviour create<T>(WorldInputFSM fsm) where T : WorldInputStateBehaviour, new()
		{
			T state = new T();
			state.init(fsm);
			return state;
		}

		protected virtual void init(WorldInputFSM fsm)
		{
			_world = fsm.getWorld();
			_inputModule = fsm.getWorld().getInputModule();
			_camera = Camera.main;
			_cameraControl = Camera.main.GetComponent<WorldCameraControl>();
		}
	}
}
