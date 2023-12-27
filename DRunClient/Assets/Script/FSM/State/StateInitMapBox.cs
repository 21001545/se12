using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client
{
	public class StateInitMapBox : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.init_mapbox;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			UILoading.getInstance().setProgress("initialize mapbox...", 14);

			loadStyleList();
		}

		private void loadStyleList()
		{
			MBStyleListLoader listLoader = MBStyleListLoader.create(ClientMain.instance);
			listLoader.run(result => { 
			
				if( result.failed())
				{
					UIPopup.spawnOK("load mapbox style fail", () => { _owner.changeState(ClientStateType.sleep); });
				}
				else
				{
					Dictionary<string, DateTime> modifiedTimeMap = listLoader.getStyleModifiedTime();

					List<string> styleList = new List<string>();
					styleList.Add(MBAccess.defaultStyle);

					loadStyleIter(styleList.GetEnumerator(), modifiedTimeMap, result => {
						if (result.failed())
						{
							UIPopup.spawnOK("load mapbox style fail", () => {
								_owner.changeState(ClientStateType.sleep);
							});
						}
						else
						{
							changeToNextState();
						}
					});

				}
			});

		}

		private void loadStyleIter(List<string>.Enumerator e,Dictionary<string,DateTime> modifiedTimeMap,Handler<AsyncResult<Module.Void>> handler)
		{
			if( e.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}
			
			string style_id = e.Current;
			float begin_time = Time.realtimeSinceStartup;

			MBStyleLoader loader = MBStyleLoader.create(style_id, modifiedTimeMap, ClientMain.instance);
			loader.run(result => {

				if( result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					float delta_time = Time.realtimeSinceStartup - begin_time;
					//Debug.Log($"load mapbox style[{style_id}] {delta_time}s");

					ClientMain.instance.getMBStyleCache().put(style_id, loader.getMBStyle());
					loadStyleIter(e, modifiedTimeMap, handler);
				}
			});
		}
	}
}
