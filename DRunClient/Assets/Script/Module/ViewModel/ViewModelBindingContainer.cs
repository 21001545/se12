using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.Module
{
	public class ViewModelBindingContainer
	{
		protected IViewModel _owner;
		protected Dictionary<string, List<Binding>> _bindings;

		public static ViewModelBindingContainer create(IViewModel owner)
		{
			ViewModelBindingContainer container = new ViewModelBindingContainer();
			container.init(owner);
			return container;
		}

		private void init(IViewModel owner)
		{
			_owner = owner;
			_bindings = new Dictionary<string, List<Binding>>();
		}

		public void updateBinding(string name)
		{
			List<Binding> listBinding;
			if (_bindings.TryGetValue(name, out listBinding))
			{
				foreach (Binding b in listBinding)
				{
					b.update();
				}
			}
		}

		public void registerBinding(Binding binding)
		{
			string name = binding.getPropertyName();
			List<Binding> listBinding;
			if (_bindings.TryGetValue(name, out listBinding) == false)
			{
				listBinding = new List<Binding>();
				_bindings.Add(name, listBinding);
			}

			listBinding.Add(binding);
		}

		public void unregisterBinding(Binding binding)
		{
			List<Binding> listBinding;
			if (_bindings.TryGetValue(binding.getPropertyName(), out listBinding))
			{
				listBinding.Remove(binding);
			}
		}
	}
}
