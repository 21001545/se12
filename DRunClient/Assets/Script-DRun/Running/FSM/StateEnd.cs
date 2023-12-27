using DRun.Client.Logic.Running;
using Festa.Client;
using Festa.Client.Module.FSM;
using Festa.Client.Module.UI;
using UnityEngine;

namespace DRun.Client.Running
{

	public class StateEnd : RecorderStateBehaviour
	{
		public override int getType()
		{
			return StateType.end;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			// GPS끔
			GPSTracker.stop();

			// 걸음 수 다시 수집
			updatePathStepCount(ViewModel.CurrentPathData,() => {

				// 음..
				ViewModel.calcStatus();

				Recorder.saveLocalData(true);

				if (ViewModel.FirstMoveChecked == false)
				{
					UIToast.spawn(
							GlobalRefDataContainer.getStringCollection().get("pro.running.discard", 0),
							new(20, -606))
						.setType(UIToastType.error)
						.withTransition<FadePanelTransition>()
						.autoClose(3.0f);

					UIRunningStatus.getInstance().close();
					UIHome.getInstance().open();
					UIMainTab.getInstance().open();

					_owner.changeState(StateType.none);
				}
				else
				{
					_owner.changeState(StateType.write_running_log);
				}
			});
		}
	}
}
