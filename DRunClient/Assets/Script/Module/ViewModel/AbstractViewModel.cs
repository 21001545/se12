using Festa.Client.Module.Net;
using System.Runtime.CompilerServices;

namespace Festa.Client.Module
{
	public abstract class AbstractViewModel : IViewModel
	{
		protected ViewModelBindingContainer _bindingContainer;
		protected MapPacketParser _packetParser;

		protected virtual void init()
        {
			_bindingContainer = ViewModelBindingContainer.create(this);
			_packetParser = MapPacketParser.create();

			bindPacketParser();
        }

		// 계정 전환등이 있어나면 reset이 필요한 경우가 있네
		public virtual void reset()
		{

		}

		protected virtual void bindPacketParser()
		{

		}

		protected virtual void bind(int key,MapPacketParser.ParseHandler handler)
		{
			_packetParser.bind(key, handler);
		}

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

		public virtual void updateFromAck(MapPacket ack)
		{
			_packetParser.parse(ack);
		}
	}
}
