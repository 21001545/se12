using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Festa.Client.MapBox
{
	public class UMBSymbolPlacementLine : UMBSymbolPlacement
	{
		private List<MBAnchors> _anchors_list;
		private float _textPreferredSize;

		private const float glyphSize = 24.0f;

		private float _textMaxSize;

		//public static long test_id = 4430351071;
		//public static long test_id = 9212195761;	//(테헤란로)
		//public static long test_id = 724652461;
		//public static long test_id = 4179201491;
		//public static long test_id = 780615011;

		public static long test_id = 9;

		private float _tileSpacing;

		protected void calcTextPreferredSize()
		{
			if( _fontSource != null)
			{
				TextMeshPro text = _control.getMapBox().text_measure;

				_fontSource.set(text);
				text.fontSize = _textSize;
				text.characterSpacing = 0.2f;
				text.lineSpacing = 1.2f;

				//_textPreferredSize = text.GetPreferredValues(_text, float.MaxValue, _textSize).x;

				// 2022.08.22 속도가 느려서 이걸로 바꿈 (임시)

				float baseScale = _textSize / (_fontSource.font.faceInfo.pointSize / _fontSource.font.faceInfo.scale);

				_fontSource.font.TryAddCharacters(_text);

				_textPreferredSize = 0;
				foreach(char c in _text)
				{
					TMP_Character charInfo;
					if(_fontSource.font.characterLookupTable.TryGetValue(c, out charInfo))
					{
						_textPreferredSize += charInfo.glyph.metrics.width * baseScale;
					}
					//else
					//{
					//	Debug.Log($"no char:{c}");
					//}
				}
			}
			else
			{
				_textPreferredSize = 0;
			}
		}

		protected override void evaluateSymbolData(MBStyleExpressionContext ctx)
		{
			base.evaluateSymbolData(ctx);

			ctx._zoom = 18;
			_textMaxSize = (float)_layer.getLayerStyle().evaluateTextSize(ctx);
		}

		protected override void createPoints()
		{
			calcTextPreferredSize();

			prepareAnchorsList();
			if( _anchors_list == null || _anchors_list.Count == 0)
			{
				return;
			}

			//float symbol_width = calcSymbolWidth();
			//if( symbol_width == 0)
			//{
			//	return;
			//}

			//if (_feature.id == test_id)
			//{
			//	int a = 0;
			//}

			int name_key = EncryptUtil.makeHashCode(_text);

			MBStyleExpressionContext ctx = MBStyleDefine.mainThreadContext;
			ctx._zoom = _control.getCurrentTilePos().zoom;
			ctx._feature = _feature;

			float pixelSpacing = (float)_layer.getLayerStyle().evaluateSymbolSpacing(ctx);
			float tileScale = (float)_control.getScaleRoot().parent.lossyScale.x * MapBoxDefine.scale_pivot;
			float tilePixelRatio = _control.getMapBox().label_root.lossyScale.x / tileScale;

			float tileLabelLength = _textPreferredSize * tilePixelRatio;
			float tileLabelHalfLength = tileLabelLength / 2.0f;
			float tileSpacing = pixelSpacing * tilePixelRatio;
			float repeatDistance = tileSpacing / 2;

			if (tileSpacing - tileLabelLength < tileSpacing / 4)
			{
				tileSpacing = tileLabelLength + tileSpacing / 4;
			}

			_tileSpacing = tileSpacing;

			foreach (MBAnchors anchors in _anchors_list)
			{
				bool isContinueLine = anchors.isContinueLine();

				float length = anchors.getLength();

				//float offset = (tileLabelLength / 2.0f + glyphSize * 2) % tileSpacing;
				float offset;

				if (anchors.isContinueLine())
				{
					offset = tileSpacing / 2;
				}
				else
				{
					offset = (tileLabelLength / 2.0f + _textSize * 2 * tilePixelRatio) % tileSpacing;
				}

				length -= offset;
				MBAnchorCursor cursor = anchors.getCursor(offset);

				List<UMBSymbolPoint> singlePointList = new List<UMBSymbolPoint>();

				while(length - tileLabelHalfLength > 0)
				{
					MBTileCoordinateDouble tilePos = tilePosFromLocalPos(cursor.getPositionInt());
					UMBSymbolPoint point = UMBSymbolPoint.create(this, tilePos, cursor.copy());
					singlePointList.Add(point);

					length -= tileSpacing;
					cursor.moveForward(tileSpacing);
				}

				if(isContinueLine == false && singlePointList.Count == 0)
				{
					if( anchors.getLength() / 2.0f + tileLabelHalfLength < anchors.getLength())
					{
						cursor = anchors.getCursor(anchors.getLength() / 2.0f);

						MBTileCoordinateDouble tilePos = tilePosFromLocalPos(cursor.getPositionInt());
						UMBSymbolPoint point = UMBSymbolPoint.create(this, tilePos, cursor.copy());
						singlePointList.Add(point);
					}
				}

				foreach(UMBSymbolPoint point in singlePointList)
				{
					if( _labelManager.symbolPointIsTooClose(name_key, point, repeatDistance) == false)
					{
						_symbolPoints.Add(point);
					}
				}
			}

			if( _feature.id == test_id)
			{
				Debug.Log($"[{ctx._zoom}] pcount[{_symbolPoints.Count}] tileSpacing[{tileSpacing}] tileScale[{tileScale}]");
			}
		}

		private void prepareAnchorsList()
		{
			if (_anchors_list == null)
			{
				_anchors_list = _feature.getAnchors();
				if (_anchors_list == null)
				{
					_anchors_list = _feature.buildAnchors();
				}
			}
		}

		//public override void onDrawGizmos()
		//{
		//	List<MBAnchors> anchorsList = _feature.getAnchors();
		//	if( anchorsList == null)
		//	{
		//		return;
		//	}

		//	if( _feature.id != test_id)
		//	{
		//		return;
		//	}

		//	drawSymbolPoints();
		//}

		private static int index = 0;

		private void drawFeatureAnchors()
		{

		}



		private void drawFeaturePaths()
		{

			Vector2Int offset = _ownerTilePos.offsetFrom(_control.getCurrentTilePos());
			offset.x *= 4096;
			offset.y *= 4096;

			++index;
			Gizmos.color = (index % 2 ==  0) ? Color.red : Color.black;

			foreach (short[] path in _feature.pathList)
			{
				List<List<Vector2Int>> clippedLines = new List<List<Vector2Int>>();
				MapBoxUtil.clipLines(path, clippedLines);
				
				foreach(List<Vector2Int> line in clippedLines)
				{
					drawLine(line, offset);
				}
			}
		}

		private void drawLine(List<Vector2Int> list,Vector2Int offset)
		{
			for (int i = 0; i < list.Count - 1; ++i)
			{
				Vector2Int from = list[i];
				Vector2Int to = list[i + 1];

				from += offset;
				to += offset;

				Vector3 wFrom = _control.getTileRoot().TransformPoint(new Vector3(from.x, -from.y, 0));
				Vector3 wTo = _control.getTileRoot().TransformPoint(new Vector3(to.x, -to.y, 0));

				Gizmos.DrawLine(wFrom, wTo);

				Gizmos.DrawCube(wTo, Vector3.one / 500.0f);
				if (i == 0)
				{
					Gizmos.DrawCube(wFrom, Vector3.one / 500.0f);
				}
			}

		}
	}
}
