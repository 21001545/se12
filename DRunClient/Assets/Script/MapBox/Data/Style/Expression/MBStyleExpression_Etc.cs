using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.MapBox.Expression
{
	public class Etc_Zoom : MBStyleExpression
	{
		public override object evaluate(MBStyleExpressionContext ctx)
		{
			return ctx._zoom;
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			
		}
	}

	public class Etc_HeatmapDensity : MBStyleExpression
	{
		public override object evaluate(MBStyleExpressionContext ctx)
		{
			throw new NotImplementedException();
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			throw new NotImplementedException();
		}
	}
}
