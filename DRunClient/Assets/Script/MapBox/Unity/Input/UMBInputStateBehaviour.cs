using Festa.Client.Module;
using Festa.Client.Module.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Festa.Client.MapBox
{
	public abstract class UMBInputStateBehaviour : StateBehaviour<object>
	{
		protected AbstractInputModule _inputModule;
		protected RectTransform _controlArea;
		protected UnityMapBox _mapBox;

		protected UMBControl _control;
		protected Camera _targetCamera;
		protected UMBInputFSM inputFSM => (_owner as UMBInputFSM);
		
		public static UMBInputStateBehaviour create<T>(UnityMapBox mapBox) where T : UMBInputStateBehaviour, new()
		{
			T state = new T();
			state.init(mapBox);
			return state;
		}

		protected virtual void init(UnityMapBox mapBox)
		{
			_mapBox = mapBox;
			_inputModule = _mapBox.getInputModule();
			_controlArea = _mapBox.getControlArea();
			_control = _mapBox.getControl();
			_targetCamera = Camera.main;
		}

		private static List<RaycastResult> raycastResults = new List<RaycastResult>();
		private static PointerEventData eventData;

		public bool isInControlArea(Vector2 pos)
		{
			if (RectTransformUtility.RectangleContainsScreenPoint(_controlArea, pos, _targetCamera) == false)
			{
				return false;
			}

			raycastResults.Clear();

			if( eventData == null)
			{
				eventData = new PointerEventData(EventSystem.current);
			}
			eventData.position = pos;
			EventSystem.current.RaycastAll(eventData, raycastResults);

			return raycastResults.Count == 0;

			/*			

						if( RectTransformUtility.RectangleContainsScreenPoint(_controlArea, pos, _targetCamera) == false)
						{
							return false;
						}

						if( RectTransformUtility.RectangleContainsScreenPoint(_mapBox.control_base, pos, _targetCamera) == true)
						{
							return false;
						}
						return true;
			*/
		}

		public Vector2 localFromScreen(Vector2 pos)
		{
			Vector2 local_pos;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(_controlArea, pos, _targetCamera, out local_pos);
			return local_pos;
		}

		public MBTileCoordinateDouble tilePosFromScreen(Vector2 pos)
		{
			Vector2 cur_pos;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(_mapBox.tile_root.parent as RectTransform, _inputModule.getTouchPosition(), _targetCamera, out cur_pos);

			MBTileCoordinate currentTilePos = _control.getCurrentTilePos();

			double tile_x = currentTilePos.tile_x + cur_pos.x / 4096;
			double tile_y = currentTilePos.tile_y + -cur_pos.y / 4096;

			return new MBTileCoordinateDouble(currentTilePos.zoom, tile_x, tile_y);
		}


	}
}
