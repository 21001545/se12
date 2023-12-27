using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.Module
{
	public static class MathUtil
	{
        public static double SmoothDamp(double current, double target, ref double currentVelocity, double smoothTime, double maxSpeed, double deltaTime)
        {
            smoothTime = System.Math.Max(0.0001, smoothTime);
            double num = 2.0 / smoothTime;
            double num2 = num * deltaTime;
            double num3 = 1.0 / (1.0 + num2 + 0.48 * num2 * num2 + 0.235 * num2 * num2 * num2);
            double num4 = current - target;
            double num5 = target;
            double num6 = maxSpeed * smoothTime;
            num4 = Clamp(num4, -num6, num6);
            target = current - num4;
            double num7 = (currentVelocity + num * num4) * deltaTime;
            currentVelocity = (currentVelocity - num * num7) * num3;
            double num8 = target + (num4 + num7) * num3;
            if (num5 - current > 0 == num8 > num5)
            {
                num8 = num5;
                currentVelocity = (num8 - num5) / deltaTime;
            }
            return num8;
        }

        public static double Clamp(double value,double min,double max)
		{
            if (value < min)
			{
                return min;
			}
            if( value > max)
			{
                return max;
			}
            return value;
		}

        public static DoubleVector2 SmoothDamp(DoubleVector2 current, DoubleVector2 target, ref DoubleVector2 currentVelocity, double smoothTime, double maxSpeed, double deltaTime)
        {
            // Based on Game Programming Gems 4 Chapter 1.10
            smoothTime = System.Math.Max(0.0001, smoothTime);
            double omega = 2.0 / smoothTime;

            double x = omega * deltaTime;
            double exp = 1.0 / (1.0 + x + 0.48 * x * x + 0.235 * x * x * x);

            double change_x = current.x - target.x;
            double change_y = current.y - target.y;
            DoubleVector2 originalTo = target;

            // Clamp maximum speed
            double maxChange = maxSpeed * smoothTime;

            double maxChangeSq = maxChange * maxChange;
            double sqDist = change_x * change_x + change_y * change_y;
            if (sqDist > maxChangeSq)
            {
                var mag = Math.Sqrt(sqDist);
                change_x = change_x / mag * maxChange;
                change_y = change_y / mag * maxChange;
            }

            target.x = current.x - change_x;
            target.y = current.y - change_y;

            double temp_x = (currentVelocity.x + omega * change_x) * deltaTime;
            double temp_y = (currentVelocity.y + omega * change_y) * deltaTime;

            currentVelocity.x = (currentVelocity.x - omega * temp_x) * exp;
            currentVelocity.y = (currentVelocity.y - omega * temp_y) * exp;

            double output_x = target.x + (change_x + temp_x) * exp;
            double output_y = target.y + (change_y + temp_y) * exp;

            // Prevent overshooting
            double origMinusCurrent_x = originalTo.x - current.x;
            double origMinusCurrent_y = originalTo.y - current.y;
            double outMinusOrig_x = output_x - originalTo.x;
            double outMinusOrig_y = output_y - originalTo.y;

            if (origMinusCurrent_x * outMinusOrig_x + origMinusCurrent_y * outMinusOrig_y > 0)
            {
                output_x = originalTo.x;
                output_y = originalTo.y;

                currentVelocity.x = (output_x - originalTo.x) / deltaTime;
                currentVelocity.y = (output_y - originalTo.y) / deltaTime;
            }
            return new DoubleVector2(output_x, output_y);
        }
    }
}
