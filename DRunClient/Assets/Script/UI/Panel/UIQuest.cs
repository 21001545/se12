using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Festa.Client.Module.UI;
using UnityEngine.SceneManagement;

namespace Festa.Client
{
	public class UIQuest : UISingletonPanel<UIQuest>
	{
		public void onClickCampAR()
		{
			SceneManager.LoadScene("CampAR", LoadSceneMode.Single);
		}

		public void onClickFaceAR()
		{
			SceneManager.LoadScene("FaceAR", LoadSceneMode.Single);
		}

		public void onClickColorCustomizing()
		{
			SceneManager.LoadScene("CustomizeShowcase", LoadSceneMode.Single);
		}
	}
}
