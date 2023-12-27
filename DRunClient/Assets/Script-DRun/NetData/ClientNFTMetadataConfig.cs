namespace DRun.Client.NetData
{
	public class ClientNFTMetadataConfig
	{
		public string baseURL;
		public int tokenIDOffset;

		public string makeURL(int token_id)
		{
			return $"{baseURL}/{tokenIDOffset + token_id}";
		}
	}
}
