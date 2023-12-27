using DRun.Client.Record;
using Festa.Client.Module;
using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
	public class UIRecordGraph_ValueItem : ReusableMonoBehaviour
	{
		public Image image;
		public RectTransform rt => (RectTransform)transform;

		public GraphValue graphValue;
	}
}
