using DRun.Client.Logic.Record;
using Festa.Client;
using Festa.Client.Module.FSM;

namespace Assets.Script_DRun.FSM.State
{
	public class StateReadToayMarathonRecord : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.read_today_marathon_record;
		}

		public override void onEnter(StateBehaviour<object> prev_state,object param)
		{
			UILoading.getInstance().setProgress("read marathon record...", 97);

			ReadTodayMarathonRecordProcessor step = ReadTodayMarathonRecordProcessor.create();
			step.run(result => {
				changeToNextState();
			});
		}
	}
}
