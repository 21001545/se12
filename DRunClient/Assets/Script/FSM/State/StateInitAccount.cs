using Festa.Client.Module.FSM;
using Firebase.Auth;
using UnityEngine;

namespace Festa.Client
{
	public class StateInitAccount : ClientStateBehaviour
    {
        public override int getType()
        {
            return ClientStateType.init_account;
        }


        public override void onEnter(StateBehaviour<object> prev_state, object param)
        {
            UILoading.getInstance().setProgress("init account...", 50);

            Debug.Log($"StateInitAccount onEnter {_data.getStartupContext().is_first_run} {_data.getStartupContext().is_newaccount}");
            if (_data.getStartupContext().is_first_run)
            {
                UIFirebaseLogin.getInstance().setInitAccountDoneCallback(() => { changeToNextState(); });
                UIFirebaseLogin.getInstance().setRestartLoginCallback(() => {
                    
                    FirebaseAuth fbAuth = FirebaseAuth.DefaultInstance;
                    if(fbAuth.CurrentUser != null)
                        fbAuth.SignOut();
    
                    PlayerPrefs.DeleteAll();

                    changeToDesignatedState(ClientStateType.firebase_login);
                });

                // 요 과정 UI가 firebaseLogin에 있는게 맞는걸까..ㅋ_ㅋ
                if (_data.getStartupContext().is_newaccount)
                {
                    UIFirebaseLogin.getInstance().setStep(UIFirebaseLogin.StepType.Name);
                }
                else
                {
                    // 중복이 있네!?
                    UIFirebaseLogin.getInstance().setStep(UIFirebaseLogin.StepType.AlreadyHasAccount);
                }
            }
            else
            {
                // 회원가입 step
                int singup_step = ClientMain.instance.getViewModel().Profile.Signup.step;
                Debug.Log($"Signstep : {singup_step}");
                if (singup_step == UIFirebaseLogin.StepType.Done || singup_step == 0)
                {
                    // 바로 넘긴다.
                    changeToNextState();
                }
                else
                {
                    // 2022.05.04 이강희 signup_step이 꼬여서 여기서 전화번호 인증으로 가버림
                    // 일단 수정
                    if(singup_step < UIFirebaseLogin.StepType.Name)
                    {
                        singup_step = UIFirebaseLogin.StepType.Name;
                    }

                    UIFirebaseLogin.getInstance().open();
                    UIFirebaseLogin.getInstance().setStep(singup_step);
                    UIFirebaseLogin.getInstance().setInitAccountDoneCallback(() => { changeToNextState(); });
                }
            }
        }

        //public override void onExit(StateBehaviour<object> next_state)
        //{
        //    if (UILoading.getInstance().gameObject.activeSelf == false)
        //    {
        //        UILoading.getInstance().open();
        //    }
        //}

    }

}