using DRun.Client.Logic.Running;
using Festa.Client;
using Festa.Client.Module.FSM;
using UnityEngine;

namespace DRun.Client.Running
{
	public class StateWriteRunningLog : RecorderStateBehaviour
	{
		public override int getType()
		{
			return StateType.write_running_log;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			writeRunningLog();
		}

		private void writeRunningLog()
		{
			// 일단 화면 전체를 막아보자
			CanvasGroup canvasGroup = UIRunningResult.getInstance().GetComponent<CanvasGroup>();
			canvasGroup.interactable = false;

			//ClientRunningLog log = ViewModel.createRunningLog();
			//WriteRunningLogProcessor.NFTStatParam statData = ViewModel.createWriteLogStatParam();
			bool isMarathonMode = ViewModel.isMarathonMode();

			WriteRunningLogProcessor step = WriteRunningLogProcessor.create();
			step.run(result => {
				if (result.failed())
				{
					_owner.changeState(StateType.fail_write_running_log, step.getResultCode());	

					//UIPopup.spawnOK("#런닝 기록 전송 실패", () =>
					//{

					//	UIRunningStatus.getInstance().close();
					//	UIHome.getInstance().open();
					//	UIMainTab.getInstance().open();
					//});
				}
				else
				{
					UIRecord.getInstance().markAsRebuildData();

					if (isMarathonMode)
					{
						UIMarathonComplete.getInstance().open(step.getLog());
					}
					else
					{
						UIRunningResult.getInstance().open(step.getLog());
					}

					_owner.changeState(StateType.none);
				}
			});
		}

	}
}
