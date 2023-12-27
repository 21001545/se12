namespace DRun.Client.Running
{
	// 서버 로그인 이후 최초 실행
	// 이전 실행에 런닝중이었을 경우에 대한 처리
	public class StateInit : RecorderStateBehaviour
	{
		public override int getType()
		{
			return StateType.init;
		}
	}
}
