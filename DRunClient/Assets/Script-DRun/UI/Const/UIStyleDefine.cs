using Festa.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DRun.Client
{
	public static class UIStyleDefine
	{
		public static class ColorStyle
		{
			public static Color black = Color.black;
			public static Color gray900 = new Color(0.04f, 0.06f, 0.16f, 1.0f);
			public static Color gray850 = new Color(0.1f, 0.12f, 0.23f, 1.0f);
			public static Color gray800 = new Color(0.17f, 0.18f, 0.29f, 1.0f);
			public static Color gray700 = new Color(0.33f, 0.34f, 0.47f, 1.0f);
			public static Color gray500 = new Color(0.52f, 0.53f, 0.66f, 1.0f);
			public static Color gray400 = new Color(0.65f, 0.66f, 0.79f, 1.0f);
			public static Color gray300 = new Color(0.83f, 0.84f, 0.91f, 1.0f);
			public static Color gray200 = new Color(0.94f, 0.94f, 0.98f, 1.0f);
			public static Color white = Color.white;

			public static Gradient gradient;

			public static Color error = new Color(1f, 0.28f, 0.43f, 1.0f);
			public static Color success = new Color(0f, 1f, 0.22f, 1.0f);
			public static Color main = new Color(0.27f, 0.58f, 1.0f, 1.0f);

			public static Color sub_02 = new Color32(255, 15, 123, 255);
			public static Color sub_03 = new Color32(68, 149, 255, 255);
		}

		public static void init()
		{
			Gradient gradient = new Gradient();
			GradientColorKey[] colorKeys = new GradientColorKey[2];
			GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];

			colorKeys[0] = new GradientColorKey(new Color(0.27f, 0.58f, 1.0f, 1.0f), 0);
			colorKeys[1] = new GradientColorKey(new Color(0.69f, 0.08f, 1.0f, 1.0f), 1);

			alphaKeys[0] = new GradientAlphaKey(1, 0);
			alphaKeys[1] = new GradientAlphaKey(1, 1);

			gradient.SetKeys(colorKeys, alphaKeys);

			ColorStyle.gradient = gradient;
		}
	}
}
