using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class UMBSymbolRenderer : ReusableMonoBehaviour
	{
		public RectTransform rect_root;
		public SpriteRenderer icon;
		public UMBSymbolText text;

		private Color _icon_color;
		private Color _text_color;
		private FloatSmoothDamper _alphaDamper;

		private UMBSymbolPlacement _placement;
		private UMBSymbolPoint _symbolPoint;

		private int _collision_order;

		private float _icon_size;
		//private float _initial_delay;
		//private float _lastCheckCollisionZoom;

		private bool _isCollided = false;
		public bool IsCollided
        {
			get { return _isCollided; }
			set { _isCollided = value; }
        }

		private List<Collider> _colliderList;
		private AABBTree _tree => _placement.getLabelManager().getTree();

        public UMBSymbolPlacement getPlacement()
		{
			return _placement;
		}

		public UMBSymbolPoint getSymbolPoint()
		{
			return _symbolPoint;
		}

		public List<Collider> getColliderList()
		{
			return _colliderList;
		}

		public int getCollisionOrder()
		{
			return _collision_order;
		}

		//public Dictionary<AABBCollider, UMBSymbolRenderer> getColliderMap()
		//{
		//	return _colliderMap;
		//}

		public override void onCreated(ReusableMonoBehaviour source)
		{
			_alphaDamper = FloatSmoothDamper.create(0.0f, 0.1f);
			_icon_color = new Color(1, 1, 1, 0.0f);
			_text_color = new Color(1, 1, 1, 0.0f);
			_colliderList = new List<Collider>();
			//_colliderList = new List<AABBCollider>();

			text.onCreated();

			//Debug.Log($"[{Time.frameCount}] create renderer", gameObject);
		}

		public override void onReused()
		{
			//Debug.Log($"[{Time.frameCount}] reuse renderer", gameObject);
		}

		public override void onDelete()
		{
			//Debug.Log($"[{Time.frameCount}] delete renderer", gameObject);
		}

		public void delete()
		{
			text.delete();

			foreach(Collider collider in _colliderList)
			{
				_placement.getLabelManager().getTree().removeObject(collider);
			}
			_colliderList.Clear();
			GameObjectCache.getInstance().delete(this);
		}

		public void setup(UMBSymbolPlacement placement,UMBSymbolPoint point)
		{
			_placement = placement;
			_symbolPoint = point;

			_collision_order = placement.getCollisionOrder();
			text.init(this);
			
			// 초기화를 위해 임시
			transform.position = _symbolPoint.getWorldPosition();

			initColor();
			initRenderers();

			setupIcon();
			setupText();

#if UNITY_EDITOR
			setupGameObjectName();
#endif
			updatePosition();

			//_initial_delay = Time.time + 1.0f;
			//_lastCheckCollisionZoom = 0;
		}

		private void initColor()
		{
			_text_color = _placement.getTextColor();
			_text_color.a = 0;
			_icon_color.a = 0;

			_alphaDamper.reset(0.0f);
			_alphaDamper.setTarget(0.0f);
		}

		private void initRenderers()
		{
			text.color = _text_color;
			icon.color = _icon_color;

			//text.enabled = false;
			icon.enabled = false;

			text.gameObject.SetActive(false);
			icon.gameObject.SetActive(false);
		}

		private void setupIcon()
		{
			if( _placement.hasIcon() == false)
			{
				return;
			}

			float iconSize = _placement.getIconSize();
			Sprite sprite = _placement.getIcon();

			icon.gameObject.SetActive(true);
			icon.sprite = sprite;
			icon.transform.localScale = Vector3.one * 100 * iconSize * 0.5f;
			icon.transform.rotation = Quaternion.identity;

			_icon_size = sprite.bounds.size.y * 100 * iconSize * 0.5f;

			Shape shape = PolygonShape.createBox(Vector2.zero, new Vector2(_icon_size, _icon_size) / 2);
			Collider collider = Collider.create(MapBoxDefine.ColliderType.icon,GetInstanceID(), _collision_order, shape, transform.localPosition);

			_tree.insertObject(collider);
			_colliderList.Add(collider);

   //         _colliderList.Add(iconCollider);
   //         _colliderMap.Add(iconCollider, this);
			//_renderTree.insertObject(iconCollider);
        }

		private void setupText()
		{
			if( _placement.hasText() == false)
			{
				return;
			}

			text.gameObject.SetActive(true);
			text.text = _placement.getText();

			UMBFontSource.FontSource fontSource = _placement.getFontSource();
			fontSource.set(text);

			if ( isAlignToLine())
			{
				setupText_AlignMap();
			}
			else
			{
				setupText_AlignViewport();
			}
		}

		private bool isAlignToLine()
		{
			int alignment = _placement.getTextRotationAlignment();
			if ( alignment == MBStyleDefine.TextPitchAlignmentType.map)
			{
				return true;
			}
			else if( alignment == MBStyleDefine.TextPitchAlignmentType.auto)
			{
				if (_placement.getType() == MBStyleDefine.SymbolPlacementType.line ||
					_placement.getType() == MBStyleDefine.SymbolPlacementType.line_center)
				{
					return true;
				}
			}

			return false;
		}

		private void setupText_AlignViewport()
		{
			float textSize = _placement.getTextSize();
			float textMaxWidth = _placement.getTextMaxWidth() * textSize;

			text.setAlignToLine(false);
			text.fontSize = textSize;
			text.lineSpacing = 1.2f;

			Vector2 textBound = new Vector2(textMaxWidth, textSize);
			Vector2 preferredSize = text.GetPreferredValues(_placement.getText(), textBound.x, textBound.y);

			//Vector2 textBound = new Vector2(textMaxWidth, textSize);
			//Vector2 preferredSize = text.GetPreferredValues(_placement.getText(), textBound.x, textBound.y);

			text.rectTransform.sizeDelta = textBound;

			if (_placement.hasIcon() && _placement.getTextOffset() != Vector2.zero)
			{
				text.rectTransform.anchoredPosition = new Vector2(0, -_icon_size / 2.0f - preferredSize.y / 2.0f);
			}
			else
			{
				text.rectTransform.anchoredPosition = Vector2.zero;
			}


			Shape shape = PolygonShape.createBox(Vector2.zero, preferredSize / 2);
			Collider collider = Collider.create(MapBoxDefine.ColliderType.text_box,GetInstanceID(), _collision_order, shape, transform.localPosition + text.transform.localPosition);

			_tree.insertObject(collider);
			_colliderList.Add(collider);

			//textCollider.getSquare().setSize(preferredSize);
			//textCollider.enabled = true;

   //         _colliderList.Add(textCollider);
   //         _colliderMap.Add(textCollider, this);
			//_renderTree.insertObject(textCollider);
        }

		private void setupText_AlignMap()
		{
			float textSize = _placement.getTextSize();
			float textMaxWidth = _placement.getTextMaxWidth() * textSize;

			text.setAlignToLine(true);
			text.fontSize = textSize;
			text.lineSpacing = 1.2f;

			Vector2 textBound = new Vector2( textMaxWidth, textSize);

			//textCollider.enabled = false;

			text.rectTransform.sizeDelta = textBound;

			//StartCoroutine(textLineWrapper.wrapTextEndOfFrame());
		}

		private void setupGameObjectName()
		{
			//string text = $"[{_placement.getLayer().getLayerStyle().getID()}][{_placement.getFeatureID()}]";
			string text = $"[{_collision_order}]";
			if (_placement.hasIcon())
			{
				text += $"[{_placement.getIcon().name}]";
			}
			if ( _placement.hasText())
			{
				text += $"[{_placement.getText()}]";
			}
			
			gameObject.name = text;
		}

		public void startDelete()
		{
			_alphaDamper.setTarget(0);
/*			foreach (AABBCollider col in _colliderList)
            {
				_renderTree.removeObject(col);
			}*/
		}

		public bool isHidingComplete()
		{
			return _alphaDamper.getCurrent() <= 0;
		}

		public void updatePosition()
		{
			transform.position = _symbolPoint.getWorldPosition();

			text.updatePosition();
			updateIconRotation();

			foreach(Collider collider in _colliderList)
			{
				if( collider.getType() == MapBoxDefine.ColliderType.icon)
				{
					collider.setPosition(transform.localPosition);
					_tree.updateObject(collider);
				}
				else if( collider.getType() == MapBoxDefine.ColliderType.text_box)
				{
					collider.setPosition(transform.localPosition + text.transform.localPosition);
					_tree.updateObject(collider);
				}
			}

			//foreach (AABBCollider col in _colliderList)
   //         {
			//	_renderTree.updateLeaf(col.getAABBNode(), col.getAABB());
   //         }
		}

		private void updateIconRotation()
		{
			if(_placement.hasIcon() == false)
			{
				return;
			}
			if(_symbolPoint.getAnchorCursor() == null)
			{
				return;
			}

			if (_placement.getIconRotationAlignment() == MBStyleDefine.RotationAlignment.map)
			{
				icon.transform.rotation = Quaternion.Euler(0, 0, _placement.getControl().getRotateRoot().localEulerAngles.z -_symbolPoint.getAnchorCursor().getAngle() * Mathf.Rad2Deg);
			}
			else if( _placement.getIconRotationAlignment() == MBStyleDefine.RotationAlignment.auto &&
				_placement.getType() != UMBSymbolPlacement.PlacementType.point)
			{
				icon.transform.rotation = Quaternion.Euler(0, 0, _placement.getControl().getRotateRoot().localEulerAngles.z - _symbolPoint.getAnchorCursor().getAngle() * Mathf.Rad2Deg);
			}
		}

		//public void addCollider(AABBCollider collider)
		//{
		//	_colliderList.Add(collider);
		//	_colliderMap.Add(collider, this);
		//	_renderTree.insertObject(collider);
		//}

		//public void removeCollider(AABBCollider collider)
		//{
		//	_colliderList.Remove(collider);
		//	_colliderMap.Remove(collider);
		//	_renderTree.removeObject(collider);
		//}

		private List<Collider> overlapList = new List<Collider>();

		public void checkCollision()
		{
			_isCollided = false;

			foreach(Collider col in _colliderList)
			{
				overlapList.Clear();
				_tree.queryOverlaps(col, overlapList);
				if( overlapList.Count == 0)
				{
					continue;
				}

				foreach(Collider overlapCol in overlapList)
				{
					if( col.getOrder() < overlapCol.getOrder() && CollisionTest.test( col, overlapCol))
					{
						_isCollided = true;
						break;
					}
				}

				if( _isCollided)
				{
					break;
				}
			}

			_alphaDamper.setTarget(_isCollided ? 0.0f : 1.0f);
		}

//		public bool handleCollision(bool collided, UMBSymbolRenderer rend = null)
//        {
///*            float currentZoom = _placement.getControl().getZoomDamper().getCurrent();
//            if (_lastCheckCollisionZoom == currentZoom)
//            {
//                return true;
//            }
//            _lastCheckCollisionZoom = currentZoom;*/

//            if (this == rend)
//            {
//                _isCollided = false;
//				return false;
//            }

//            _isCollided = collided;

//			if(_isCollided)
//            {
//				if(_collision_order < rend._collision_order)
//					_isCollided = true;
//				else
//					_isCollided = false;
//            }

//			if (!_isCollided)
//			{
//				if (_placement.hasText() && text.isTouchOutlineEdge())
//				{
//					_isCollided = true;
//				}
//			}

//			//_alphaDamper.setTarget(1.0f);
//			_alphaDamper.setTarget(_isCollided ? 0.0f : 1.0f);
//			return _isCollided;
//		}

		public void update()
		{
			// 알파, 색 이런 거
			if( _alphaDamper.update())
			{
				_icon_color.a = _alphaDamper.getCurrent();
				_text_color.a = _alphaDamper.getCurrent();

				text.color = _text_color;
				icon.color = _icon_color;

				if (_text_color.a <= 0)
				{
					//if (text.gameObject.activeSelf && text.enabled == true)
					//{
					//	text.enabled = false;
					//}
					if (icon.gameObject.activeSelf && icon.enabled == true)
					{
						icon.enabled = false;
					}
				}
				else
				{
					//if (text.gameObject.activeSelf && text.enabled == false)
					//{
					//	text.enabled = true;
					//}

					if (icon.gameObject.activeSelf && icon.enabled == false)
					{
						icon.enabled = true;
					}
				}
			}
		}

		void OnDrawGizmos()
		{
			if (_colliderList == null)
			{
				return;
			}

			Gizmos.color = Color.black;

			foreach (Collider collider in _colliderList)
			{
				if (collider.getShape().getType() == Shape.ShapeType.Circle)
				{
					CircleShape circle = collider.getShape() as CircleShape;

					GizmoExtension.drawCircleLine(transform.parent, collider.getPosition(), circle._radius, Color.black);
				}
				else
				{
					PolygonShape polygon = collider.getShape() as PolygonShape;
					AABB localAABB = polygon.getLocalAABB();

					GizmoExtension.drawBoxLine(transform.parent, collider.getPosition(), localAABB.Size / 2, Color.black);
				}
			}

		}
	}
}
