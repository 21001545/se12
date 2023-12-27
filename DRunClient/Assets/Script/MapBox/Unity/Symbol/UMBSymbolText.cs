using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;

namespace Festa.Client.MapBox
{
	public class UMBSymbolText : TextMeshPro
	{
		private UMBSymbolRenderer _owner;
		private UMBSymbolPlacement _placement;
		private UMBSymbolPoint _point;
		private RectTransform _tileRoot;
		private UMBControl _control;
		private MBTileCoordinateDouble _pointTilePos;
		private List<Collider> _colliderList;
		private bool _isAlignToLine;

		private TMP_MeshInfo[] _cachedMeshInfo;
		private bool _isTouchOutlineEdge;
		private AABBTree _tree => _placement.getLabelManager().getTree();

		private static CustomSampler _profileSampler = CustomSampler.Create("UMBSymbolText.alignToLine");

		public bool isTouchOutlineEdge()
		{
			return _isTouchOutlineEdge;
		}

		public void setAlignToLine(bool enable)
		{
			_isAlignToLine = enable;
		}

		public void onCreated()
		{
			_colliderList = new List<Collider>();
		}

		public void init(UMBSymbolRenderer owner)
		{
			_owner = owner;
			_placement = owner.getPlacement();
			_point = owner.getSymbolPoint();
			_control = _owner.getPlacement().getControl();
			_tileRoot = _owner.getPlacement().getControl().getTileRoot();
			_pointTilePos = _point.getTilePos();
			
			_isTouchOutlineEdge = false;
		}

		public void delete()
		{
			_colliderList.Clear();
		}

		// alpha fade처리 때문에 여러번 호출되는 이슈가 있네

		protected override void GenerateTextMesh()
		{
			base.GenerateTextMesh();

			//Debug.Log($"[{Time.frameCount}] generate text mesh", gameObject);

			if (_owner != null && _isAlignToLine)
			{
				_cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
				_profileSampler.Begin();
				alignToLine();
				_profileSampler.End();
			}
		}

		public Vector2Int meshToAnchorPos(Vector3 pos)
		{
			MBTileCoordinateDouble tilePos = _control.worldToTilePos(transform.TransformPoint(pos), _point.getTilePos().zoom);
			return _placement.localPosFromTilePos(tilePos);
		}

		public Vector3 anchorToMeshPos(Vector2 pos)
		{
			MBTileCoordinateDouble tilePos = _placement.tilePosFromLocalPos(pos);
			Vector3 worldPos = _control.tilePosToWorldPosition(tilePos);
			return transform.InverseTransformPoint(worldPos);
		}

		public void updatePosition()
		{
			//if (_owner.getPlacement().getFeatureID() == 4177339071)
			//{
			//	int a = 0;
			//}

			if (m_havePropertiesChanged == true)
			{
				return;
			}
			if (_isAlignToLine)
			{
//				ForceMeshUpdate();
				alignToLine();
			}
		}

		private bool checkCachedMeshValid()
		{
			if (_cachedMeshInfo == null)
			{
				return false;
			}

			if( textInfo.meshInfo.Length != _cachedMeshInfo.Length)
			{
				return false;
			}

			for(int i = 0; i < textInfo.meshInfo.Length; ++i)
			{
				if(textInfo.meshInfo[i].vertices.Length != _cachedMeshInfo[i].vertices.Length)
				{
					return false;
				}
			}

			return true;
		}

		private void updateColliders()
		{
			int characterCount = textInfo.characterCount;
			float text_begin = textInfo.characterInfo[0].vertex_TL.position.x;
			float text_end = textInfo.characterInfo[characterCount - 1].vertex_TR.position.x;

			//float thickness = fontSize / 10.0f;
			float thickness = this.preferredHeight;
			float total_distance = (text_end - text_begin);
			int collider_count = Mathf.CeilToInt((total_distance/* - thickness*/) / thickness);

			float offset = _owner.getSymbolPoint().getAnchorCursor().getOffset();
			MBAnchors anchors = _owner.getSymbolPoint().getAnchorCursor().getAnchors();
			MBAnchorCursor cursor = null;

			//
			Vector2Int center = meshToAnchorPos(Vector3.zero);
			Vector2Int begin = meshToAnchorPos(new Vector3(text_begin + 0 * thickness + thickness / 2.0f, 0));
			Vector2Int end_pos = meshToAnchorPos(new Vector3(text_begin + (collider_count - 1) * thickness + thickness / 2.0f, 0));

			float offset_begin = (offset - (begin - center).magnitude);
			float offset_end = offset_begin + (end_pos - begin).magnitude;

			if (offset_begin < 0 || offset_end > anchors.getLength())
			{
				//prepareCollider(0);
				_isTouchOutlineEdge = true;

				foreach(Collider collider in _colliderList)
				{
					_owner.getColliderList().Remove(collider);
					_tree.removeObject(collider);
				}
				_colliderList.Clear();

				return;
			}

			_isTouchOutlineEdge = false;

			//prepareCollider(collider_count);

			Vector2Int last_pos = Vector2Int.zero;

			for (int i = 0; i < collider_count; ++i)
			{
				if (i == 0)
				{
					center = meshToAnchorPos(Vector3.zero);
					begin = meshToAnchorPos(new Vector3(text_begin + i * thickness + thickness / 2.0f, 0));

					cursor = anchors.getCursor(offset - (begin - center).magnitude);

					last_pos = begin;
				}
				else
				{
					Vector2Int curAnchorPos = meshToAnchorPos(new Vector3(text_begin + i * thickness + thickness / 2.0f, 0));

					cursor.moveForward((curAnchorPos - last_pos).magnitude);

					last_pos = curAnchorPos;

				}

				float radius = thickness / 2.0f;
				Vector3 colliderPosition = _owner.transform.localPosition + transform.localPosition + anchorToMeshPos(cursor.getPosition());

				if( i < _colliderList.Count)
				{
					Collider collider = _colliderList[i];
					CircleShape circleShape = collider.getShape() as CircleShape;

					circleShape.setRadius(radius);
					collider.setPosition(colliderPosition);

					_tree.updateObject(collider);
				}
				else
				{
					Shape shape = CircleShape.create(radius);
					Collider collider = Collider.create(MapBoxDefine.ColliderType.text_along_line, _owner.GetInstanceID(), _owner.getCollisionOrder(), shape, colliderPosition);

					_tree.insertObject(collider);
					_owner.getColliderList().Add(collider);
					_colliderList.Add(collider);
				}
			}

			// 
			int delete_count = _colliderList.Count - collider_count;
			for(int i = 0; i < delete_count; ++i)
			{
				Collider collider = _colliderList[i + collider_count];

				_owner.getColliderList().Remove(collider);
				_tree.removeObject(collider);
				_colliderList.Remove(collider);
			}
		}

		private void alignToLine()
		{
			TMP_CharacterInfo[] characterInfo = textInfo.characterInfo;
			int count = textInfo.characterCount;
			if( count == 0)
			{
				return;
			}

			// check cache validation
			if( checkCachedMeshValid() == false)
			{
				return;
			}

			updateColliders();

			if( color.a == 0)
			{
				return;
			}

			//if( _placement.getFeatureID() == UMBSymbolPlacementLine.test_id)
			//{
			//	int a = 0;
			//}

			Vector3[] vertices;
			Vector3[] vertices_source;
			Matrix4x4 matrix;

			Vector2Int lastAnchorPos = Vector2Int.zero;

			MBAnchors anchors = _owner.getSymbolPoint().getAnchorCursor().getAnchors();
			float offset = _owner.getSymbolPoint().getAnchorCursor().getOffset();
			MBAnchorCursor cursor = null;

			float control_angle = _control.getRotateRoot().rotation.eulerAngles.z;
			Vector2 cursor_direction = _owner.getSymbolPoint().getAnchorCursor().getDirection();
			Vector2 world_direction = _control.getRotateRoot().TransformDirection( new Vector2( cursor_direction.x, -cursor_direction.y));

			int orientation;
			bool cursor_backward;

			float mx = world_direction.x;
			float my = world_direction.y;

			if( Mathf.Abs(mx) > Mathf.Abs(my))
			{
				orientation = 0;

				if( mx < 0)
				{
					cursor_backward = true;
				}
				else
				{
					cursor_backward = false;
				}
			}
			else
			{
				orientation = 1;

				if( my > 0)
				{
					cursor_backward = true;
				}
				else
				{
					cursor_backward = false;
				}
			}

			float verticleBeginOffset = m_text.Length * fontSize / 2.0f;

			for (int i = 0; i < count; ++i)
			{
				if (!characterInfo[i].isVisible)
				{
					continue;
				}

				int vertexIndex = characterInfo[i].vertexIndex;
				int materialIndex = characterInfo[i].materialReferenceIndex;
				vertices = textInfo.meshInfo[materialIndex].vertices;
				vertices_source = _cachedMeshInfo[materialIndex].vertices;

				Vector3 offsetToMidBaseline = new Vector2((vertices_source[vertexIndex + 0].x + vertices_source[vertexIndex + 2].x) / 2, characterInfo[i].baseLine);

				if (i == 0)
				{
					Vector2Int centerAnchorPos = meshToAnchorPos(Vector3.zero);
					Vector2Int beginAnchorPos;
					
					if( orientation == 0)
					{
						beginAnchorPos = meshToAnchorPos(new Vector3(offsetToMidBaseline.x, 0, 0));
					}
					else
					{
						beginAnchorPos = meshToAnchorPos(new Vector3(0, fontSize * i - verticleBeginOffset, 0));
					}

					if (cursor_backward)
					{
						cursor = anchors.getCursor(offset + (beginAnchorPos - centerAnchorPos).magnitude);
					}
					else
					{
						cursor = anchors.getCursor(offset - (beginAnchorPos - centerAnchorPos).magnitude);
					}

					lastAnchorPos = beginAnchorPos;
				}
				else
				{
					Vector2Int anchorPos;
					
					if( orientation == 0)
					{
						anchorPos = meshToAnchorPos(new Vector3(offsetToMidBaseline.x, 0, 0));
					}
					else
					{
						anchorPos = meshToAnchorPos(new Vector3(0, fontSize * i - verticleBeginOffset, 0));
					}

					if (cursor_backward)
					{
						cursor.moveBackward((anchorPos - lastAnchorPos).magnitude);
					}
					else
					{
						cursor.moveForward((anchorPos - lastAnchorPos).magnitude);
					}

					lastAnchorPos = anchorPos;
				}

				vertices[vertexIndex + 0] = vertices_source[vertexIndex + 0] - offsetToMidBaseline;
				vertices[vertexIndex + 1] = vertices_source[vertexIndex + 1] - offsetToMidBaseline;
				vertices[vertexIndex + 2] = vertices_source[vertexIndex + 2] - offsetToMidBaseline;
				vertices[vertexIndex + 3] = vertices_source[vertexIndex + 3] - offsetToMidBaseline;

				float char_angle = control_angle - cursor.getAngle() * Mathf.Rad2Deg;
				if( orientation == 0)
				{
					if (cursor_backward)
					{
						char_angle += 180.0f;
					}
				}
				else
				{
					if( cursor_backward)
					{
						char_angle -= 90;
					}
					else
					{
						char_angle += 90;
					}
				}

				Quaternion rotate = Quaternion.Euler(0, 0, char_angle);
				Vector3 pos = anchorToMeshPos(cursor.getPosition());

				pos += (rotate * Vector3.up) * characterInfo[i].baseLine;
				
				matrix = Matrix4x4.TRS(pos, rotate, Vector3.one);

				vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4( vertices[vertexIndex + 0]);
				vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4( vertices[vertexIndex + 1]);
				vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4( vertices[vertexIndex + 2]);
				vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4( vertices[vertexIndex + 3]);
			}

			//_text.UpdateVertexData();

			UpdateVertexData();
		}
	}

}
