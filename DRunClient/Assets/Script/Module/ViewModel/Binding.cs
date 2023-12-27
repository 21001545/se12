using Festa.Client.Module;
using System.Reflection;
using UnityEngine.Events;

namespace Festa.Client.Module
{
	public class Binding
	{
		protected IViewModel			_vmObject;
		protected PropertyInfo			_property;
		protected object				_targetObject;
		protected PropertyInfo			_targetProperty;
		protected ConverterDelegate		_converter;
		protected UnityAction<object>	_onUpdateCallback;
		protected UnityAction<int, object> _onCollectionUpdateCallback;

		public delegate object ConverterDelegate(object src);

		public string getPropertyName()
		{
			return _property.Name;
		}

		public static Binding create(IViewModel vmObject, PropertyInfo property, object targetObject, PropertyInfo targetProperty,ConverterDelegate converter = null)
		{
			Binding b = new Binding();
			b.init(vmObject, property, targetObject, targetProperty, converter);
			return b;
		}

		public static Binding create(IViewModel vmObject, PropertyInfo property, UnityAction<object> OnUpdateCallback)
		{
			Binding b = new Binding();
			b.init(vmObject, property, OnUpdateCallback);
			return b;
		}

		public static Binding create(IViewModel vmObject,UnityAction<int,object> OnCollectionUpdateCallback)
		{
			Binding b = new Binding();
			b.init(vmObject, OnCollectionUpdateCallback);
			return b;
		}

		protected virtual void init(IViewModel vmObject, PropertyInfo property,object targetObject,PropertyInfo targetProperty,ConverterDelegate converter)
		{
			_vmObject = vmObject;
			_property = property;
			_targetObject = targetObject;
			_targetProperty = targetProperty;
			_converter = converter;
		}

		protected virtual void init(IViewModel vmObject, PropertyInfo property,UnityAction<object> OnUpdateCallback)
		{
			_vmObject = vmObject;
			_property = property;
			_onUpdateCallback = OnUpdateCallback;
		}

		protected virtual void init(IViewModel vmObject,UnityAction<int,object> OnCollectionUpdateCallback)
		{
			_vmObject = vmObject;
			_onCollectionUpdateCallback = OnCollectionUpdateCallback;
		}

		public virtual void update()
		{
			if( _targetObject != null)
			{
				object vmValue = _property.GetValue(_vmObject);

				if (_converter != null)
				{
					vmValue = _converter(vmValue);
				}

				_targetProperty.SetValue(_targetObject, vmValue);
			}
			else if( _onUpdateCallback != null)
			{
				object vmValue = _property.GetValue(_vmObject);

				_onUpdateCallback(vmValue);
			}
		}

		public virtual void updateCollection(int event_type,object item)
		{
			_onCollectionUpdateCallback(event_type, item);
		}

		public virtual void unregisterFromViewModel()
		{
			_vmObject.unregisterBinding(this);
		}
	}
}
