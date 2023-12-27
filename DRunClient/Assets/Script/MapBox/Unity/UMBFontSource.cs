using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Festa.Client.MapBox
{
	[CreateAssetMenu(fileName = "MapBoxFontSource", menuName ="Festa/MapBox/FontSource", order = 4)]
	public class UMBFontSource : ScriptableObject
	{
		[Serializable]
		public class FontSource
		{
			public string name;
			public TMP_FontAsset font;
			public Material fontMaterial;

			public void set(TMP_Text text)
			{
				text.font = font;
				text.fontMaterial = fontMaterial;
			}
		}

		public List<FontSource> fontList;

		private Dictionary<int, FontSource> _fontMap;

		public FontSource getSource(int font_hash)
		{
			if( font_hash == 0)
			{
				return fontList[0];
			}

			if( _fontMap == null)
			{
				buildFontMap();
			}

			FontSource source;
			if( _fontMap.TryGetValue(font_hash, out source) )
			{
				return source;
			}
			return fontList[0];
		}

		private void buildFontMap()
		{
			_fontMap = new Dictionary<int, FontSource>();
			for(int i = 0; i < fontList.Count; i++)
			{
				FontSource font = fontList[i];
				_fontMap.Add(EncryptUtil.makeHashCode(font.name), font);
			}
		}
	}
}
