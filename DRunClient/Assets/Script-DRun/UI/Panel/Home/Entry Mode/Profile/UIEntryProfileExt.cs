using System;
using System.Collections;
using DRun.Client.ViewModel;
using Festa.Client;
using Festa.Client.ViewModel;
using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
	public class UIEntryProfileExt : MonoBehaviour
	{
		[SerializeField]
		private UIEntryProfileNotification _notification;

		private IDisposable _unsub;

		private static ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		public void init()
		{
			_unsub = ViewModel.BasicMode.SubscribeToEntryExpDeferrer(
				new EntryExpChangeDeferObserver(_notification.text_notification_contents)
			);
		}
	}
}