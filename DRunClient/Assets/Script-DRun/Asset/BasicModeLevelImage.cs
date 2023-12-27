using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DRun.Client
{
	[CreateAssetMenu(fileName = "BasicModeLevelImage", menuName ="DRun/BasicModeLevelImage", order = 1)]
	public class BasicModeLevelImage : ScriptableObject
	{
		public Texture[] levelList;
	
		public Texture getImage(int level)
		{
			if (level < 0 || level > levelList.Length)
				return null;

			return levelList[level];
		}
	}
}
