using System;
using UnityEngine;

namespace Festa.Client.Module
{
	public struct DoubleVector2 : IEquatable<DoubleVector2>
	{
		public double x;
		public double y;

		public double magnitude
		{
			get
			{
				return System.Math.Sqrt(x * x + y * y);
			}
		}

		public double sqrMagnitude
		{
			get
			{
				return x * x + y * y;
			}
		}

		public DoubleVector2(double xx,double yy)
		{
			x = xx;
			y = yy;
		}

		public DoubleVector2 Abs()
		{
			return new DoubleVector2(System.Math.Abs(x), System.Math.Abs(y));
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return Equals((DoubleVector2)obj);
		}

		public bool Equals(DoubleVector2 obj)
		{
			return x == obj.x && y == obj.y;
		}

		public static DoubleVector2 operator +(DoubleVector2 a, DoubleVector2 b)
		{
			return new DoubleVector2(a.x + b.x, a.y + b.y);
		}

		public static DoubleVector2 operator -(DoubleVector2 a)
		{
			return new DoubleVector2(-a.x, -a.y);
		}
		public static DoubleVector2 operator -(DoubleVector2 a, DoubleVector2 b)
		{
			return new DoubleVector2(a.x - b.x, a.y - b.y);
		}
		public static DoubleVector2 operator *(double d, DoubleVector2 a)
		{
			return new DoubleVector2(a.x * d, a.y * d);
		}
		public static DoubleVector2 operator *(DoubleVector2 a, double d)
		{
			return new DoubleVector2(a.x * d, a.y * d);
		}
		public static DoubleVector2 operator *(DoubleVector2 a, DoubleVector2 b)
		{
			return new DoubleVector2(a.x * b.x, a.y * b.y);
		}
		public static DoubleVector2 operator /(DoubleVector2 a, double d)
		{
			return new DoubleVector2(a.x / d, a.y / d);
		}
		public static DoubleVector2 operator /(DoubleVector2 a, DoubleVector2 b)
		{
			return new DoubleVector2(a.x / b.x, a.y / b.y);
		}
		public static bool operator ==(DoubleVector2 lhs, DoubleVector2 rhs)
		{
			return lhs.x == rhs.x &&
					lhs.y == rhs.y;
		}
		public static bool operator !=(DoubleVector2 lhs, DoubleVector2 rhs)
		{
			return lhs.x != rhs.x ||
					lhs.y != rhs.y;
		}

		public static implicit operator Vector2Int(DoubleVector2 v)
		{
			return new Vector2Int((int)v.x, (int)v.y);
		}


		public override string ToString()
		{
			return $"({x}, {y})";
		}

		public static DoubleVector2 zero = new DoubleVector2(0, 0);
	}
}