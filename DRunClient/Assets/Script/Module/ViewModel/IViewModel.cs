using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.Module
{
	public interface IViewModel
	{
		public void registerBinding(Binding binding);
		public void unregisterBinding(Binding binding);
	}
}
