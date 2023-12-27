using Festa.Client.RefData;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Festa.Client
{
    public class scrollGradientController : MonoBehaviour
	{
		[SerializeField]
		private ScrollRect scrollRect;
		[SerializeField]
		private int maxShowCount = 6;

		[SerializeField]
		private GameObject[] _gradient = new GameObject[2];

        private void LateUpdate()
        {
			// 스크롤 위치에 따라 그라뎅 조절~~
			if (scrollRect.content.childCount >= maxShowCount)
			{
				if (scrollRect.normalizedPosition.y >= 0.99f)
				{
					// 맨 위로 스크롤됨
					_gradient[0].gameObject.SetActive(false);
					_gradient[1].gameObject.SetActive(true);
				}
				else if (scrollRect.normalizedPosition.y <= 0.01f)
				{
					// 맨 아래로 스크롤됨
					_gradient[0].gameObject.SetActive(true);
					_gradient[1].gameObject.SetActive(false);
				}
				else
				{
					_gradient[0].gameObject.SetActive(true);
					_gradient[1].gameObject.SetActive(true);
				}
			}

            else
            {
				if (scrollRect.velocity.y > 0.01f)
				{
					_gradient[0].gameObject.SetActive(true);
					_gradient[1].gameObject.SetActive(true);
				}
				else
				{
					// 아무튼 스크롤 안 됨
					_gradient[0].gameObject.SetActive(false);
					_gradient[1].gameObject.SetActive(false);
				}
			}
        }
	}
}