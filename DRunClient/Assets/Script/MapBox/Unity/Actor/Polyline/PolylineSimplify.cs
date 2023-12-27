using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.MapBox
{
	public static class PolylineSimplify
	{
        // square distance between 2 PolylinePoints
        private static int GetSquareDistance(PolylinePoint p1, PolylinePoint p2)
        {
            return (p1.position - p2.position).sqrMagnitude;
        }

        // square distance from a PolylinePoint to a segment
        private static double GetSquareSegmentDistance(PolylinePoint p, PolylinePoint p1, PolylinePoint p2)
        {
            int x = p1.position.x;
            int y = p1.position.y;
            int dx = p2.position.x - x;
            int dy = p2.position.y - y;

            if (!dx.Equals(0.0) || !dy.Equals(0.0))
            {
                var t = ((p.position.x - x) * dx + (p.position.y - y) * dy) / (dx * dx + dy * dy);

                if (t > 1)
                {
                    x = p2.position.x;
                    y = p2.position.y;
                }
                else if (t > 0)
                {
                    x += dx * t;
                    y += dy * t;
                }
            }

            dx = p.position.x - x;
            dy = p.position.y - y;

            return (dx * dx) + (dy * dy);
        }

        // rest of the code doesn't care about PolylinePoint format

        // basic distance-based simplification
        private static List<PolylinePoint> SimplifyRadialDistance(List<PolylinePoint> PolylinePoints, double sqTolerance)
        {
            var prevPolylinePoint = PolylinePoints[0];
            var newPolylinePoints = new List<PolylinePoint> { prevPolylinePoint };
            PolylinePoint PolylinePoint = null;

            for (var i = 1; i < PolylinePoints.Count; i++)
            {
                PolylinePoint = PolylinePoints[i];

                if (GetSquareDistance(PolylinePoint, prevPolylinePoint) > sqTolerance)
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
        private static List<PolylinePoint> SimplifyDouglasPeucker(List<PolylinePoint> PolylinePoints, double sqTolerance)
        {
            var len = PolylinePoints.Count;
            var markers = new int?[len];
            int? first = 0;
            int? last = len - 1;
            int? index = 0;
            var stack = new List<int?>();
            var newPolylinePoints = new List<PolylinePoint>();

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
        public static List<PolylinePoint> Simplify(List<PolylinePoint> PolylinePoints, double tolerance = 0.3, bool highestQuality = false)
        {
            if (PolylinePoints == null || PolylinePoints.Count == 0)
                return new List<PolylinePoint>();

            var sqTolerance = tolerance * tolerance;

            if (highestQuality)
                return SimplifyDouglasPeucker(PolylinePoints, sqTolerance);

            List<PolylinePoint> PolylinePoints2 = SimplifyRadialDistance(PolylinePoints, sqTolerance);
            return SimplifyDouglasPeucker(PolylinePoints2, sqTolerance);
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
