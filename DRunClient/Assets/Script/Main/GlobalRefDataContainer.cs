using DRun.Client.RefData;
using Festa.Client.RefData;

namespace Festa.Client
{
	public class GlobalRefDataContainer
	{
		private static RefDataContainer _instance;
		private static System.Object _locker = new System.Object();

		private static RefDataHelper _helper;

		public static RefDataContainer getInstance()
		{
			lock(_locker)
			{
				return _instance;
			}
		}

		public static RefStringCollection getStringCollection()
		{
			lock(_locker)
			{ 
				if( _instance == null)
				{
					return null;
				}
				return _instance.getStringCollection();
			}
		}

		public static RefDataHelper getRefDataHelper()
		{
			return _helper;
		}

		public static void setInstance(RefDataContainer container)
		{
			lock(_locker)
			{
				_instance = container;
				_helper = RefDataHelper.create(container);
			}
		}

		//
		public static int getConfigInteger(int key,int def)
		{
			RefConfig config = _instance.get<RefConfig>(key);
			if( config == null)
			{
				return def;
			}
			else
			{
				return config.getInteger();
			}
		}

		public static double getConfigDouble(int key,double def)
		{
			RefConfig config = _instance.get<RefConfig>(key);
			if( config == null)
			{
				return def;
			}
			else
			{
				return config.getDouble();
			}
		}

		public static string getConfigString(int key,string def)
		{
			RefConfig config = _instance.get<RefConfig>(key);
			if (config == null)
			{
				return def;
			}
			else
			{
				return config.getString();
			}
		}
	}
}
