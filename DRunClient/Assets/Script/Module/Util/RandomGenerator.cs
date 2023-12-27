using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Module.Navigation;
//using fix32 = System.Int64;

namespace Festa.Client.Module
{

	public class RandomGenerator {
		
		public static RandomGenerator create(int seed)
		{
			RandomGenerator r = new RandomGenerator();
			r.init( seed);
			return r;
		}
		
		public const int RAND_MAX = 32767;
		private int	_next = 1;
		//private static fix32 fix32_RANDOM_MAX = Fixed64.FromInt( RAND_MAX);
		
		private void init(int seed)
		{
			_next = seed;
		}
		
		public int nextInt()
		{
			_next = _next * 1103515245 + 12345;
			return (_next >> 16) & RAND_MAX;
		}

		public int nextInt(int begin, int end)
		{
			int bound = end - begin + 1;
			int r = nextInt();

			return begin + (r % bound);
		}

		public double nextDouble()
		{
			int v = nextInt();
			return (double)v / (double)RAND_MAX;
		}

		public bool nextBoolean(double ratio)
		{
			return nextDouble() <= ratio;
		}

		public T select<T>(List<T> list) where T : class
		{
			int id = nextInt(0, list.Count - 1);
			return list[id];
		}

        //public fix32 nextFix32()
        //{
        //    return Fixed64.Div(nextInt(), fix32_RANDOM_MAX);
        //}

        //public fix32 nextFix32(fix32 begin,fix32 end)
        //{
        //    return begin + Fixed64.Mul(nextFix32(), (end - begin));
        //}
	}

	
}
