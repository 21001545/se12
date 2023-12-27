using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Festa.Client.Module.UI
{
	public class UISingletonPanel<T> : SingletonBehaviourT<T>, UIAbstractPanel, IViewModel, ITransitionEventHandler where T : SingletonBehaviour 
	{
		protected UILayer	_layer;
		protected AbstractPanelTransition _transition;
		protected UIBindingManager _bindingManager;
		protected ViewModelBindingContainer _bindingContainer;
		protected UIPanelOpenParam _openParam;

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			_transition = GetComponent<AbstractPanelTransition>();
			_transition.init( this);
			_bindingManager = UIBindingManager.create();
			_bindingContainer = ViewModelBindingContainer.create(this);

			gameObject.SetActive(false);

			initializer.addPostProcess( this);
		}

		public override void initSingletonPostProcess(SingletonInitializer initializer)
		{
			registerLayer();
		}

		protected virtual void registerLayer()
		{
			_layer = findLayer();
			if( _layer == null)
			{
				Debug.LogError( "can't find layer", gameObject);
				return;
			}		
			_layer.registerPanel( this);
		}

		protected virtual UILayer findLayer()
		{
			Transform parent = transform.parent;
			while (parent != null)
			{
				UILayer layer;
				if( parent.TryGetComponent<UILayer>(out layer))
				{
					return layer;
				}


				parent = parent.parent;
			}

			return null;
		}

		public AbstractPanelTransition getTransition()
		{
			return _transition;
		}

		public virtual void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			_openParam = param;
			_layer.openPanel(this, param, transitionType, closeType);
		}

		public virtual void close(int transitionType = 0)
		{
			_layer.closePanel(this, transitionType);
		}

		public virtual void update()
		{
			_transition.update();
		}

		public virtual void updateFixed()
		{

		}

		public virtual void setLayer(UILayer layer)
		{
			_layer = layer;
		}

		public virtual UILayer getLayer()
		{
			return _layer;
		}

		public virtual void onTransitionEvent(int type)
		{
		}

		public virtual UIPanelOpenParam getOpenParam()
		{
			return _openParam;
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

		protected bool Set<valueT>(ref valueT storage, valueT value,
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

