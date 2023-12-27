using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.MapBox
{
    // 2022.7.22 이강희 최적화를 위해 구조체로 바꿈
    public struct AABB
    {
        private Vector2 _centerPosition;
        public Vector2 CenterPosition
        {
            get { return _centerPosition; }
            set { 
                _centerPosition = value;
                _min = _centerPosition - _size / 2;
                _max = _centerPosition + _size / 2;
            }
        }

        private Vector2 _size;
        public Vector2 Size
        {
            get { return _size; }
            set { 
                _size = value;
                _min = _centerPosition - _size / 2;
                _max = _centerPosition + _size / 2;
            }
        }
        private float _surfaceArea;     // 근데 이사람은 왜 rational 이 아니라 real 을 썼을까???
        private Vector2 _min;
        private Vector3 _max;

        public AABB(Vector2 position, float radius)
        {
            if (radius < 0)
            {
                throw new Exception("negative radius not allowed");
            }

            _centerPosition = position;
            _size = new Vector2(radius * 2f, radius * 2f);
            _surfaceArea = calculateSurfaceArea(_size.x, _size.y);
            _min = _centerPosition - _size / 2;
            _max = _centerPosition + _size / 2;
        }

        public AABB(Vector2 position, Vector2 size)
        {
            _centerPosition = position;
            _size = size;
            _surfaceArea = calculateSurfaceArea(_size.x,_size.y);
            _min = _centerPosition - _size / 2;
            _max = _centerPosition + _size / 2;
        }

        public AABB(float minX, float minY, float maxX, float maxY)
        {
            _centerPosition = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
            _size = new Vector2(maxX - minX, maxY - minY);
            _surfaceArea = calculateSurfaceArea(_size.x,_size.y);
            _min = _centerPosition - _size / 2;
            _max = _centerPosition + _size / 2;
        }

        public AABB(AABB aabb,Vector2 position)
		{
            _centerPosition = aabb._centerPosition + position;
            _size = aabb._size;
            _surfaceArea = aabb._surfaceArea;
            _min = _centerPosition - _size / 2;
            _max = _centerPosition + _size / 2;
        }

        public void reset()
        {
            _centerPosition = Vector2.zero;
            _size = Vector2.zero;
            _surfaceArea = 0f;
        }

        // 집합연산
        public bool overlaps(AABB other)
        {
            return _max.x > other._min.x &&
                   _min.x < other._max.x &&
                   _max.y > other._min.y &&
                   _min.y < other._max.y;
        }

        public bool contains(AABB other)
        {
            Vector2 thisMin = _min;
            Vector2 thisMax = _max;
            Vector2 otherMin = other._min;
            Vector2 otherMax = other._max;

            return otherMin.x >= thisMin.x &&
                    otherMax.x <= thisMax.x &&
                    otherMin.y >= thisMin.y &&
                    otherMax.y <= thisMax.y;
        }

        public AABB union(AABB other)
        {
            Vector2 thisMin = _min;
            Vector2 thisMax = _max;
            Vector2 otherMin = other._min;
            Vector2 otherMax = other._max;

            return new AABB(Mathf.Min(thisMin.x, otherMin.x), Mathf.Min(thisMin.y, otherMin.y),
                        Mathf.Max(thisMax.x, otherMax.x), Mathf.Max(thisMax.y, otherMax.y));
        }

        public AABB intersection(AABB other)
        {
            Vector2 thisMin = _min;
            Vector2 thisMax = _max;
            Vector2 otherMin = other._min;
            Vector2 otherMax = other._max;

            return new AABB(Mathf.Max(thisMin.x, otherMin.x), Mathf.Max(thisMin.y, otherMin.y),
                        Mathf.Min(thisMax.x, otherMax.x), Mathf.Min(thisMax.y, otherMax.y));
        }

        private static float calculateSurfaceArea(float size_x,float size_y)
        {
            return size_x * size_y;
        }

        //private Vector2 getMin()
        //{
        //    return _centerPosition - _size * 0.5f;
        //}

        //private Vector2 getMax()
        //{
        //    return _centerPosition + _size * 0.5f;
        //}

        public float getSurfaceArea()
        {
            return _surfaceArea;
        }
    }
}
