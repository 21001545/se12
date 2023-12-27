using UnityEngine;
using UnityEditor;

namespace Festa.Client.Module
{
	public abstract class ReusableMonoBehaviour : MonoBehaviour
	{
		private bool _isInCache;

		public bool isInCache()
		{
			return _isInCache;
		}

		public void setInCache(bool b)
		{
			_isInCache = b;
		}

		public virtual void onCreated(ReusableMonoBehaviour source)
		{

		}

		public virtual void onReused()
		{

		}

		public virtual void onDelete()
		{

		}

		public T make<T>(Transform parent,int type) where T : ReusableMonoBehaviour
		{
			return GameObjectCache.getInstance().make((T)this, parent, type);
		}
	}

}
