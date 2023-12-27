//using Festa.Client.Module;
//using Festa.Client.NetData;
//using UnityEngine;
//using UnityEngine.UI;

//namespace Festa.Client
//{
//	public class UIMain_StepReward : ReusableMonoBehaviour
//	{
//		public Animator animator;
//		public Button button;
//		public Image image_bubble;

//		private RectTransform _rt;
//		private Vector2SmoothDamper _positionDamper;
//		private ClientStepReward _data;
//		private bool _claimed;

//		public ClientStepReward getData()
//		{
//			return _data;
//		}

//		public override void onCreated(ReusableMonoBehaviour source)
//		{
//			_rt = transform as RectTransform;
//			_positionDamper = Vector2SmoothDamper.create(Vector2.zero, 0.5f, 0.1f);
//		}

//		public override void onReused()
//		{
//		}

//		public void setup(ClientStepReward data,Vector2 start_pos,Vector2 target_pos)
//		{
//			_data = data;
//			_positionDamper.init(start_pos, 0.1f + Random.Range( 0, 0.25f), 2.0f);
//			_positionDamper.setTarget(target_pos);
//			_claimed = false;
//			image_bubble.gameObject.SetActive(false);


//			button.interactable = false;
//		}

//		public void OnEnable()
//		{
//			animator.Play("wait_touch", -1, Random.Range(0, 0.9f));
//		}

//		public void onClaimed(Vector2 target_pos)
//		{
//			_positionDamper.init(_positionDamper.getCurrent(), 0.1f, 2.0f);
//			_positionDamper.setTarget(target_pos);
//			_claimed = true;
//			button.interactable = false;

//			image_bubble.gameObject.SetActive(false);
//		}

//		//public void OnEnable()
//		//{
//		//	if( _isWaitTouch)
//		//	{
//		//		animator
//		//	}
//		//}

//		public void update()
//		{
//			if( _positionDamper.update())
//			{
//				_rt.anchoredPosition = _positionDamper.getCurrent();

//				if( _positionDamper.getCurrent() == _positionDamper.getTarget())
//				{
//					if( _claimed == false)
//					{
//						button.interactable = true;
//						image_bubble.gameObject.SetActive(true);
//					}
//					else
//					{
//						MainThreadDispatcher.dispatch(()=>{ 
//							UIMain.getInstance().onStepRewardClaimComplete(this);
//						});
//					}
//				}
//			}
//		}

//		public void onClick()
//		{
//			button.interactable = false;
//			UIMain.getInstance().onClickStepReward(this);
//		}
//	}
//}
