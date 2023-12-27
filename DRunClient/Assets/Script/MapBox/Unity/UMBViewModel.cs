using Festa.Client.Module;
using UnityEngine;

namespace Festa.Client.MapBox
{
	// 여러가지 상태값을 저장
	// UI랑 Interaction할려고 만듬
	public class UMBViewModel : AbstractViewModel
	{
		private int _currentLocationMode;
		private int _projectionMode;
		private UMBMapRevealViewModel _mapReveal;
		private bool _showMapDeco;
		private float _extrudeRatio;
		private float _zoom;
		private float _zAngle;

		public int CurrentLocationMode
		{
			get
			{
				return _currentLocationMode;
			}
			set
			{
				Set(ref _currentLocationMode, value);
			}
		}

		public int ProjectionMode
		{
			get
			{
				return _projectionMode;
			}
			set
			{
				Set(ref _projectionMode, value);
			}
		}

		public UMBMapRevealViewModel MapReveal
		{
			get
			{
				return _mapReveal;
			}
			set
			{
				Set(ref _mapReveal, value);
			}
		}

		public bool ShowMapDeco
		{
			get
			{
				return _showMapDeco;
			}
			set
			{
				Set(ref _showMapDeco, value);
			}
		}

		public float ExtrudeRatio
		{
			get
			{
				return _extrudeRatio;
			}
			set
			{
				Set(ref _extrudeRatio, value);
			}
		}

		public float Zoom
		{
			get
			{
				return _zoom;
			}
			set
			{
				Set(ref _zoom, value);
			}
		}

		public float ZAngle
		{
			get
			{
				return _zAngle;
			}
			set
			{
				Set(ref _zAngle, value);
			}
		}

		public static UMBViewModel create()
		{
			UMBViewModel vm = new UMBViewModel();
			vm.init();
			return vm;
		}

		protected override void init()
		{
			base.init();

			_currentLocationMode = UMBDefine.CurrentLocationMode.none;
			_projectionMode = UMBDefine.ProjectionMode.two_d;
			_mapReveal = null;
			_showMapDeco = false;
		}
	}
}
