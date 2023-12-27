//using Firebase.Auth;
//using TMPro;
//using UnityEngine;

//namespace Assets.Script.Test
//{
//	public class GoogleLoginTest : MonoBehaviour
//	{
//		public TMP_Text userInfo;
//		public TMP_InputField inputPhoneNumber;

//		public GameObject rootLogin;
//		public GameObject rootLogout;
		
//		public GameObject rootCode;
//		public UnityEngine.UI.Button btnRequestCode;
//		public UnityEngine.UI.Button btnSubmitCode;
//		public UnityEngine.UI.Button btnLogout;
//		public TMP_InputField inputCode;

//		private FirebaseAuth firebaseAuth;
//		private FirebaseUser firebaseUser;
//		private string verifyingID;
		
//		//private static uint timeoutMS = 1000 * 60 * 5;

//		private void Awake()
//		{
//			rootCode.SetActive(false);
//		}

//		private void Start()
//		{
//			firebaseAuth = FirebaseAuth.DefaultInstance;
//			firebaseAuth.StateChanged += AuthStateChanged;

//			updateUI();
//		}

//		void AuthStateChanged(object sender,System.EventArgs eventArgs)
//		{
//			if(firebaseAuth.CurrentUser != null)
//			{
//				bool signedIn = firebaseUser != firebaseAuth.CurrentUser && firebaseAuth.CurrentUser != null;
//				if (!signedIn && firebaseUser != null)
//				{
//					Debug.Log("Signed out " + firebaseUser.UserId);
//				}
//				firebaseUser = firebaseAuth.CurrentUser;
//				if (signedIn)
//				{
//					Debug.Log("Signed in " + firebaseUser.UserId);
//				}
//			}
//			updateUI();
//		}

//		void updateUI()
//		{
//			FirebaseUser user = firebaseAuth.CurrentUser;

//			if( user != null)
//			{
//				rootLogin.SetActive(false);
//				rootLogout.SetActive(true);

//				rootCode.SetActive(false);
//				btnRequestCode.interactable = true;
//				btnSubmitCode.interactable = true;

//				System.Text.StringBuilder sb = new System.Text.StringBuilder();
//				sb.AppendFormat("UserId:{0}\n", user.UserId);
//				sb.AppendFormat("ProviderId:{0}\n", user.ProviderId);
//				sb.AppendFormat("DisplayName:{0}\n", user.DisplayName);
//				sb.AppendFormat("PhoneNumber:{0}\n", user.PhoneNumber);

//				userInfo.text = sb.ToString();
//			}
//			else
//			{
//				rootLogin.SetActive(true);
//				rootLogout.SetActive(false);

//				btnLogout.interactable = true;

//				userInfo.text = "Logged out..";
//			}
//		}

//		private void OnDestroy()
//		{
//			firebaseAuth.StateChanged -= AuthStateChanged;
//		}

//		public void VerifyPhoneNumber()
//		{
//			btnRequestCode.interactable = false;

//			PhoneAuthProvider provider = PhoneAuthProvider.GetInstance(firebaseAuth);
//			provider.VerifyPhoneNumber(inputPhoneNumber.text, PhoneAuthProvider.MaxTimeoutMs, null, 
//				verificationCompleted:(credential)=> {
//					Debug.Log("verificationCompleted");
//				},
//				verificationFailed:(error)=> {
//					Debug.LogError(string.Format("veritifcationFailed:{0}", error));
//					btnRequestCode.interactable = true;
//				},
//				codeSent:(id,token)=> {
//					Debug.Log(string.Format("codeSent:id[{0}] token[{1}]", id, token));
//					rootCode.SetActive(true);
//					btnSubmitCode.interactable = true;

//					verifyingID = id;
//				},
//				codeAutoRetrievalTimeOut: (id) =>
//				{
//					Debug.LogWarning(string.Format("codeAutoRetrievalTimeOut:id[{0}]", id));
//				});
//		}

//		public void SubmitCode()
//		{
//			btnSubmitCode.interactable = false;

//			string code = inputCode.text;
//			PhoneAuthProvider provider = PhoneAuthProvider.GetInstance(FirebaseAuth.DefaultInstance);
//			Credential credential = provider.GetCredential(verifyingID, code);

//			firebaseAuth.SignInWithCredentialAsync(credential).ContinueWith(task => {
//				if (task.IsFaulted)
//				{
//					Debug.LogError("SignInWithCredentialAsync encountered an error: " +
//								   task.Exception);

//					rootCode.SetActive(false);
//					btnRequestCode.interactable = true;
//					return;
//				}

//				FirebaseUser newUser = task.Result;
//				Debug.Log("User signed in successfully");
//				// This should display the phone number.
//				Debug.Log("Phone number: " + newUser.PhoneNumber);
//				// The phone number providerID is 'phone'.
//				Debug.Log("Phone provider ID: " + newUser.ProviderId);
//			});
//		}

//		public void Logout()
//		{
//			btnLogout.interactable = false;
//			firebaseAuth.SignOut();
//		}
//	}
//}
