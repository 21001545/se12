using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Events;

namespace Festa.Client.Module.UI
{
	public class UIBindingManager
	{
		private List<Binding> _bindingList;
		private bool _isUpdatingAllBindings;

		public static UIBindingManager create()
		{
			UIBindingManager m = new UIBindingManager();
			m.init();
			return m;
		}

		private void init()
		{
			_bindingList = new List<Binding>();
			_isUpdatingAllBindings = false;
		}

		public List<Binding> getBindingList()
		{
			return _bindingList;
		}

		public bool isUpdatingAllBindings()
		{
			return _isUpdatingAllBindings;
		}

		public Binding makeBinding(IViewModel vmObject,string propertyName,object targetObject,string targetPropertyName,Binding.ConverterDelegate converter)
		{
			PropertyInfo vmProperty = vmObject.GetType().GetProperty(propertyName);
			PropertyInfo targetProperty = targetObject.GetType().GetProperty(targetPropertyName);

			Binding binding = Binding.create(vmObject, vmProperty, targetObject, targetProperty, converter);
			vmObject.registerBinding(binding);

			_bindingList.Add(binding);

			return binding;
		}

		public Binding makeBinding(IViewModel vmObject,string propertyName,UnityAction<object> OnUpdateCallback)
		{
			PropertyInfo vmProperty = vmObject.GetType().GetProperty(propertyName);

			Binding binding = Binding.create(vmObject, vmProperty, OnUpdateCallback);
			vmObject.registerBinding(binding);

			_bindingList.Add(binding);

			return binding;
		}

		public Binding makeBinding(IViewModel vmObject,UnityAction<int,object> OnCollectionUpdateCallback)
		{
			Binding binding = Binding.create(vmObject, OnCollectionUpdateCallback);
			vmObject.registerBinding(binding);

			_bindingList.Add(binding);
			return binding;
		}

		public void clearAllBindings()
		{
			foreach(Binding b in _bindingList)
			{
				b.unregisterFromViewModel();
			}
			_bindingList.Clear();
		}

		public void updateAllBindings()
		{
			_isUpdatingAllBindings = true;

			foreach (Binding b in _bindingList)
			{
				b.update();
			}

			_isUpdatingAllBindings = false;
		}
	}
}
