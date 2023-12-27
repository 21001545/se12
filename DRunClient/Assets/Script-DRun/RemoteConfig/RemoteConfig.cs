using Festa.Client.Module;
using Firebase.RemoteConfig;

namespace DRun.Client
{
	public class RemoteConfig
	{
		public static string force_update = "force_update";
		public static string force_update_message = "force_update_message";
		public static string store_url = "store_url";
		public static string server_list = "server_list";
		public static string show_server_list = "show_server_list";

		private static FirebaseRemoteConfig fbRemoteConfig => FirebaseRemoteConfig.DefaultInstance;

		public static string getString(string key)
		{
			return fbRemoteConfig.GetValue(key).StringValue;
		}

		public static bool getBoolean(string key)
		{
			return fbRemoteConfig.GetValue(key).BooleanValue;
		}

		public static JsonObject getJsonObject(string key)
		{
			return new JsonObject(fbRemoteConfig.GetValue(key).StringValue);
		}

		public static JsonArray getJsonArray(string key)
		{
			return new JsonArray(fbRemoteConfig.GetValue(key).StringValue);
		}
	}
}
