using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class UMBTileStencilIDManager
	{
		private int _lastID;
		private LinkedList<int> _freeList;

		public static UMBTileStencilIDManager create()
		{
			UMBTileStencilIDManager m = new UMBTileStencilIDManager();
			m.init();
			return m;
		}

		private void init()
		{
			_lastID = 2;
			_freeList = new LinkedList<int>();
		}

		public int allocID()
		{
			int id;
			if( _freeList.Count > 0)
			{
				id = _freeList.First();
				_freeList.RemoveFirst();
			}
			else
			{
				_lastID += 2;		// 아랫자리 1은 UI Mask로 사용된다
				if( _lastID > 255)
				{
					Debug.LogError("no more allocate stencil id");
					return 0;
				}

				id = _lastID;
			}

			//Debug.Log($"occupy stencil id : {id}");

			return id;
		}

		public void freeID(int id)
		{
			_freeList.AddLast(id);

			//Debug.Log($"free stencil id : {id}");
		}
	}
}
