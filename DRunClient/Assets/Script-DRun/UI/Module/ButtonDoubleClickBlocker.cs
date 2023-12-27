using System.Collections;
using System.Collections.Generic;

using Drun.Client;

using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
	public class ButtonDoubleClickBlocker
	{
		public class Single
		{
			public bool isBlocked { get; private set; }
			private readonly Button _button;

			public Single(Button button) => _button = button;

			public void block(float delay) => Yielder.run(unblock(delay));

			private IEnumerator unblock(float delay)
			{
				this.isBlocked = true;
				_button.interactable = false;

				yield return Yielder.seconds(delay);

				this.isBlocked = false;
				_button.interactable = true;
			}
		}

		public class Many
		{
			private readonly IEnumerable<Button> _buttons;
			public bool isBlocked { get; private set; }

			public Many(IEnumerable<Button> buttons) => _buttons = buttons;

			public void block(float delay) => Yielder.run(unblockAll(delay));

			private IEnumerator unblockAll(float delay)
			{
				this.isBlocked = true;

				foreach (var btn in _buttons)
					btn.interactable = false;

				yield return Yielder.seconds(delay);

				this.isBlocked = false;
				foreach (var btn in _buttons)
					btn.interactable = true;
			}
		}
	}
}