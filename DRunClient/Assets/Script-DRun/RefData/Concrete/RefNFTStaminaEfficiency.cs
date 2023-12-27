namespace DRun.Client.RefData
{
	public class RefNFTStaminaEfficiency : Festa.Client.RefData.RefData
	{
		public int percent_begin;
		public bool equal_begin;
		public int percent_end;
		public bool equal_end;
		public int efficiency_DRN;

		public bool check(int value)
		{
			if( equal_begin == true && value < percent_begin)
			{
				return false;
			}
			else if( equal_begin == false && value <= percent_begin)
			{
				return false;
			}

			if( equal_end == true && value > percent_end)
			{
				return false;
			}
			else if( equal_end == false && value >= percent_end)
			{
				return false;
			}

			return true;

		}
	}
}
