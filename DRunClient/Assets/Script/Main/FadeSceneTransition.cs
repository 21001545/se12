using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Festa.Client
{
	public class FadeSceneTransition : MonoBehaviour
	{
		public Image image;
		public float fade_in_time;
		public float fade_out_time;

		private string _level_name;
		private FloatSmoothDamper _damper;
		private int _step;

		public void startTransition(string level_name)
		{
			_level_name = level_name;
			_damper = FloatSmoothDamper.create(0.0f, fade_in_time);
			_damper.setTarget(1.0f);
			_step = 0;

			DontDestroyOnLoad(gameObject);
		}

		void Update()
		{
			if( _step == 0)
			{
				if( _damper.update())
				{
					image.color = new Color(0, 0, 0, _damper.getCurrent());
				}
				else
				{
					loadScene();

					_step = 1;
					_damper = FloatSmoothDamper.create(1.0f, fade_out_time);
					_damper.setTarget(0);
				}
			}
			else if( _step == 1)
			{
				if( _damper.update())
				{
					image.color = new Color(0, 0, 0, _damper.getCurrent());
				}
				else
				{
					GameObject.Destroy(gameObject);
				}
			}
		}

		void loadScene()
		{
			SceneManager.LoadScene( _level_name, LoadSceneMode.Single);
		}
	}
}
