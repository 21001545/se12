using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Festa.Client.EditorTool
{
	public class ColorConversion : EditorWindow
	{
		[MenuItem("Window/Festa/ColorConversion")]
		static void init()
		{
			ColorConversion window = EditorWindow.GetWindow<ColorConversion>();
			window.Show();
		}

		private float _h = 0;
		private float _s = 100;
		private float _l = 100;

		private Color _color;
		private Color _color_linear;

		private void OnGUI()
		{
			_h = EditorGUILayout.FloatField("H", _h);
			_s = EditorGUILayout.FloatField("S", _s);
			_l = EditorGUILayout.FloatField("L", _l);

			if( GUILayout.Button("toRGB"))
			{
				_color = ColorUtil.fromHSL(_h / 360.0f, _s / 100.0f, _l / 100.0f);
				_color_linear = _color.linear;
			}

			_color = EditorGUILayout.ColorField("RGB", _color);

			EditorGUILayout.LabelField($"{(int)(_color.r * 255.0f)},{(int)(_color.g * 255.0f)},{(int)(_color.b * 255.0f)}");

			_color_linear = EditorGUILayout.ColorField("RGB(linear)", _color_linear);

			EditorGUILayout.LabelField($"{(int)(_color_linear.r * 255.0f)},{(int)(_color_linear.g * 255.0f)},{(int)(_color_linear.b * 255.0f)}");

			for(int i = 1; i < 16; ++i)
			{
				float y = Mathf.Pow(2, i);
				float x = Mathf.Log(y, 2);

				EditorGUILayout.LabelField($"{y},{x}");
			}

		}
	}
}
