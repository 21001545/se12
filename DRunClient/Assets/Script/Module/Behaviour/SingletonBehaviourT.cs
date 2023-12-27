using UnityEngine;
using UnityEditor;

namespace Festa.Client.Module
{
	public class SingletonBehaviourT<T> : SingletonBehaviour where T : SingletonBehaviour
	{
		protected static SingletonBehaviour _instance;

		public override void initSingleton(SingletonInitializer initializer)
		{
			_instance = this;
		}

		public override void initSingletonPostProcess(SingletonInitializer initializer)
		{
			Debug.LogError("not implemented",gameObject);
		}

		public static T getInstance()
		{
			return (T)_instance;
		}
	}
}

