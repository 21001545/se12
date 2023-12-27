using Festa.Client.Module.Events;
using Festa.Client.Module;
using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class MBStyleLoaderOld
	{
		private string _text;

		private MBStyleExpressionParser _expressionParser;
		private MBStyle _style;

		public MBStyle getStyle()
		{
			return _style;
		}

		public static MBStyleLoaderOld create(string text)
		{
			MBStyleLoaderOld loader = new MBStyleLoaderOld();
			loader.init(text);
			return loader;
		}

		private void init(string text)
		{
			_text = text;
			_expressionParser = MBStyleExpressionParser.create(MBStyleExpressionFactory.create());
		}

		public void run(Handler<bool> callback)
		{
			ClientMain.instance.getMultiThreadWorker().execute<MBStyle>(promise => { 

				try
				{
					JsonObject json = new JsonObject(_text);
					MBStyle style = MBStyle.create(json, _expressionParser);
					promise.complete(style);
				}
				catch(Exception e)
				{
					promise.fail(e);
				}

			}, result => { 
				if( result.failed())
				{
					Debug.LogException(result.cause());
					callback(false);
				}
				else
				{
					_style = result.result();
					callback(true);
				}
			});
		}
	}
}
