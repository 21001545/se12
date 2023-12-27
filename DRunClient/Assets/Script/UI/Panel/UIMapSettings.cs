using Festa.Client.Module;
using Festa.Client.Module.UI;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Festa.Client
{
	public class UIMapSettings : UISingletonPanel<UIMapSettings>
	{
		public TMP_Dropdown dd_qmps;
		public TMP_Text txt_debug;

		//private AbstractLocationDevice device => ClientMain.instance.getLocation().getDevice();
		private static float[] _qmps_list = new float[]{ 
			1.0f,
			0.5f,
			1.1f,
			1.2f,
			1.3f,
			1.4f,
			1.5f,
			5.0f,
			10.0f
		};

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			List<string> option_list = new List<string>();
			foreach(float f in _qmps_list)
			{
				option_list.Add(f.ToString("F1"));
			}

			dd_qmps.AddOptions(option_list);
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			base.open(param, transitionType, closeType);

			LocationInfo li = Input.location.lastData;

			StringBuilder sb = new StringBuilder();

			sb.AppendFormat("lon:{0}\n", li.longitude);
			sb.AppendFormat("lat:{0}\n", li.latitude);
			sb.AppendFormat("alt:{0}\n", li.altitude);
			sb.AppendFormat("accuracy:{0},{1}\n", li.horizontalAccuracy, li.verticalAccuracy);
			sb.AppendFormat("timestamp:{0}\n", li.timestamp);

			txt_debug.text = sb.ToString();

			//float current_qmps = device.getFilterQMetersPerSecond();
			//for(int i = 0; i < _qmps_list.Length; ++i)
			//{
			//	if( _qmps_list[ i] == current_qmps)
			//	{
			//		dd_qmps.value = i;
			//		break;
			//	}
			//}
		}

		public void onQMPSChanged(int value)
		{
			TMP_Dropdown.OptionData data = dd_qmps.options[dd_qmps.value];
			float q = float.Parse(data.text);

			//device.setFilterQMetersPerSecond(q);
		}

		public void onClickOK()
		{
			close();
		}

	}
}
