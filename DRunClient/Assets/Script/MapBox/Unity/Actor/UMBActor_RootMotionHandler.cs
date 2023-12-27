using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class UMBActor_RootMotionHandler : MonoBehaviour
	{
		private Animator _animator;

		public void init()
		{
			_animator = GetComponent<Animator>();
		}

		void OnAnimatorMove()
		{
			
		}
	}
}
