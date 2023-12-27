using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace Festa.Client.Module.MsgPack
{

	public class ObjectFactory {

		private Dictionary<int, ObjectSchema> _schema_map_by_id;
		private Dictionary<Type, ObjectSchema> _schema_map_by_class;
		private GenericSchema _generic_schema;

		public static ObjectFactory create()
		{
			ObjectFactory factory = new ObjectFactory();
			factory.init();
			return factory;
		}

		private void init()
		{
			_schema_map_by_id = new Dictionary<int, ObjectSchema>();
			_schema_map_by_class = new Dictionary<Type, ObjectSchema>();
			_generic_schema = new GenericSchema();
		}

		public ObjectSchema getSchema(Type t)
		{
			ObjectSchema schema;
			if( _schema_map_by_class.TryGetValue(t, out schema) == false)
			{
				return null;
//				throw new Exception("can't find ObjectSchema : " + t.Name);
			}

			return schema;
		}

		public ObjectSchema getSchema(int hash_id)
		{
			ObjectSchema schema;
			if( _schema_map_by_id.TryGetValue( hash_id, out schema) == false)
			{
				return null;
				//throw new Exception("can't find ObjectSchema : " + hash_id);
			}

			return schema;
		}

		public void register<T>() where T : new()
		{
			ObjectSchema schema = ObjectSchema.create(this, typeof(T),()=> { return new T(); });
			_schema_map_by_id.Add(schema.getHashID(), schema);
			_schema_map_by_class.Add( typeof(T), schema);
#if UNITY_EDITOR
			Debug.Log(string.Format("register object schema : name[{0}] hash_id[{1}]", typeof(T).Name, schema.getHashID()));
#endif
		}

		public void registerDictionary<keyT,valueT>()
		{
			_generic_schema.registerDirectionry<keyT, valueT>();
		}

		public void registerList<keyT>()
		{
			_generic_schema.registerList<keyT>();
		}

		public GenericSchema.fnCreate getCreateList(Type t)
		{
			return _generic_schema.getList(t);
		}

		public GenericSchema.fnCreate getCreateDictionary(Type keyT,Type valueT)
		{
			return _generic_schema.getDictionary(keyT, valueT);
		}
	}


}
