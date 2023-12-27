using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client
{
	// Garbage Collector를 활용해볼까도 했지만, 그냥 명시적으로 하는게 좋을것 같다
	public class TextureCacheItemUsage
	{
		public int key;
		public Texture texture;

		public TextureCacheItemUsage(TextureCacheItem item)
		{
			key = item.getKey();
			texture = item.getTexture();
		}

		public TextureCacheItemUsage(int key,Texture texture)
		{
			this.key = key;
			this.texture = texture;
		}
	}
}
