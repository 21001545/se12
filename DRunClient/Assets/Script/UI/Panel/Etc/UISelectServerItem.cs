using Festa.Client.Module;
using TMPro;
using UnityEngine.Events;

namespace Festa.Client
{
	public class UISelectServerItem : ReusableMonoBehaviour
	{
		public TMP_Text txt_button;

		private JsonObject _config;
		private UnityAction<JsonObject> _callback;

		public void setup(JsonObject config,UnityAction<JsonObject> onSelect)
		{
			_config = config;
			_callback = onSelect;

			txt_button.text = _config.getString("displayName");
		}

		public void onClick()
		{
			_callback(_config);
		}
	}
}
