using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.MapBox
{
	[CreateAssetMenu(fileName = "MapBoxStyle", menuName = "Festa/MapBox/MapBoxStyle", order = 1)]
	public class UMBStyleData : ScriptableObject
	{
		public Material matBackground;
		public Material matDefault;
		public Material matBuilding;
		public Material matBuildingZOnly;
		public Material matLine;
		public Material matPolyline;

		//public UMBMakiSpriteContainer makiSpriteContainer;
		public UMBFontSource fontSource;
		public UMBActorSourceContainer actorSourceContainer;
		public UMBMapDecoSourceContainer mapDecoSourceContainer;
		//public TextAsset mbStyleSource;
	}
}
