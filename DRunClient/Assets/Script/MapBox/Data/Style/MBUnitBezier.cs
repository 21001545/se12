using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.MapBox
{
	public class MBUnitBezier
	{
        private double cx;
        private double bx;
        private double ax;

        private double cy;
        private double by;
        private double ay;

        public MBUnitBezier(double p1x, double p1y, double p2x, double p2y)
        {
            cx = 3.0 * p1x;
            bx = 3.0 * (p2x - p1x) - (3.0 * p1x);
            ax = 1.0 - (3.0 * p1x) - (3.0 * (p2x - p1x) - (3.0 * p1x));
            cy = 3.0 * p1y;
            by = 3.0 * (p2y - p1y) - (3.0 * p1y);
            ay = 1.0 - (3.0 * p1y) - (3.0 * (p2y - p1y) - (3.0 * p1y));
        }

        DoubleVector2 getP1()
		{
            return new DoubleVector2(cx / 3.0, cy / 3.0);
		}

        DoubleVector2 getP2()
		{
            return new DoubleVector2((bx + (3.0 * cx / 3.0) + cx) / 3.0,
                                     (by + (3.0 * cy / 3.0) + cy) / 3.0);
		}

        double sampleCurveX(double t) 
        {
            // `ax t^3 + bx t^2 + cx t' expanded using Horner's rule.
            return ((ax* t + bx) * t + cx) * t;
        }

        double sampleCurveY(double t)
        {
            return ((ay* t + by) * t + cy) * t;
        }

        double sampleCurveDerivativeX(double t)
        {
            return (3.0 * ax * t + 2.0 * bx) *t + cx;
        }

        double solveCurveX(double x,double epsilon)
		{
            double t0;
            double t1;
            double t2;
            double x2;
            double d2;
            int i;

            // First try a few iterations of Newton's method -- normally very fast.
            for (t2 = x, i = 0; i < 8; ++i)
            {
                x2 = sampleCurveX(t2) - x;
                if (System.Math.Abs(x2) < epsilon)
                    return t2;
                d2 = sampleCurveDerivativeX(t2);
                if (System.Math.Abs(d2) < 1e-6)
                    break;
                t2 = t2 - x2 / d2;
            }

            // Fall back to the bisection method for reliability.
            t0 = 0.0;
            t1 = 1.0;
            t2 = x;

            if (t2 < t0)
                return t0;
            if (t2 > t1)
                return t1;

            while (t0 < t1)
            {
                x2 = sampleCurveX(t2);
                if (System.Math.Abs(x2 - x) < epsilon)
                    return t2;
                if (x > x2)
                    t0 = t2;
                else
                    t1 = t2;
                t2 = (t1 - t0) * .5 + t0;
            }

            // Failure.
            return t2;
        }

        public double solve(double x, double epsilon) {
            return sampleCurveY(solveCurveX(x, epsilon));
        }
}
}
