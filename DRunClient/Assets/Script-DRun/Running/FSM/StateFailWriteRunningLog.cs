using Festa.Client;
using Festa.Client.Module.FSM;
using Festa.Client.RefData;

namespace DRun.Client.Running
{
	public class StateFailWriteRunningLog : RecorderStateBehaviour
	{
		public override int getType()
		{
			return StateType.fail_write_running_log;
		}

		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			int resultCode = (int)param;
		
			if( resultCode == ResultCode.error_http_call_error)
			{
				confirmRetry();
			}
			else if( resultCode == ResultCode.error_already)	// 중복 요청은 조용히 넘어갈까?
			{
				returnToMain();
			}
			else
			{
				showErrorPopup();
			}
		}

		private void confirmRetry()
		{
			UIPopup.spawnRetry(StringCollection.get("pro.record.save_file.title", 0),
							 StringCollection.get("pro.record.save_fail.network_error", 0), () => {

								 _owner.changeState(StateType.write_running_log);
							 
			});
		}

		private void showErrorPopup()
		{
			UIPopup.spawnError(StringCollection.get("pro.record.save_file.title", 0),
				StringCollection.get("pro.record.save_fail.logic_error", 0), () =>
				{
					returnToMain();
				});
		}

		private void returnToMain()
		{
			UIRunningStatus.getInstance().close();
			UIHome.getInstance().open();
			UIMainTab.getInstance().open();

			_owner.changeState(StateType.none);
		}
	}
}
