using Festa.Client.Module;

namespace Festa.Client.MapBox
{
	public static class MBPropertyKey
	{
		public static int name = EncryptUtil.makeHashCode("name");
		public static int name_ko = EncryptUtil.makeHashCode("name_ko");
		public static int class_key = EncryptUtil.makeHashCode("class");
		public static int extrude = EncryptUtil.makeHashCode("extrude");
		public static int height = EncryptUtil.makeHashCode("height");
		public static int min_height = EncryptUtil.makeHashCode("min_height");
		public static int maki = EncryptUtil.makeHashCode("maki");
		public static int mapbox_clip_start = EncryptUtil.makeHashCode("mapbox_clip_start");
		public static int mapbox_clip_end = EncryptUtil.makeHashCode("mapbox_clip_end");
	}

}
