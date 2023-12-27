using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.Module.UI
{
	public class UILayer : ReusableMonoBehaviour
	{
		protected List<AbstractLayerJob> _reserved_job_list;
		protected AbstractLayerJob	_current_job;
		protected UIAbstractPanel		_current_panel;

		private List<UIAbstractPanel>	_panels;

		public virtual bool isFixed()
		{
			return false;
		}

		public override void onCreated(ReusableMonoBehaviour source)
		{
		}

		public override void onReused()
		{
			_panels.Clear();
		}

		public virtual void init()
		{
			_reserved_job_list = new List<AbstractLayerJob>();
			_current_job = null;
			_current_panel = null;
			_panels = new List<UIAbstractPanel>(); 
		}

		public virtual void registerPanel(UIAbstractPanel panel)
		{
			if( _panels.Contains( panel))
			{
				Debug.LogError( "panel is already registered");
				return;
			}
			_panels.Add( panel);
		}

		public virtual void unregisterPanel(UIAbstractPanel panel)
		{
			if( _panels.Contains( panel) == false)
			{
				Debug.LogError( "panel is not registered");
				return;
			}

			_panels.Remove( panel);
		}

		public virtual void openPanel(UIAbstractPanel next_panel,UIPanelOpenParam param, int transitionType, int closeType = TransitionEventType.start_close)
		{
			_reserved_job_list.Add( LayerJobOpen.create( this, next_panel, param, transitionType, closeType));
		}

		public virtual void closePanel(UIAbstractPanel next_panel, int transitionType)
		{
			_reserved_job_list.Add( LayerJobClose.create( this, next_panel, transitionType));
		}

		public void update()
		{
			if( _current_job != null)
			{
				if( _current_job.run())
				{
					_current_job = null;
					if( startNextJob() == false)
					{
						// 고정 레이어가 아닌데 panel이 아무것도 없으면 지워준다
						if( isFixed() == false && _current_panel == null)
						{
							UIManager.getInstance().removeLayer(this);
						}
					}
				}
			}
			else
			{
				startNextJob();
			}

			for(int i = 0; i < _panels.Count; ++i)
			{
				_panels[i].update();
			}
		}

		public void updateFixed()
		{
			for(int i = 0; i < _panels.Count; ++i)
			{
				_panels[i].updateFixed();
			}
		}

		private bool startNextJob()
		{
			if( _reserved_job_list.Count == 0)
				return false;

			_current_job = _reserved_job_list[ 0];
			_reserved_job_list.RemoveAt( 0);

			_current_job.start();
			return true;
		}

		public UIAbstractPanel getCurrentPanel()
		{
			return _current_panel;
		}

		public void setCurrentPanel(UIAbstractPanel panel)
		{
			_current_panel = panel;
		}
	}
}