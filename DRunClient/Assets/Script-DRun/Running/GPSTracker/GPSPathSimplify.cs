using Festa.Client.MapBox;
using Festa.Client.Module;
using System.Collections.Generic;

namespace DRun.Client.Running
{
	public class GPSPathSimplify
	{
		public const double defaultThreshold = 10.0 / 4096.0;

		// square distance between 2 PolylinePoints
		private static double GetSquareDistance(GPSTilePosition p1, GPSTilePosition p2)
		{
			return (p1.tile_pos - p2.tile_pos).sqrMagnitude;
		}

		// square distance from a PolylinePoint to a segment
		private static double GetSquareSegmentDistance(GPSTilePosition p, GPSTilePosition p1, GPSTilePosition p2)
		{
			double x = p1.tile_pos.x;
			double y = p1.tile_pos.y;
			double dx = p2.tile_pos.x - x;
			double dy = p2.tile_pos.y - y;

			if (dx != 0.0 || dy != 0.0)
			{
				var t = ((p.tile_pos.x - x) * dx + (p.tile_pos.y - y) * dy) / (dx * dx + dy * dy);

				if (t > 1)
				{
					x = p2.tile_pos.x;
					y = p2.tile_pos.y;
				}
				else if (t > 0)
				{
					x += dx * t;
					y += dy * t;
				}
			}

			dx = p.tile_pos.x - x;
			dy = p.tile_pos.y - y;

			return (dx * dx) + (dy * dy);
		}

		// rest of the code doesn't care about PolylinePoint format

		// basic distance-based simplification
		private static List<GPSTilePosition> SimplifyRadialDistance(List<GPSTilePosition> PolylinePoints, double sqTolerance)
		{
			var prevPolylinePoint = PolylinePoints[0];
			var newPolylinePoints = new List<GPSTilePosition> { prevPolylinePoint };
			GPSTilePosition PolylinePoint = null ;

			for (var i = 1; i < PolylinePoints.Count; i++)
			{
				PolylinePoint = PolylinePoints[i];

				double distance = GetSquareDistance(PolylinePoint, prevPolylinePoint);

				if ( distance > sqTolerance)
				{
					newPolylinePoints.Add(PolylinePoint);
					prevPolylinePoint = PolylinePoint;
				}
			}

			if (PolylinePoint != null && !prevPolylinePoint.Equals(PolylinePoint))
				newPolylinePoints.Add(PolylinePoint);

			return newPolylinePoints;
		}

		// simplification using optimized Douglas-Peucker algorithm with recursion elimination
		private static List<GPSTilePosition> SimplifyDouglasPeucker(List<GPSTilePosition> PolylinePoints, double sqTolerance)
		{
			var len = PolylinePoints.Count;
			var markers = new int?[len];
			int? first = 0;
			int? last = len - 1;
			int? index = 0;
			var stack = new List<int?>();
			var newPolylinePoints = new List<GPSTilePosition>();

			markers[first.Value] = markers[last.Value] = 1;

			while (last != null)
			{
				var maxSqDist = 0.0d;

				for (int? i = first + 1; i < last; i++)
				{
					var sqDist = GetSquareSegmentDistance(PolylinePoints[i.Value], PolylinePoints[first.Value], PolylinePoints[last.Value]);

					if (sqDist > maxSqDist)
					{
						index = i;
						maxSqDist = sqDist;
					}
				}

				if (maxSqDist > sqTolerance)
				{
					markers[index.Value] = 1;
					stack.AddRange(new[] { first, index, index, last });
				}


				if (stack.Count > 0)
				{
					last = stack[stack.Count - 1];
					stack.RemoveAt(stack.Count - 1);
				}
				else
					last = null;

				if (stack.Count > 0)
				{
					first = stack[stack.Count - 1];
					stack.RemoveAt(stack.Count - 1);
				}
				else
					first = null;
			}

			for (var i = 0; i < len; i++)
			{
				if (markers[i] != null)
					newPolylinePoints.Add(PolylinePoints[i]);
			}

			return newPolylinePoints;
		}

		/// <summary>
		/// Simplifies a list of PolylinePoints to a shorter list of PolylinePoints.
		/// </summary>
		/// <param name="PolylinePoints">PolylinePoints original list of PolylinePoints</param>
		/// <param name="tolerance">Tolerance tolerance in the same measurement as the PolylinePoint coordinates</param>
		/// <param name="highestQuality">Enable highest quality for using Douglas-Peucker, set false for Radial-Distance algorithm</param>
		/// <returns>Simplified list of PolylinePoints</returns>
		public static List<GPSTilePosition> Simplify(List<GPSTilePosition> PolylinePoints, double tolerance = 0.3, bool highestQuality = false)
		{
			if (PolylinePoints == null || PolylinePoints.Count == 0)
				return new List<GPSTilePosition>();

			var sqTolerance = tolerance * tolerance;

			if (highestQuality)
				return SimplifyDouglasPeucker(PolylinePoints, sqTolerance);

			//List<GPSTilePosition> PolylinePoints2 = SimplifyRadialDistance(PolylinePoints, sqTolerance);
			//return SimplifyDouglasPeucker(PolylinePoints2, sqTolerance);

			// 정 직선 코스를 뭉게는 이슈가 있다
			return SimplifyRadialDistance(PolylinePoints, sqTolerance);
		}

		///// <summary>
		///// Simplifies a list of PolylinePoints to a shorter list of PolylinePoints.
		///// </summary>
		///// <param name="PolylinePoints">PolylinePoints original list of PolylinePoints</param>
		///// <param name="tolerance">Tolerance tolerance in the same measurement as the PolylinePoint coordinates</param>
		///// <param name="highestQuality">Enable highest quality for using Douglas-Peucker, set false for Radial-Distance algorithm</param>
		///// <returns>Simplified list of PolylinePoints</returns>
		//public static List<PolylinePoint> SimplifyArray(PolylinePoint[] PolylinePoints, double tolerance = 0.3, bool highestQuality = false)
		//{
		//    return new SimplifyUtility().Simplify(PolylinePoints, tolerance, highestQuality);
		//}
	}
}
