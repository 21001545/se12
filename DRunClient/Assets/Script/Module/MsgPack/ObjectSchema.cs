using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace Festa.Client.Module.MsgPack
{

	public class ObjectSchema {

		public delegate object fnCreateInstance();

		private Type _class_type;
		private int _class_key;
		private fnCreateInstance _create_instance;
		private List<ObjectField> _field_list;
		private Dictionary<int, List<ObjectField>> _filtered_field_map;
		private bool _is_custom_serializer;

		public static ObjectSchema create(ObjectFactory owner,Type t,fnCreateInstance create_instance)
		{
			ObjectSchema schema = new ObjectSchema();
			schema.init(owner,t,create_instance);
			return schema;
		}

		public int getHashID()
		{
			return _class_key;
		}

		public Type getClassType()
		{
			return _class_type;
		}

		public bool isCustomSerializer()
		{
			return _is_custom_serializer;
		}

		public object createInstance()
		{
			return _create_instance();
		}

		private void init(ObjectFactory of,Type t,fnCreateInstance create_instance)
		{
			_class_type = t;
			_create_instance = create_instance;
			_class_key = EncryptUtil.makeHashCode(t.Name);
			_field_list = new List<ObjectField>();
			_filtered_field_map = new Dictionary<int, List<ObjectField>>();
			_is_custom_serializer = typeof(CustomSerializer).IsAssignableFrom(t);

			FieldInfo[] fields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			for(int i = 0; i < fields.Length; ++i)
			{
				FieldInfo fi = fields[i];
				if( isNoneSerialize(fi))
				{
					continue;
				}

				if( fi.IsStatic)
				{
					continue;
				}

				ObjectField field = ObjectField.create(of, this, fi);
				_field_list.Add(field);
			}

			_field_list.Sort((a, b) =>	{
				return string.Compare(a._field.Name, b._field.Name, false);
			});

			//for (int i = 0; i < _field_list.Count; ++i)
			//{
			//	Debug.Log(string.Format("[{0}][{1}] : {2}", _class_type.Name, i, _field_list[i]._field.Name));
			//}

		}

		private bool isNoneSerialize(FieldInfo fi)
		{
			object[] attributes = fi.GetCustomAttributes(typeof(SerializeOption), false);
			if( attributes.Length == 0)
			{
				return false;
			}

			SerializeOption opt = (SerializeOption)attributes[0];
			return opt.value == SerializeOption.NONE;
		}

		public List<ObjectField> getFields(int option_mask)
		{
			lock(this)
			{
				List<ObjectField> list_field = null;
				if (_filtered_field_map.TryGetValue(option_mask, out list_field))
				{
					return list_field;
				}

				list_field = new List<ObjectField>();

				for (int i = 0; i < _field_list.Count; ++i)
				{
					ObjectField field = _field_list[i];
					if (field.isFiltered(option_mask))
					{
						continue;
					}

					list_field.Add(field);
				}

				_filtered_field_map.Add(option_mask, list_field);
				return list_field;
			}
		}
	}


}
