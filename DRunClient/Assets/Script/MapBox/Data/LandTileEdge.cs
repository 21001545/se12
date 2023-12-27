using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
    public class LandTileEdge
    {
        public class EdgeType
        {
            public const int left = 0;
            public const int right = 1;
            public const int top = 2;
            public const int bottom = 3;

            public static int[] adjaceny_type = new int[]
            {
                right,
                left,
                bottom,
                top
            };
        }

        private LandTileFeature _feature;
        private int _type;
        private Vector2Int _range;
    
        public int getType()
        {
            return _type;
        }

        public LandTileFeature getFeature()
        {
            return _feature;
        }

        public static LandTileEdge create(LandTileFeature owner,int type,int a, int b)
        {
            LandTileEdge edge = new LandTileEdge();
            edge.init(owner, type, a, b);
            return edge;
        }

        private void init(LandTileFeature owner,int type, int a, int b)
        {
            _feature = owner;
            _type = type;
            _range = new Vector2Int(System.Math.Min(a, b), System.Math.Max(a, b));
        }

        public static bool overlap(LandTileEdge a,LandTileEdge b)
        {
            if(a._range.y < b._range.x)
            {
                return false;
            }

            if(a._range.x > b._range.y)
            {
                return false;
            }

            return true;
        }
        
    }
}
