using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.Module.UI
{
	public interface UIAbstractPanel
	{
		AbstractPanelTransition getTransition();
		void update();
		void updateFixed();
		
		UILayer getLayer();
		void setLayer(UILayer layer);
		void open(UIPanelOpenParam param = null, int transitionType = 0, int closeTransition = TransitionEventType.start_close);
		void close(int transitionType = 0);

		UIBindingManager getBindingManager();
		UIPanelOpenParam getOpenParam();

		//
		void onLanguageChanged(int newLanguage);
	}
}

