using UnityEngine;
using System.Collections;

namespace Festa.Client.Module.MsgPack
{

	public class ValueType {

		public static ValueType NIL = new ValueType(false,false,"NIL");
		public static ValueType BOOLEAN = new ValueType(false, false, "BOOLEAN");
		public static ValueType INTEGER = new ValueType(true, false, "INTEGER");
		public static ValueType FLOAT = new ValueType(true, false, "FLOAT");
		public static ValueType STRING = new ValueType(false, true, "STRING");
		public static ValueType BINARY = new ValueType(false, true, "BINARY");
		public static ValueType ARRAY = new ValueType(false, false, "ARRAY");
		public static ValueType MAP = new ValueType(false, false, "MAP");
		public static ValueType EXTENSION = new ValueType(false, false, "EXTENSION");

		//
		public bool numberType;
		public bool rawType;
		public string name;

		ValueType(bool numberType, bool rawType,string name)
		{
			this.numberType = numberType;
			this.rawType = rawType;
			this.name = name;
		}

	}


}
