using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.Module.UI
{
	public static class UIUtil
	{
		public static object convertCurrencyString(object obj)
		{
			int value = (int)obj;
			return value.ToString("N0");
		}

		public static object convertFestaCoinString(object obj)
		{
			return $"<sprite name=\"coin\">{convertCurrencyString(obj)}";
		}
	}
}
