using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
	[RequireComponent(typeof(Animator))]
	public class UIAnimationToggleButton : Button
	{
		public bool status;

		private static int id_status = Animator.StringToHash("status");

		public void setStatus(bool s)
		{
			status = s;
			applyStatus();
		}

		private void applyStatus()
		{
			animator.SetFloat(id_status,status ? 1 : 0);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			applyStatus();
		}
	}
}
