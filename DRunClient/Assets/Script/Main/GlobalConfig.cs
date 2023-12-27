using Festa.Client.Module;
using UnityEngine;

namespace Festa.Client
{
	public static class GlobalConfig
	{
		public static string gameserver_url = "http://20.41.101.167:10100/packet";
		public static string fileserver_url = "http://20.41.101.167:10101/file";
		public static string fileupload_url = "http://20.41.101.167:10100/upload";

		public static JsonObject buildConfig = new JsonObject();

		public static void setupServerAddress(JsonObject server)
		{
			gameserver_url = server.getString("game_url");
			fileupload_url = server.getString("fileupload_url");
			fileserver_url = server.getString("file_url");
		}

		//
		public static bool isInhouseBranch()
		{
#if UNITY_EDITOR
			return true;
#else
			return buildConfig.getString("branch") == "inhouse";
#endif
		}

		//
		public static bool isOpenWalletFeature()
		{
			//return isInhouseBranch();
			return true;
		}

		public static bool isOpenInvitationCodeEvent()
		{
			//return isInhouseBranch();
			return true;
		}

		public static bool isOpenShareFeature()
		{
			//return isInhouseBranch();
			return true;
		}

		public static string getDeviceID()
		{
#if UNITY_EDITOR
			return SystemInfo.deviceUniqueIdentifier + EncryptUtil.makeHashCode(Application.dataPath).ToString();
#else
			return SystemInfo.deviceUniqueIdentifier;
#endif
		}
	}
}
