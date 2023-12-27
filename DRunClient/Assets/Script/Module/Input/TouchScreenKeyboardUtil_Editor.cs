using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.Module
{
	public class TouchScreenKeyboardUtil_Editor : TouchScreenKeyboardUtil
	{
		private bool _visible = false;
		private float _heightRatio = 0;

		public float HeightRatio
		{
			get
			{
				return _heightRatio;
			}
			set
			{
				_heightRatio = value;
			}

		}

		public override bool isVisible()
		{
			return _visible;
		}

		protected override float getTouchScreenKeyboardHeight()
		{
			if( _visible == false)
			{
				return 0;
			}
			else
			{
				return Screen.height * HeightRatio;
			}
		}

		public void setVisible(bool b)
		{
			_visible = b;
		}
	}
}