using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Festa.Client.Module
{
	public abstract class SingletonBehaviour : MonoBehaviour
	{
		public abstract void initSingleton(SingletonInitializer initializer);
		public abstract void initSingletonPostProcess(SingletonInitializer initializer);
	}
}
