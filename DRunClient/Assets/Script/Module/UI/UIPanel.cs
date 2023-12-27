using System.Runtime.CompilerServices;

namespace Festa.Client.Module.UI
{
	public class UIPanel : ReusableMonoBehaviour, UIAbstractPanel, IViewModel, ITransitionEventHandler
	{
		private UILayer	_layer;
		private AbstractPanelTransition _transition;
		private UIBindingManager _bindingManager;
		private ViewModelBindingContainer _bindingContainer;
		private UIPanelOpenParam _openParam;

		public override void onCreated(ReusableMonoBehaviour source)
		{
			_layer = null;
			_transition = GetComponent<AbstractPanelTransition>();
			if ( _transition != null )
				_transition.init( this);
			_bindingManager = UIBindingManager.create();
			_bindingContainer = ViewModelBindingContainer.create(this);
		}

		public AbstractPanelTransition getTransition()
		{
			return _transition;
		}

		public virtual void open(UIPanelOpenParam param, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			_openParam = param;
			_layer.openPanel(this,param, transitionType, closeType);
		}

		public virtual void close(int transitionType = 0)
		{
			_layer.closePanel(this, transitionType);
		}

		public virtual void update()
		{
			_transition?.update();
		}

		public virtual void updateFixed()
		{

		}

		public virtual UIPanelOpenParam getOpenParam()
		{
			return _openParam;
		}

		public virtual void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.end_close)
			{
				UIManager.getInstance().deleteInstantPanel(this);
			}
		}

		public virtual void setLayer(UILayer layer)
		{
			_layer = layer;
		}

		public virtual UILayer getLayer()
		{
			return _layer;
		}

		public virtual UIBindingManager getBindingManager()
		{
			return _bindingManager;
		}

		public virtual void onLanguageChanged(int newLanguage)
		{

		}

		#region IViewModel

		protected void notifyPropetyChanged([CallerMemberName] string fieldname = null)
		{
			_bindingContainer.updateBinding(fieldname);
		}

		protected bool Set<T>(ref T storage, T value,
			[CallerMemberName] string propertyName = null)
		{
			if (Equals(storage, value))
			{
				return false;
			}

			storage = value;
			notifyPropetyChanged(propertyName);
			return true;
		}

		public virtual void registerBinding(Binding binding)
		{
			_bindingContainer.registerBinding(binding);
		}

		public virtual void unregisterBinding(Binding binding)
		{
			_bindingContainer.unregisterBinding(binding);
		}

		#endregion
	}
}