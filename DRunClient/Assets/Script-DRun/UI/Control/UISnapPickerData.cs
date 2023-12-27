using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

namespace DRun.Client
{
	public abstract class UISnapPickerData
	{
		public abstract int getValue();
		public abstract string getText();
		public abstract HorizontalAlignmentOptions getTextHorizontalAlign();
	}
}
