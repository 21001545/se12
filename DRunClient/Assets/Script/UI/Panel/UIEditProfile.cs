using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using Festa.Client.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Festa.Client
{
	public class UIEditProfile : UISingletonPanel<UIEditProfile>
	{
		[Header("sticker")]
		[SerializeField]
		private RectTransform rect_stickerRoot;
		[SerializeField]
		private GameObject stickerPrefab;

		[Header("profile")]
		public UIPhotoThumbnail image_photo;
		public TMP_InputField input_message;

		//private bool _photo_changed;
		private string _new_photo_path;
		private int _upload_id;
		private int _agent_id;

		[Header("Step_Name")]
		#region name
		[SerializeField]
		private Image img_nameInputBg;
		[SerializeField]
		private Image img_checkMark;
		[SerializeField]
		private TMP_InputField input_name;
		[SerializeField]
		private TMP_Text txt_name_valid_status;
		[SerializeField]
		private TMP_Text txt_name_wordCount;
		[SerializeField]
		private GameObject go_clearName;
		[SerializeField]
		private Sprite[] checkMarks = new Sprite[3];    // 0 : check inactive, 1 : check active, 2 : x

		private Coroutine CachedValidateNameCoroutine;

		// 입력된 이름이 유효한가?
		private bool _isValidName = false;
		#endregion

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			base.open(param, transitionType, closeType);
			setUIColor_inputName(0);
			buildStickerBoard();
		}

		private void buildStickerBoard()
		{
			var jsonData = ClientMain.instance.getViewModel().Sticker.StickerBoard.getJsonData();

			int version = jsonData.getInteger("version");
			JsonArray stickerList = jsonData.getJsonArray("list");

			int currentStickers = rect_stickerRoot.transform.childCount;

			for (int i = 0; i < stickerList.size(); ++i)
			{
				JsonObject sticker = stickerList.getJsonObject(i);

				int type = sticker.getInteger("type");
				int id = sticker.getInteger("id");

				JsonArray vertices = sticker.getJsonArray("vertices");
				float position_x = (float)vertices.getDouble(0);
				float position_y = (float)vertices.getDouble(1);
				float rightBottomVertex_x = (float)vertices.getDouble(2);
				float rightBottomVertex_y = (float)vertices.getDouble(3);

				//// 만들기!!
				//StickerSizeController controller;
				//if (i < currentStickers)
				//{
				//	// 재활용~~
				//	rect_stickerRoot.GetChild(i).gameObject.SetActive(true);
				//	controller = rect_stickerRoot.GetChild(i).GetComponent<StickerSizeController>();
				//}
				//else
				//{
				//	GameObject stickerObj = Instantiate(stickerPrefab, rect_stickerRoot);
				//	controller = stickerObj.GetComponent<StickerSizeController>();
				//}

				//controller.setup(StickerState.OnBoard, id, i, position_x, position_y, rightBottomVertex_x, rightBottomVertex_y, rect_stickerRoot);

				//if (UIProfileBoard.getInstance().testIDs.Contains(id))
				//{
				//	int index = Array.IndexOf(UIProfileBoard.getInstance().testIDs, id);
				//	controller.setImage(UIProfileBoard.getInstance().testStickers[index].texture);
				//}
			}

			for (int i = stickerList.size(); i < currentStickers; ++i)
			{
				// 지우지 말고 꺼놓자~ 언제 다시 쓸지 몰라
				rect_stickerRoot.GetChild(i).gameObject.SetActive(false);
			}
		}

		public void resetForEdit()
		{
			ProfileViewModel vm = ViewModel.Profile;

			input_name.text = vm.Profile.name;
			input_message.text = vm.Profile.message;

			_new_photo_path = null;
			_upload_id = 0;
			_agent_id = 0;

			image_photo.setImageFromCDN(vm.Profile.getPicktureURL(GlobalConfig.fileserver_url));
		}

		public void onClickBackNavigation()
		{
			ClientMain.instance.getPanelNavigationStack().pop();
		}

		public void onClickEditBoard()
        {
			//UIProfileBoard.getInstance().open();
			//UIPanelNavigationStackItem stack = ClientMain.instance.getPanelNavigationStack().push(this, UIProfileBoard.getInstance());
		}

		public void onClickPhoto()
		{
			//UIBackNavigation.getInstance().setup(this, UIPhotoTake.getInstance(), UIMainTab.getInstance());
			//UIBackNavigation.getInstance().open();
			//UIPhotoTake.getInstance().setMode(UIPhotoTake.Mode.profile_edit);
			//UIPhotoTake.getInstance().open();
			UISelectPhoto.spawn((NativeGallery.NativePhotoContext context) => {
				if ( context != null )
					onSelectPhoto(new List<NativeGallery.NativePhotoContext> { context });
			});
		}

		public void onSelectPhoto(List<NativeGallery.NativePhotoContext> photoList)
		{
			if( photoList.Count > 0)
			{
				image_photo.setImageFromFile(photoList[ 0]);
				_new_photo_path = photoList[0].path;
			}
		}

		#region change name

		private void setUIColor_inputName(int i, string tempName = "")
		{
			// 색이랑 이미지만 바꿔줄 거야~~

			if (i == -1)
				img_checkMark.sprite = checkMarks[0];
			else
				img_checkMark.sprite = checkMarks[i];           // 체크마크 바꿔 넣고
			txt_name_valid_status.gameObject.SetActive(true);
			var sc = GlobalRefDataContainer.getInstance().getStringCollection();

			switch (i)
			{
				case -1: // 확인 중
					img_checkMark.sprite = checkMarks[0];
					txt_name_valid_status.color = ColorChart.gray_150;   // 에러메시지 부분 색 바꾼당
					txt_name_wordCount.color = ColorChart.gray_200;
					txt_name_valid_status.text = sc.get("signIn.username.check", 0);
					break;

				case 0: // 회색체크
						// 포커싱 여부는 업뎃에서 확인하므로! 여기서는 에러부분만 신경 써 본당
					img_checkMark.sprite = checkMarks[0];
					txt_name_wordCount.color = ColorChart.gray_150;
					txt_name_valid_status.gameObject.SetActive(false);
					break;

				case 1: // 초록체크
					img_checkMark.sprite = checkMarks[1];
					img_nameInputBg.color = ColorChart.success_100;
					txt_name_valid_status.color = ColorChart.success_300;
					txt_name_wordCount.color = ColorChart.success_300;
					txt_name_valid_status.text = sc.getFormat("signIn.username.canuse", 0, tempName);
					break;

				case 2: // 엑스자
					img_checkMark.sprite = checkMarks[2];
					img_nameInputBg.color = ColorChart.error_100;
					txt_name_valid_status.color = ColorChart.secondary_200;
					txt_name_wordCount.color = ColorChart.secondary_200;
					txt_name_valid_status.text = sc.getFormat("signIn.username.alreadyused", 0, tempName);
					break;
			}
		}

		// 이름 입력 했을 때, 
		// 이벤트 호출할때마다 네트워크에 요청하면 부하가 올테니, 최소 0.5초 입력을 안하면.. 네트워크에 보내도록 해보자.
		public void onInputNameChanged()
		{
			if(this.isActiveAndEnabled)
            {
				txt_name_wordCount.text = $"{input_name.text.Length}/{input_name.characterLimit}";

				if (CachedValidateNameCoroutine != null)
				{
					StopCoroutine(CachedValidateNameCoroutine);
					CachedValidateNameCoroutine = null;
				}

				_isValidName = false;

				if (input_name.text.Length > 0)
					CachedValidateNameCoroutine = StartCoroutine(ValidateNameCoroutine());
			}
		}

		private IEnumerator ValidateNameCoroutine()
		{
			setUIColor_inputName(-1);
			yield return new WaitForSeconds(0.5f);

			// 아직 중복 체크 API가 없다.
			// 무조건 되는 걸로..!

			string tempName = input_name.text;
			MapPacket req = Network.createReq(CSMessageID.Account.CheckNameReq);
			req.put("name", tempName);

			bool wait = true;
			Network.call(req, ack =>
			{
				// 에러가 나면 일단 사용은 못하게 하자. 중복이든.. 서버 에러든..
				// 서버에서 통과가 안되었을 테니..
				// ResultCode.error_username_already_exists
				_isValidName = ack.getResult() == ResultCode.ok;
				wait = false;
			});

			yield return new WaitWhile(() => { return wait; });

			if (_isValidName)
				setUIColor_inputName(1, tempName);
			else
				setUIColor_inputName(2, tempName);
		}

        public void Update()
        {
			if (!txt_name_valid_status.gameObject.activeSelf)
			{
				if (input_name.isFocused)
					img_nameInputBg.color = ColorChart.gray_500;
				else
					img_nameInputBg.color = ColorChart.gray_150;
			}

			if (input_name.isFocused && go_clearName.activeSelf == false)
				go_clearName.SetActive(true);
			else if(!input_name.isFocused && go_clearName.activeSelf == true)
				go_clearName.SetActive(false);
		}

		public void onClickClearInputField()
		{
			input_name.text = String.Empty;
			input_name.ActivateInputField();
		}

		#endregion

		public void onClickConfirm()
		{
			string name = input_name.text;
			string message = input_message.text;

			UIBlockingInput.getInstance().open();

			UnityAction close_ui = () => {
				UIBlockingInput.getInstance().close();
				//UIBackNavigation.getInstance().onClickBackButton();
			};

			checkName(check_name_result=>{ 
				if( check_name_result.failed())
				{
					UIBlockingInput.getInstance().close();
					UIPopup.spawnOK(StringCollection.get("signIn.username.alreadyused", 0), () => {
						input_name.text = ViewModel.Profile.Profile.name;
					});
					return;
				}

				uploadPhoto(upload_result => {

					if (upload_result.failed())
					{
						close_ui();
						return;
					}

					MapPacket req = Network.createReq(CSMessageID.Account.ModifyProfileReq);
					req.put("agent_id", _agent_id);
					req.put("agent_upload_id", _upload_id);
					req.put("name", name);
					req.put("message", message);

					Network.call(req, ack => {

						if (ack.getResult() == ResultCode.ok)
						{
							ViewModel.updateFromPacket(ack);
						}

						close_ui();
					});
				});
			});
		}

		private void checkName(Handler<AsyncResult<Module.Void>> handler)
		{
			// 같은 이름 이면 통과
			string new_name = input_name.text;
			if( ViewModel.Profile.Profile.name == new_name)
			{
				handler(Future.succeededFuture());
				return;
			}

			MapPacket req = Network.createReq(CSMessageID.Account.CheckNameReq);
			req.put("name", input_name.text);

			Network.call(req, ack =>{ 
				if( ack.getResult() == ResultCode.ok)
				{
					handler(Future.succeededFuture());
				}
				else
				{
					// 중복된 이름이다 (이미 누가 사용중이다)
					if( ack.getResult() == ResultCode.error_username_already_exists)
					{
						handler(Future.failedFuture(ack.makeErrorException()));
					}
					// 다른 에러
					else
					{
						handler(Future.failedFuture(ack.makeErrorException()));
					}
				}
			
			});
		}

		private void uploadPhoto(Handler<AsyncResult<Module.Void>> handler)
		{
			if( _new_photo_path == null)
			{
				handler(Future.succeededFuture());
				return;
			}


			List<string> photo_list = new List<string>();
			photo_list.Add(_new_photo_path);

			HttpFileUploader uploader = Network.createFileUploader(photo_list);
			uploader.run(ack => { 
				if (ack.getInteger("result") != ResultCode.ok)
				{
					handler(Future.failedFuture(new Exception("upload photo fail")));
				}
				else
				{
					_upload_id = ack.getInteger("id");
					_agent_id = ack.getInteger("agent_id");

					handler(Future.succeededFuture());
				}
			
			});
		}

		// test
		public void onClickWithdraw()
		{
			UIPopup.spawnYesNo("계정 탈퇴 하시겠습니까?", () => {

				// health, location정보 서버로 보내는거 멈춤
				ClientMain.instance.getFSM().changeState(ClientStateType.sleep);

				string firebase_id = ClientMain.instance.getData().getStartupContext().firebase_id;

				UIBlockingInput.getInstance().open();

				MapPacket req = Network.createReq(CSMessageID.Account.WithdrawReq);
				req.put("firebase_id", firebase_id);

				Network.call(req, ack =>
				{
					UIBlockingInput.getInstance().close();
					//this.close();
					UILoading.getInstance().open();

					UIPopup.spawnOK("계정 탈퇴되었습니다.", () => {

						UIBlockingInput.getInstance().close();

						if (ack.getResult() == ResultCode.ok)
						{
							UIMainTab.getInstance().close();
							ClientMain.instance.getFSM().changeState(ClientStateType.select_server);
						}

					});
				});
			});
		}
	}
}
