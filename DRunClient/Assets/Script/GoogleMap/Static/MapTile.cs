using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Festa.GoogleMap.Static
{
	[RequireComponent(typeof(RawImage))]
	public class MapTile : MonoBehaviour
	{
		public double Longitude { get; set; } = 37.50634312319362;
		public double Latitude { get; set; } = 127.05441805010621;

		public int TileX = 0;
		public int TileY = 0;

		private RectTransform _rt;
		private RawImage _image;
		private bool _needUpdate = true;
		private MapQueryConfig _queryConfig;

		void Awake()
		{
			_rt = transform as RectTransform;
			_image = GetComponent<RawImage>();
			_queryConfig = new MapQueryConfig();
		}

		private void Update()
		{
			if( _needUpdate)
			{
				_needUpdate = false;
				updateImage();
			}
		}

		void prepareConfig()
		{
			//Vector3[] corners = new Vector3[4];
			//_rt.GetLocalCorners(corners);

			//float controlWidth = Mathf.Abs(corners[2].x - corners[0].x);
			//float controlHeight = Mathf.Abs(corners[2].y - corners[0].y);
			//float aspect = controlHeight / controlWidth;
			//int queryMapSizeX = 500;
			//int queryMapSizeY = (int)(queryMapSizeX * aspect);

			//_queryConfig.SizeX = queryMapSizeX;
			//_queryConfig.SizeY = queryMapSizeY;
			_queryConfig.SizeX = 128;
			_queryConfig.SizeY = 128;

			GoogleMapPosition pos = GoogleMapPosition.create(Longitude,Latitude)
				.moveByPixel(128 * TileX, 128 * TileY, _queryConfig.Zoom);

			_queryConfig.Longitude = pos.Longitude;
			_queryConfig.Latitude = pos.Latitude;
		}

		void updateImage()
		{
			prepareConfig();
			StartCoroutine(StaticMapTextureLoader.Load(_queryConfig, tex => { 
				if( tex != null)
				{
					_image.texture = tex;
				}
			}));
		}
	}
}

