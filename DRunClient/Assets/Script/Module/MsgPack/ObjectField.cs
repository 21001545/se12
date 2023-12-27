using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;

namespace Festa.Client.Module.MsgPack
{

    public class ObjectField {

		public const int Type_Normal = 1;
		public const int Type_Array = 2;
		public const int Type_List = 3;
		public const int Type_Dictionary = 4;

        public FieldInfo _field;
        public int   _option;
		public int _type;
		
		public List<Type> _genericTypeList;
		public List<GenericSchema.fnCreate> _genericCreateList;

		//public GenericSchema.fnCreate

		//public GenericSchema.fnCreateList _create_list;
		//public GenericSchema.fnCreateDictionary _create_dictionary;

		//public List<Type> _sub_generic_type_list;
		//public List<GenericSchema.fnCreateList> _sub_generic_create_list;
		//public List<GenericSchema.fnCreateDictionary> _sub_generic_cretae_directionry;

		public static ObjectField create(ObjectFactory of,ObjectSchema owner,FieldInfo object_field)
		{
			ObjectField field = new ObjectField();
			field.init(of, owner, object_field);
			return field;
		}

		public static bool isListOrDic(Type type)
		{
			if( type.IsArray)
			{
				return false;
			}
			return typeof(IList).IsAssignableFrom(type) || typeof(IDictionary).IsAssignableFrom(type);
		}

		public void init(ObjectFactory of,ObjectSchema owner,FieldInfo object_field)
		{
			_field = object_field;
			_option = SerializeOption.ALL;
			_type = Type_Normal;
			_genericTypeList = new List<Type>();
			_genericCreateList = new List<GenericSchema.fnCreate>();
			//_create_list = null;
			//_create_dictionary = null;

			//_sub_generic_type_list = new List<Type>();
			//_sub_generic_create_list = new List<GenericSchema.fnCreateList>();
			//_sub_generic_cretae_directionry = new List<GenericSchema.fnCreateDictionary>();
			//_hasSubGeneric = false;

			if ( _field.FieldType.IsArray)
			{
				_type = Type_Array;
			}
			else if( typeof(IList).IsAssignableFrom( _field.FieldType))
			{
				_type = Type_List;

				if( owner.isCustomSerializer() == false)
				{
					prepareGeneric(of, _field.FieldType);
				}

				//Type argument_type = _field.FieldType.GetGenericArguments()[0];
				//_create_list = of.getCreateList( argument_type);	

				//// generic type이 generic일 수 있다
				//if( typeof(IList).IsAssignableFrom( argument_type) || typeof(IDictionary).IsAssignableFrom( argument_type))
				//{
				//	prepareSubGeneric(of,argument_type);
				//	_hasSubGeneric = true;
				//}
			}
			else if( typeof(IDictionary).IsAssignableFrom( _field.FieldType))
			{
				_type = Type_Dictionary;

				if (owner.isCustomSerializer() == false)
				{
					prepareGeneric(of, _field.FieldType);
				}

				//Type[] generic_types = _field.FieldType.GetGenericArguments();
				//_create_dictionary = of.getCreateDictionary(generic_types[0], generic_types[1]);

				//// Key가 List나 Dictionary일순 없겠지.. 제발 
			
				//// generic type이 generic일 수 있다
				//if (typeof(IList).IsAssignableFrom(generic_types[1]) || typeof(IDictionary).IsAssignableFrom(generic_types[1]))
				//{
				//	prepareSubGeneric(of,generic_types[1]);
				//	_hasSubGeneric = true;
				//}
			}

			SerializeOption option = getSerializeOption(_field);
			if( option != null)
			{
				_option = option.value;
			}

			//Debug.Log(string.Format("class[{0}] field[{1}] type[{2}] option[{3}]", owner.getClassType().Name, _field.Name, _field.FieldType.Name, _option));
		}

		private void prepareGeneric(ObjectFactory of,Type type)
		{
			if( isListOrDic(type) == false)
			{
				return;
			}

			_genericTypeList.Add(type);

			if (typeof(IList).IsAssignableFrom(type))
			{
				Type[] arg_types = type.GetGenericArguments();

				_genericCreateList.Add(of.getCreateList(arg_types[0]));
				prepareGeneric(of, arg_types[0]);
			}
			else if (typeof(IDictionary).IsAssignableFrom(type))
			{
				Type[] arg_types = type.GetGenericArguments();
				_genericCreateList.Add(of.getCreateDictionary( arg_types[0],arg_types[1]));

				prepareGeneric(of, arg_types[1]);
			}
		}

		public static SerializeOption getSerializeOption(FieldInfo field_info)
		{
			object[] attributes = field_info.GetCustomAttributes(typeof(SerializeOption), false);
			if( attributes.Length == 0)
			{
				return null;
			}

			return attributes[0] as SerializeOption;
		}

		public bool isFiltered(int option_mask)
		{
			if( _option == SerializeOption.NONE)
			{
				return true;
			}

			return (_option & option_mask) == 0;
		}

		// MsgPack용
		public object validateForGeneric(int depth,object src)
		{
			Type type = _genericTypeList[depth];
			if( typeof(IList).IsAssignableFrom(type))
			{
				IList target_list = (IList)_genericCreateList[depth]();
				if (src.GetType().IsArray)
				{
					object[] src_array = (object[])src;

					for(int i = 0; i < src_array.Length; ++i)
					{
						object item = src_array[i];
						if (depth + 1 < _genericTypeList.Count)
						{
							item = validateForGeneric(depth + 1, item);
						}

						target_list.Add(item);
					}
				}
				else
				{
					throw new Exception( String.Format("object must be array for IList field: src[{0}]", src.GetType().Name));
				}
				return target_list;
			}
			else if( typeof(IDictionary).IsAssignableFrom(type))
			{
				IDictionary target_dic = (IDictionary)_genericCreateList[depth]();
				if( typeof(IDictionary).IsAssignableFrom(src.GetType()))
				{
					IDictionary src_dic = (IDictionary)src;
					IDictionaryEnumerator src_dic_enum = src_dic.GetEnumerator();
					while (src_dic_enum.MoveNext())
					{
						object value = src_dic_enum.Value;

						if( depth + 1 < _genericTypeList.Count)
						{
							value = validateForGeneric(depth + 1, value);
						}

						target_dic.Add(src_dic_enum.Key, value);
					}
				}
				else
				{
					throw new Exception(String.Format("object must be dictionary for IDictionary field: src[{0}]", src.GetType().Name));
				}

				return target_dic;
			}

			throw new Exception(String.Format("is not generic type: type[{0}]", type.Name));
		}
    }

}
