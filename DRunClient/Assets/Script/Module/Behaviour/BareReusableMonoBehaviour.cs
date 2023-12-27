using UnityEngine;

namespace Festa.Client.Module
{
	public class BareReusableMonoBehaviour : ReusableMonoBehaviour
	{
		public RectTransform rt => (RectTransform)transform;
	}
}
