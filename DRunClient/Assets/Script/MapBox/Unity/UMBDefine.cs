using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public static class UMBDefine
	{
		public class CurrentLocationMode
		{
			public const int none = 0;
			public const int follow = 1;
			public const int follow_fitzoom = 2;	// 탐험 경로를 한꺼번에 보여주는 모드
		}

		public class ProjectionMode
		{
			public const int two_d = 0;
			public const int three_d = 1;
		}

		public static Vector3[] tileBoundVerts = new Vector3[4]
		{
			new Vector3( 0, 0, 0),
			new Vector3( 0,  -MapBoxDefine.tile_extent, 0),
			new Vector3( MapBoxDefine.tile_extent,  -MapBoxDefine.tile_extent, 0),
			new Vector3( MapBoxDefine.tile_extent, 0, 0)
		};
	}
}
