using DRun.Client.Logic.Wallet;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace DRun.Client
{
	public class UICreateWallet : UISingletonPanel<UICreateWallet>
	{
		public class PageType
		{
			public const int select_wallet = 0;
			public const int input_password = 1;
			public const int input_password_re = 2;
			public const int wait_create= 3;
			public const int complete_create = 4;
		}



		[Header("================= Main ===============")]
		public GameObject[] pages;
		public GameObject btn_back;

		[Header("========== Step0_Terms ===============")]
		public UISpriteToggleButton btn_agree;
		public UILoadingButton btn_agree_next;

		[Header("========== Step1_InputPassword =======")]
		public UIInputPinCode input_password;

		[Header("========== Step1_InputPassword =======")]
		public UIInputPinCode input_password_re;
		public GameObject go_mismatch;

		//--------------------------------------------------------------
		private UnityAction[] _setupPageHandlers;
		private int _currentPage;

		private List<int> _codeList;

		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			_setupPageHandlers = new UnityAction[pages.Length];
			_setupPageHandlers[0] = setupPage_SelectWallet;
			_setupPageHandlers[1] = setupPage_InputPassword;
			_setupPageHandlers[2] = setupPage_InputPasswordRe;
			_setupPageHandlers[3] = setupPage_WaitCreate;
			_setupPageHandlers[4] = setupPage_CompleteCreate;
			_currentPage = -1;
		}

		public override void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.start_open)
			{
				showPage(PageType.select_wallet);
			}
		}

		public void onClick_Back()
		{
			if( _currentPage == 0)
			{
				//close();
				UIWallet.getInstance().open();
			}
			else
			{
				showPage(_currentPage - 1);
			}
		}

		private void showPage(int page)
		{
			for(int i = 0; i < pages.Length; ++i)
			{
				pages[i].SetActive(i == page);
			}
			_setupPageHandlers[page]();
			_currentPage = page;
		}

		#region select_wallet
		private void setupPage_SelectWallet()
		{
			btn_back.gameObject.SetActive(true);
			btn_agree.setStatus(false);
			btn_agree_next.interactable = false;
		}

		public void onClick_Agree()
		{
			btn_agree.setStatus(!btn_agree.status);
			btn_agree_next.interactable = btn_agree.status;
		}

		public void onClick_ShowTerms()
		{
			string path = StringCollection.get("wallet.terms.url", 0);
			string url = GlobalConfig.fileserver_url + "/" + path;

			UIFullscreenWebView.spawnURL(url);
		}

		public void onClick_AgreeNext()
		{
			showPage(PageType.input_password);
		}

		#endregion

		#region input_password
		private void setupPage_InputPassword()
		{
			input_password.reset();

		}

		public void onValueChanged_InputPassword()
		{
			if( input_password.CodeList.Count == 6)
			{
				_codeList = new List<int>();
				_codeList.AddRange(input_password.CodeList);

				showPage(PageType.input_password_re);
			}
		}

		#endregion

		#region input_password_re
		private void setupPage_InputPasswordRe()
		{
			input_password_re.reset();
			go_mismatch.gameObject.SetActive(false);
		}

		public void onValueChanged_InputPasswordRe()
		{
			if( input_password_re.CodeList.Count == 6)
			{
				List<int> reCodeList = input_password_re.CodeList;
				bool check = true;
				for(int i = 0; i < 6; ++i)
				{
					if (reCodeList[ i] != _codeList[i])
					{
						check = false;
						break;
					}
				}

				if( check == false)
				{
					go_mismatch.gameObject.SetActive(true);
				}
				else
				{
					startCreate();
				}
			}
		}

		private void startCreate()
		{
			showPage(PageType.wait_create);

			StringBuilder sb = new StringBuilder();
			for(int i = 0; i < 6; ++i)
			{
				sb.Append(_codeList[i].ToString());
			}

			string pinHash = EncryptUtil.password(sb.ToString());

			CreateWalletProcessor step = CreateWalletProcessor.create(pinHash);
			step.run(result => { 
				if( result.failed())
				{
					UIPopup.spawnOK(StringCollection.get("wallet.create.fail", 0), () => {
						showPage(PageType.select_wallet);
					});
				}
				else
				{
					showPage(PageType.complete_create);
				}
			});
		}

		#endregion

		#region wait_create
		private void setupPage_WaitCreate()
		{
			btn_back.gameObject.SetActive(false);
		}

		#endregion
		private void setupPage_CompleteCreate()
		{
			btn_back.gameObject.SetActive(false);
		}

		public void onClick_OK()
		{
			UIWallet.getInstance().open();
			UIWallet.getInstance().setTab(UIWallet.TabType.wallet, true);
		}

		#region complete_create

		#endregion

	}
}
