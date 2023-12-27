using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.Module
{
	public class SingletonInitializer
	{
		private List<SingletonBehaviour> _post_process_list;

		public static SingletonInitializer create()
		{
			SingletonInitializer initializer = new SingletonInitializer();
			initializer.init();
			return initializer;
		}

		private void init()
		{
			_post_process_list = new List<SingletonBehaviour>();
		}

		public void run(Transform root)
		{
			SingletonBehaviour[] singletons = root.GetComponentsInChildren<SingletonBehaviour>(true);
			for(int i = 0; i < singletons.Length; ++i)
			{
				singletons[ i].initSingleton( this);
			}

			for(int i = 0; i < _post_process_list.Count; ++i)
			{
				_post_process_list[ i].initSingletonPostProcess( this);
			}
		}

		public void addPostProcess(SingletonBehaviour s)
		{
			_post_process_list.Add( s);
		}
	}

}