using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.Module
{
	public static class CollectionEventType
	{
		public const int add = 1;
		public const int remove = 2;
		public const int clear = 3;
		public const int sort = 4;
		public const int update = 5;
	}

	public class ObservableList<T> : IViewModel
	{
		private List<T> _list = new List<T>();
		private List<Binding> _bindingList = new List<Binding>();

		public List<T> getList()
		{
			return _list;
		}

		public void add(T item)
		{
			_list.Add(item);

			updateBinding(CollectionEventType.add, item);
		}

		public void remove(T item)
		{
			_list.Remove(item);

			updateBinding(CollectionEventType.remove, item);
		}

		public void clear()
		{
			_list.Clear();

			updateBinding(CollectionEventType.clear, default);
		}

		public void sort(Comparison<T> comparison)
		{
			_list.Sort( comparison);

			updateBinding(CollectionEventType.sort, default);
		}

		public void sort(IComparer<T> comparer)
		{
			_list.Sort(comparer);

			updateBinding(CollectionEventType.sort, default);
		}

		public int size()
		{
			return _list.Count;
		}

		public T get(int index)
		{
			return _list[index];
		}

		public bool contains(T item)
		{
			return _list.Contains(item);
		}

		private void updateBinding(int event_type,T item)
		{
			foreach(Binding binding in _bindingList)
			{
				binding.updateCollection(event_type, item);
			}
		}

		public void registerBinding(Binding binding)
		{
			_bindingList.Add(binding);
		}

		public void unregisterBinding(Binding binding)
		{
			_bindingList.Remove(binding);
		}


	}
}
