using Festa.Client.Module.FSM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Messaging;
using Firebase.Extensions;
using UnityEngine;

namespace Festa.Client
{
	public class StateFirebasePush : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.firebase_push;
		}

        public override void onEnter(StateBehaviour<object> prev_state, object param)
        {
            UILoading.getInstance().setProgress("init firebase push...", 100);

            // 소현 : 노티 스킵 여부에 따라 다르게 진입
            if (!UnityEngine.PlayerPrefs.HasKey("isNotiSkipped"))
                UnityEngine.PlayerPrefs.SetInt("isNotiSkipped", 0);

            if (UnityEngine.PlayerPrefs.GetInt("isNotiSkipped") == 0)
                // 스킵 안하기로 했다면
                getNotiPermission();
            else
                changeToNextState();
        }

        public void getNotiPermission()
        {
            // 2022.06.08 이강희 에러 로그를 없애보자
            ClientMain.instance.getPushManager().start();

            ClientMain.instance.StartCoroutine(_requestPermission());

        }

        IEnumerator _requestPermission()
        {
            yield return new WaitForSeconds(0.1f);

            UIBlockingInput.getInstance().open();
            FirebaseMessaging.RequestPermissionAsync().ContinueWithOnMainThread(task =>
            {
                UIBlockingInput.getInstance().close();
                changeToNextState();
            });
        }
    }
}
