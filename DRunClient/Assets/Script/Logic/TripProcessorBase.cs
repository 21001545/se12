using Festa.Client.Module;
using Festa.Client.Module.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.Logic
{
	public abstract class TripProcessorBase : BaseStepProcessor
	{
		protected ClientNetwork Network => ClientMain.instance.getNetwork();
		protected ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		//protected ClientLocationManager LocationManager => ClientMain.instance.getLocation();
		protected ClientHealthManager HealthManager => ClientMain.instance.getHealth();

		protected void flushHealth(Handler<AsyncResult<Module.Void>> handler)
		{
			HealthManager.queryAndFlushNow(() => {
				handler(Future.succeededFuture());
			});
		}

		protected void flushLocation(Handler<AsyncResult<Module.Void>> handler)
		{
			//LocationManager.flushNow(true, () => {
			//	handler(Future.succeededFuture());
			//});
		}
	}
}
