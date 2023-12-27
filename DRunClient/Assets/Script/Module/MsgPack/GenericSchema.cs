using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Festa.Client.Module.MsgPack
{

	public class GenericSchema
	{
		public delegate object fnCreate();

		private Dictionary<Type, fnCreate> _list_schema	= new Dictionary<Type, fnCreate>();
		private TwoKeyDictionary<Type, Type, fnCreate> _dic_schema = new TwoKeyDictionary<Type, Type, fnCreate>();

		public void registerList<T>()
		{
			_list_schema.Add(typeof(T), () => { return new List<T>(); });
		}

		public void registerDirectionry<keyT,valueT>()
		{
			_dic_schema.put(typeof(keyT), typeof(valueT), () => { return new Dictionary<keyT, valueT>(); });
		}

		public fnCreate getList(Type t)
		{
			fnCreate fn;
			if( _list_schema.TryGetValue( t, out fn) == false)
			{
				throw new Exception(string.Format("List<{0}> schema is not registered", t.Name));
			}

			return fn;
		}

		public fnCreate getDictionary(Type keyT,Type valueT)
		{
			fnCreate fn;
			fn = _dic_schema.get(keyT, valueT);
			if( fn == null)
			{
				throw new Exception(string.Format("Dictionary<{0},{1}> schema is not registered", keyT.Name, valueT.Name));
			}

			return fn;
		}

	}


}
