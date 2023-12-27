using Festa.Client.Module.FSM;
using UnityEngine;

namespace Festa.Client
{
	public class StateLoadRefData : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.load_refdata;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			UILoading.getInstance().setProgress("load refdata...", 31);

			startLoading();
		}

		private void startLoading()
		{
			GlobalRefDataLoader loader = GlobalRefDataLoader.create(GlobalConfig.fileserver_url);

			loader.run(result => {

				if (result)
				{
					setupLanguage();
					changeToNextState();
				}
				else
				{
					UIPopup.spawnOK("load refdata fail", () => {

						// 서버를 잘못 선택하는 경우가 많아서 서버 선택으로 보내자
						//startLoading();

						_owner.changeState(ClientStateType.select_server);
					});
				}

			});
		}

		private void setupLanguage()
		{
			int savedType = PlayerPrefs.GetInt("languageType", -1);
			if( savedType == -1)
			{
				savedType = LanguageType.getLanguageTypeFromDevice();
			}

			GlobalRefDataContainer.getStringCollection().setCurrentLangType(savedType);
		}
	}
}
