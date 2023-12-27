using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.MapBox.Expression
{
	public class Feature_Accumulated : MBStyleExpression
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

	public class Feature_FeatureState : MBStyleExpression
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

	public class Feature_GeomtryType : MBStyleExpression
	{
		public override object evaluate(MBStyleExpressionContext ctx)
		{
			if( ctx._feature.type == MBFeatureType.polygon)
			{
				return "Polygon";
			}
			else if( ctx._feature.type == MBFeatureType.linestring)
			{
				return "LineString";
			}
			else if( ctx._feature.type == MBFeatureType.point)
			{
				return "Point";
			}

			return "Unknown";
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			
		}
	}

	public class Feature_ID : MBStyleExpression
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

	public class Feature_LineProgress : MBStyleExpression
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

	public class Feature_Properties : MBStyleExpression
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
