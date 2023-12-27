using System;

namespace Festa.Client.Module
{
	public struct DoubleVector3 : IEquatable<DoubleVector3>
	{
		public double x;
		public double y;
		public double z;

		public double magnitude
		{
			get
			{
				return System.Math.Sqrt(x * x + y * y + z * z);
			}
		}

		public double sqrMagnitude
		{
			get
			{
				return x * x + y * y + z * z;
			}
		}

		public DoubleVector3(double xx,double yy,double zz)
		{
			x = xx;
			y = yy;
			z = zz;
		}

		public DoubleVector3 Abs()
		{
			return new DoubleVector3(Math.Abs(x),
									 Math.Abs(y),
									 Math.Abs(z));
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return Equals((DoubleVector3)obj);
		}

		public bool Equals(DoubleVector3 obj)
		{
			return x == obj.x && y == obj.y && z == obj.z;
		}

		public static DoubleVector3 operator + (DoubleVector3 a, DoubleVector3 b)
		{
			return new DoubleVector3(a.x + b.x, a.y + b.y, a.z + b.z);
		}

		public static DoubleVector3 operator - (DoubleVector3 a)
		{
			return new DoubleVector3(-a.x, -a.y, -a.z);
		}

		public static DoubleVector3 operator - (DoubleVector3 a,DoubleVector3 b)
		{
			return new DoubleVector3(a.x - b.x, a.y - b.y, a.z - b.z);
		}

		public static DoubleVector3 operator *(double d,DoubleVector3 a)
		{
			return new DoubleVector3(a.x * d, a.y * d, a.z * d);
		}

		public static DoubleVector3 operator *(DoubleVector3 a,DoubleVector3 b)
		{
			return new DoubleVector3(a.x * b.x, a.y * b.y, a.z * b.z);
		}

		public static DoubleVector3 operator /(DoubleVector3 a,double d)
		{
			return new DoubleVector3(a.x / d, a.y / d, a.z / d);
		}

		public static DoubleVector3 operator /(DoubleVector3 a,DoubleVector3 b)
		{
			return new DoubleVector3(a.x / b.x, a.y / b.y, a.z / b.z);
		}

		public static bool operator == (DoubleVector3 a,DoubleVector3 b)
		{
			return a.x == b.x &&
					a.y == b.y &&
					a.z == b.z;
		}

		public static bool operator !=(DoubleVector3 a, DoubleVector3 b)
		{
			return a.x != b.x ||
					a.y != b.y ||
					a.z != b.z;
		}

		public override string ToString()
		{
			return $"({x},{y},{z})";
		}

		public static DoubleVector3 zero = new DoubleVector3(0, 0, 0);
	}
}
