using Festa.Client.Module;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.Demo
{
	public class CampingDemo : MonoBehaviour
	{
		public static CampingDemo instance = null;

		public Camera _worldCamera;
		public Camera		_uiCamera;
		public ChatBubble[] _bubbleSource;

		public Transform _bubbleRoot;

		public Transform[] _bubbleTarget;

		private const int type_female = 0;
		private const int type_male = 1;

		private ChatBubble[] _lastBubble;

		void Awake()
		{
			instance = this;
			_lastBubble = new ChatBubble[2];

			foreach(ChatBubble bubble in _bubbleSource)
			{
				bubble.gameObject.SetActive(false);
			}

			initSingletons();
		}

		private void initSingletons()
		{
			SingletonInitializer initializer = SingletonInitializer.create();
			initializer.run(transform);
		}

		void Start()
		{
			StartCoroutine(startDemo());
		}

		public void onClickLeaveParty()
		{
			GameObject go = Resources.Load<GameObject>("SceneTransition");
			GameObject new_go = GameObject.Instantiate(go);
			FadeSceneTransition transition = new_go.GetComponent<FadeSceneTransition>();
			transition.startTransition("Main");
		}

/*
남: How was your day?
여: so busy to work ..
여: :실망스럽지만_안도한:
남: Looks like you need a break.
여: yeah, definately.
*/
		IEnumerator startDemo()
		{
			yield return new WaitForSeconds(2.0f);

			spawnChatBubble(type_male, "How was your day?");

			yield return new WaitForSeconds(1.5f);

			spawnChatBubble(type_female, "so busy to work ..");

			yield return new WaitForSeconds(1.5f);

			spawnChatBubble(type_female, "<size=200%><sprite index=\"6\"></size>");

			yield return new WaitForSeconds(0.5f);
			_lastBubble[1].delete();
			_lastBubble[1] = null;

			yield return new WaitForSeconds(2.0f);

			spawnChatBubble(type_male, "Looks like you need a break.");

			yield return new WaitForSeconds(1.5f);

			spawnChatBubble(type_female, "yeah, definately.");

			yield return new WaitForSeconds(3.5f);

			_lastBubble[0].delete();
			_lastBubble[0] = null;
			_lastBubble[1].delete();
			_lastBubble[1] = null;

			StartCoroutine(startDemo());
		}

		void spawnChatBubble(int type,string message)
		{
			ChatBubble bubble = _bubbleSource[type].make<ChatBubble>(_bubbleRoot, GameObjectCacheType.actor);
			bubble.setup(_worldCamera, _bubbleTarget[type], _uiCamera, message);

			if( _lastBubble[ type] != null)
			{
				_lastBubble[type].delete();
			}
			_lastBubble[type] = bubble;
		}
	}
}
