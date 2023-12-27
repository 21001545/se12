using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client
{
	public static class ColorUtil
	{
		public static Color fromHSL(float h,float s,float l)
		{
			float r, g, b;
			if (s == 0)
			{
				r = g = b = l;
			}
			else
			{
				float v1, v2;

				v2 = (l < 0.5) ? (l * (1 + s)) : ((l + s) - (l * s));
				v1 = 2 * l - v2;

				r = HueToRGB(v1, v2, h + (1.0f / 3.0f));
				g = HueToRGB(v1, v2, h);
				b = HueToRGB(v1, v2, h - (1.0f / 3.0f));
			}

			return new Color(r, g, b, 1.0f);
		}

		private static float HueToRGB(float v1, float v2, float vH)
		{
			if (vH < 0)
				vH += 1;

			if (vH > 1)
				vH -= 1;

			if ((6 * vH) < 1)
				return (v1 + (v2 - v1) * 6 * vH);

			if ((2 * vH) < 1)
				return v2;

			if ((3 * vH) < 2)
				return (v1 + (v2 - v1) * ((2.0f / 3) - vH) * 6);

			return v1;
		}
	}
}
