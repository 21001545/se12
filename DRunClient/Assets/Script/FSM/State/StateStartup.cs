using DRun.Client;
using Festa.Client.Module;
using Festa.Client.Module.FSM;
using UnityEngine;

namespace Festa.Client
{
	public class StateStartup : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.startup;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			loadBuildConfig();

			UIBackgroundBuilder.getInstance().build(() => {
				changeToNextState();
			});
		}

		private void loadBuildConfig()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("build_settings");
			GlobalConfig.buildConfig = new JsonObject(textAsset.text);
		}
	}
}
