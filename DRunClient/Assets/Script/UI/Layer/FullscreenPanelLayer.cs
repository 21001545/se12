using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Festa.Client.Module.UI;

namespace Festa.Client
{
	public class FullscreenPanelLayer : UIFixedLayer
	{
		public override void openPanel(UIAbstractPanel next_panel, UIPanelOpenParam param, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			_reserved_job_list.Add(LayerJobCoverUp.create(this, next_panel, param, transitionType));
		}
	}
}
