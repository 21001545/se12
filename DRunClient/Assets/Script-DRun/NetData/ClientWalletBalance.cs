namespace DRun.Client.NetData
{
	public class ClientWalletBalance
	{
		public int asset_type;
		public string total;
		public string liquid;

		public class AssetType
		{
			public static int ETH = 1;
			public static int DRNT = 2;
			public static int NFT = 3;

			// 클라 내부 처리용
			public static int NONE = -1;
		}

		public string getDisplayLiquid()
		{
			return liquid;
			//double value = double.Parse(liquid);
			//value = System.Math.Floor(value * 100000000.0) / 100000000.0;
			//return value.ToString();
		}
	}
}
