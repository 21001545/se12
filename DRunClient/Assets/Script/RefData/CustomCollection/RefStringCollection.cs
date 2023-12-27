using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.RefData
{
	public class RefStringCollection
	{
		private Dictionary<int, Dictionary<long, RefString>> _data;
		private Dictionary<int, Dictionary<int, List<RefString>>> _dataTypeList;

		private Dictionary<long, RefString> _currentLangDic;
		private int _currentLangType;
		private int _fallbackLangType;

		public int getCurrentLangType()
		{
			return _currentLangType;
		}

		public void setCurrentLangType(int lang_type)
		{
			_currentLangType = lang_type;
		}

		public static RefStringCollection create(RefDataContainer container)
		{
			RefStringCollection c = new RefStringCollection();
			c.init(container);
			return c;
		}

		private void init(RefDataContainer container)
		{
			_data = new Dictionary<int, Dictionary<long, RefString>>();
			_dataTypeList = new Dictionary<int, Dictionary<int, List<RefString>>>();
			_fallbackLangType = LanguageType.en;

			Dictionary<int,RefData> dic = container.getMap<RefString>();
			foreach(KeyValuePair<int,RefData> item in dic)
			{
				RefString ref_data = item.Value as RefString;

				//------------------------------------------------------
				Dictionary<long, RefString> langDic;
				if( _data.TryGetValue( ref_data.lang, out langDic) == false)
				{
					langDic = new Dictionary<long, RefString>();
					_data.Add(ref_data.lang, langDic);
				}

				long key = makeKey(ref_data.type, ref_data.id);
				if( langDic.ContainsKey( key))
				{
					langDic.Remove(key);
				}
				langDic.Add(key, ref_data);

				//--------------------------------------------------------
				Dictionary<int, List<RefString>> langDicTypeList;
				if( _dataTypeList.TryGetValue( ref_data.lang, out langDicTypeList) == false)
				{
					langDicTypeList = new Dictionary<int, List<RefString>>();
					_dataTypeList.Add(ref_data.lang, langDicTypeList);
				}

				List<RefString> typeList;
				if(langDicTypeList.TryGetValue( ref_data.type, out typeList) == false)
				{
					typeList = new List<RefString>();
					langDicTypeList.Add(ref_data.type, typeList);
				}

				typeList.Add(ref_data);
			}

			_currentLangType = LanguageType.ko;
			_currentLangDic = getLangDic(LanguageType.ko);
		}

		public static long makeKey(int type,int id)
		{
			long key = (long)type + (long)Int32.MaxValue + ((long)id << 32);
			return key;
		}

		private Dictionary<long,RefString> getLangDic(int lang)
		{
			Dictionary<long, RefString> langDic;
			if( _data.TryGetValue( lang, out langDic) == false)
			{
				return null;
			}

			return langDic;
		}

		private Dictionary<int,List<RefString>>getLangDicTypeList(int lang)
		{
			Dictionary<int, List<RefString>> langDicTypeList;
			if(_dataTypeList.TryGetValue( lang, out langDicTypeList) == false)
			{
				return null;
			}

			return langDicTypeList;
		}


		public string get(int lang, int type,int id)
		{
			RefString ref_string = getRefStringWithFallback(lang, type, id);
			if( ref_string == null)
			{
				return string.Format("#{0}.{1}.{2}", type, id, lang);
			}

			return ref_string.value;
		}

		public string get(int lang,string type,int id)
		{
			int type_hash = EncryptUtil.makeHashCode(type);

			RefString ref_string = getRefStringWithFallback(lang, type_hash, id);
			if( ref_string == null)
			{
				return string.Format("#{0}.{1}.{2}", type, id, lang);
			}

			return ref_string.value;
		}

		public RefString getRefString(int lang,int type,int id)
		{
			Dictionary<long, RefString> dic = getLangDic(lang);
			if( dic == null)
			{
				return null;
			}

			long key = makeKey(type, id);
			RefString ref_string;
			if(dic.TryGetValue(key, out ref_string))
			{
				return ref_string;
			}
			return null;
		}

		public RefString getRefStringWithFallback(int lang,int type,int id)
		{
			RefString ref_string = getRefString(lang, type, id);
			if( ref_string == null)
			{
				ref_string = getRefString(_fallbackLangType, type, id);
			}

			return ref_string;
		}

		public string getFormat(int lang,string type,int id,params object[] args)
		{
			return string.Format( get(lang, type, id), args);
		}

		public string get(string type,int id)
		{
			return get(_currentLangType, type, id);
		}

		public string getFormat(string type,int id,params object[] args)
		{
			return getFormat(_currentLangType, type, id, args);
		}

		// type에 속한 모든 스트링 얻어오기
		public List<RefString> getList(string type)
		{
			return getList(EncryptUtil.makeHashCode(type));
		}

		private static List<RefString> _empty_list = new List<RefString>();

		public List<RefString> getList(int type)
		{
			Dictionary<int, List<RefString>> langDicTypeList = getLangDicTypeList(_currentLangType);
			if( langDicTypeList == null)
			{
				return _empty_list;
			}

			List<RefString> list;
			if( langDicTypeList.TryGetValue(type, out list) == false)
			{
				return _empty_list;
			}

			return list;
		}
	}
}
