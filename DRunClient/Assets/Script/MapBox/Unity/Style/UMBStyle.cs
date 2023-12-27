using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class UMBStyle
	{
		private UMBStyleData _data;

		private Material _backgroundMaterial;
		private Material _defaultMaterial;
		private Material _defaultMaterialForHillShadeOpacity;
		private Material _buildingMaterial;
		private Material _buildingMaterialZOnly;
		private Material _lineMaterial;
		private Material _polylineMaterial;

		public static int _id_stencil = Shader.PropertyToID("_Stencil");
		public static int _id_extrudeRatio = Shader.PropertyToID("_ExtrudeRatio");
		public static int _id_screenMask = Shader.PropertyToID("ScreenMask");
		public static int _id_color = Shader.PropertyToID("_Color");
		public static int _id_opacity = Shader.PropertyToID("_Opacity");
		public static int _id_width = Shader.PropertyToID("_Width");
		public static int _id_offset = Shader.PropertyToID("_Offset");
		public static int _id_gap_width = Shader.PropertyToID("_GapWidth");
		public static int _id_device_pixel_ratio = Shader.PropertyToID("_DevicePixelRatio");
		public static int _id_u_ratio = Shader.PropertyToID("_URatio");

		private int _layer3D;
		private int _layer3D_ZOnly;
		private int _layer2D;

		public Material getBuildingMaterial()
		{
			return _buildingMaterial;
		}

		public Material getBuildingMaterialZOnly()
		{
			return _buildingMaterialZOnly;
		}

		public Material getBackgroundMaterial()
		{
			return _backgroundMaterial;
		}

		public Material getDefaultMaterial()
		{
			return _defaultMaterial;
		}

		public Material getHillshadeMaterial()
		{
			return _defaultMaterialForHillShadeOpacity;
		}

		public Material getLineMaterial()
		{
			return _lineMaterial;
		}
		public Material getPolylineMaterial()
		{
			return _polylineMaterial;
		}

		public static UMBStyle create(UMBStyleData data)
		{
			UMBStyle s = new UMBStyle();
			s.init(data);
			return s;
		}

		private void init(UMBStyleData data)
		{
			_data = data;

			_backgroundMaterial = new Material(data.matBackground);
			_buildingMaterial = new Material(data.matBuilding);
			_buildingMaterialZOnly = new Material(data.matBuildingZOnly);
			_defaultMaterial = new Material(data.matDefault);
			_defaultMaterialForHillShadeOpacity = new Material(data.matDefault);
			_lineMaterial = new Material(data.matLine);
			_polylineMaterial = new Material(data.matPolyline);

			_layer2D = LayerMask.NameToLayer("MapBox_2DBase");
			_layer3D = LayerMask.NameToLayer("MapBox_3D");
			_layer3D_ZOnly = LayerMask.NameToLayer("MapBox_3D_ZOnly");
		}

		public Material getMaterial(MBStyleLayer layer)
		{
			if (layer.getType() == MBStyleDefine.LayerType.fill_extrusion)
			{
				return _buildingMaterial;
			}
			else if (layer.getType() == MBStyleDefine.LayerType.line)
			{
				return _lineMaterial;
			}
			else if (layer.getIDHashCode() == MBLayerRenderData.id_hillShade)
			{
				return _defaultMaterialForHillShadeOpacity;
			}
			else
			{
				return _defaultMaterial;
			}
		}

		public int getLayer(MBStyleLayer layer)
		{
			if( layer.getType() == MBStyleDefine.LayerType.fill_extrusion)
			{
				return _layer3D;
			}
			else
			{
				return _layer2D;
			}
		}

		public int getLayer2D()
		{
			return _layer2D;
		}

		public int getLayer3D_ZOnly()
		{
			return _layer3D_ZOnly;
		}

		public void setExtrudeRatio(float ratio)
		{
			_buildingMaterial.SetFloat(_id_extrudeRatio, ratio);
			_buildingMaterialZOnly.SetFloat(_id_extrudeRatio, ratio);
		}

		public UMBFontSource.FontSource getFontSource(int font)
		{
			return _data.fontSource.getSource(font);
		}

		public void setScreenMask(Vector4 v)
		{
			_buildingMaterial.SetVector(_id_screenMask, v);
			_defaultMaterial.SetVector(_id_screenMask, v);
			_defaultMaterialForHillShadeOpacity.SetVector(_id_screenMask, v);
			_backgroundMaterial.SetVector(_id_screenMask, v);
		}

		//public void setHillShadeOpacity(float opacity)
		//{
		//	_defaultMaterialForHillShadeOpacity.SetFloat(_id_opacity, opacity);
		//}

		//public void setBackgroundColor(Color c)
		//{
		//	_backgroundMaterial.SetColor(_id_color, c);
		//}
	}
}
