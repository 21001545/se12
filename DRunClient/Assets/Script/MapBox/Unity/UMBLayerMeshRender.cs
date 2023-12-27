using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Festa.Client.MapBox
{
	public class UMBLayerMeshRender : ReusableMonoBehaviour
	{
		private Mesh _mesh;
		private MeshFilter _mf;
		private MeshRenderer _mr;

		public GameObject goSubMesh;
		public MeshFilter mfSubMesh;
		public MeshRenderer mrSubMesh;

		private UMBTile _ownerTile;
		private MBLayerRenderData _renderData;
		private MBStyleLayer _styleLayer;
		private UMBStyle _umbStyle;

		private Action<MBStyleExpressionContext> _handlerUpdateMaterialParam;
		private MBStyleExpression _exBackground;

		private Material _privateMaterial;

		public override void onCreated(ReusableMonoBehaviour source)
		{
			_mf = GetComponent<MeshFilter>();
			_mr = GetComponent<MeshRenderer>();

			_mesh = new Mesh();
			_mesh.name = string.Format("merged");
			_mesh.MarkDynamic();
			_privateMaterial = null;

			goSubMesh.SetActive(false);
		}

		public override void onReused()
		{
			goSubMesh.SetActive(false);

			_ownerTile = null;
			_renderData = null;
			_styleLayer = null;
			_handlerUpdateMaterialParam = null;

			if(_privateMaterial != null)
			{
				UnityEngine.Object.Destroy(_privateMaterial);
				_privateMaterial = null;
			}
		}

		public void setup(UMBTile owner,MBLayerRenderData renderData)
		{
			_ownerTile = owner;
			_renderData = renderData;
			_styleLayer = renderData.getLayerStyle();
			_umbStyle = owner.getMapBox().getUMBStyle();

			gameObject.name = _styleLayer.getID();
			gameObject.layer = _umbStyle.getLayer(_styleLayer);

			Material material = _umbStyle.getMaterial(_styleLayer);

			if( _styleLayer.getType() == MBStyleDefine.LayerType.fill_extrusion)
			{
				setupSubMesh(_umbStyle.getBuildingMaterialZOnly(), _umbStyle.getLayer3D_ZOnly());
			}
			else if( _styleLayer.getType() == MBStyleDefine.LayerType.fill)
			{
				material = new Material(material);
				_privateMaterial = material;

				_handlerUpdateMaterialParam = updateMaterialFill;
			}
			else if( _styleLayer.getType() == MBStyleDefine.LayerType.line)
			{
				material = new Material(material);
				_privateMaterial = material;

				_handlerUpdateMaterialParam = updateMaterialLine;
			}

			setupMesh(_renderData.getMergedMesh(), _styleLayer.getLayerOrder() + 10, material);
		}

		public void setupBackground(UMBTile owner,MBStyleExpression exBackground)
		{
			_ownerTile = owner;
			_handlerUpdateMaterialParam = updateMaterialBase;
			_umbStyle = owner.getMapBox().getUMBStyle();
			_exBackground = exBackground;

			gameObject.name = "base";
			gameObject.layer = _umbStyle.getLayer2D();

			_privateMaterial = new Material(_umbStyle.getBackgroundMaterial());

			setupMesh(MBMesh.getTileBaseMesh(), 0, _privateMaterial);
		}

		public void setupLandTile(UMBTile owner,MBMesh mesh)
		{
			_ownerTile = owner;
			_handlerUpdateMaterialParam = updateMaterialLandFill;
			_umbStyle = owner.getMapBox().getUMBStyle();

			gameObject.name = "landTileMesh";
			gameObject.layer = _umbStyle.getLayer2D();

			_privateMaterial = new Material(_umbStyle.getDefaultMaterial());

			setupMesh(mesh, 15, _privateMaterial);
		}

		public void setupLegacy(MBMesh mesh, int sorting_order,Material mat,string name,int layer)
		{
			setupMesh(mesh, sorting_order, mat);
			gameObject.name = name;
			gameObject.layer = layer;
		}

		private void updateMaterialBase(MBStyleExpressionContext ctx)
		{
			_privateMaterial.SetFloat(UMBStyle._id_stencil, _ownerTile.getStencilID());
			_privateMaterial.SetColor(UMBStyle._id_color, (Color)_exBackground.evaluate(ctx));
		}

		private void updateMaterialFill(MBStyleExpressionContext ctx)
		{
			_privateMaterial.SetFloat(UMBStyle._id_stencil, _ownerTile.getStencilID());
			_privateMaterial.SetFloat(UMBStyle._id_opacity, (float)_styleLayer.evaluateFillOpacity(ctx));
		}

		private void updateMaterialLine(MBStyleExpressionContext ctx)
		{
			_privateMaterial.SetFloat(UMBStyle._id_stencil, _ownerTile.getStencilID());
			_privateMaterial.SetFloat(UMBStyle._id_opacity, (float)_styleLayer.evaluateLineOpacity(ctx));
			_privateMaterial.SetFloat(UMBStyle._id_width, (float)_styleLayer.evaluateLineWidth(ctx));
			_privateMaterial.SetFloat(UMBStyle._id_gap_width, (float)_styleLayer.evaluateLineGapWidth(ctx));
		}

		private void updateMaterialLandFill(MBStyleExpressionContext ctx)
		{
			_privateMaterial.SetFloat(UMBStyle._id_stencil, _ownerTile.getStencilID());
			_privateMaterial.SetFloat(UMBStyle._id_opacity, 1.0f);
		}

		public void updateMaterialParams(MBStyleExpressionContext ctx)
		{
			if( _handlerUpdateMaterialParam != null)
			{
				_handlerUpdateMaterialParam(ctx);
			}
		}

		protected void setupMesh(MBMesh merged_mesh,int sorting_order,Material layer_mat)
		{
			_mesh.Clear();

			_mesh.SetVertices(merged_mesh.getVertexList());
			//_mesh.normals = merged_mesh.getNormalList().ToArray();
			_mesh.SetNormals(merged_mesh.getNormalList());
			_mesh.SetColors(merged_mesh.getColorList());
			_mesh.SetTriangles(merged_mesh.getIndexList(), 0);

			if( merged_mesh.getUVList().Count > 0)
			{
				_mesh.SetUVs(0,merged_mesh.getUVList());
			}
		
			_mesh.subMeshCount = 1;

			_mf.sharedMesh = _mesh;
			_mr.sharedMaterial = layer_mat;

			_mr.receiveShadows = false;
			_mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

			_mr.sortingOrder = sorting_order;
		}

		protected void setupSubMesh(Material mat,int layer)
		{
			goSubMesh.SetActive(true);
			goSubMesh.layer = layer;

			mfSubMesh.sharedMesh = _mesh;
			mrSubMesh.sharedMaterial = mat;

			mrSubMesh.receiveShadows = false;
			mrSubMesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

			mrSubMesh.sortingOrder = _mr.sortingOrder;
		}
	}
}
