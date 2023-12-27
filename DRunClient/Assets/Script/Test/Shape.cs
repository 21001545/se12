using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public abstract class Shape
	{
		public enum ShapeType
		{
			Circle = 0,
			Polygon = 1
		}

		public abstract ShapeType getType();
		public abstract AABB getAABB(Vector2 position);
	}

	public class CircleShape : Shape
	{
		public float _radius;

		public override ShapeType getType()
		{
			return ShapeType.Circle;
		}

		public float getRadius()
		{
			return _radius;
		}

		public void setRadius(float radius)
		{
			_radius = radius;
		}

		public override AABB getAABB(Vector2 position)
		{
			return new AABB(position, _radius);
		}

		public static CircleShape create(float radius)
		{
			CircleShape shape = new CircleShape();
			shape._radius = radius;
			return shape;
		}
	}

	public class PolygonShape : Shape
	{
		private List<Vector2> _vertices;
		private List<Vector2> _normals;
		private AABB _localAABB;

		public override ShapeType getType()
		{
			return ShapeType.Polygon;
		}

		public List<Vector2> getVertices()
		{
			return _vertices;
		}

		public List<Vector2> getNormals()
		{
			return _normals;
		}

		public AABB getLocalAABB()
		{
			return _localAABB;
		}

		public static PolygonShape createBox(Vector2 offset,Vector2 extent)
		{
			PolygonShape shape = new PolygonShape();
			shape.setBox(offset, extent);
			return shape;
		}

		public void setBox(Vector2 offset,Vector2 extent)
		{
			_vertices = new List<Vector2>();
			_normals = new List<Vector2>();

			float hw = extent.x;
			float hh = extent.y;

			_vertices.Add(new Vector2(-hw, -hh));
			_vertices.Add(new Vector2(hw, -hh));
			_vertices.Add(new Vector2(hw, hh));
			_vertices.Add(new Vector2(-hw, hh));

			_normals.Add(new Vector2(0, -1));
			_normals.Add(new Vector2(1, 0));
			_normals.Add(new Vector2(0, 1));
			_normals.Add(new Vector2(-1, 0));
		
			_localAABB = new AABB(offset, extent * 2);
		}

		public override AABB getAABB(Vector2 position)
		{
			return new AABB(_localAABB, position);
		}

		public Vector2 getSupport(Vector2 dir)
		{
			float bestProjection = float.MinValue;
			Vector2 bestVertex = Vector2.zero;

			for(int i = 0; i < _vertices.Count; ++i)
			{
				Vector2 v = _vertices[i];
				float projection = Vector2.Dot(v, dir);

				if( projection > bestProjection)
				{
					bestVertex = v;
					bestProjection = projection;
				}
			}

			return bestVertex;
		}
	}

	/*

	class PolygonShape : public Shape,public ResuableObject<PolygonShape>
{
public:
	void ComputeMass(real density, Body* body)
	{
		// Calculate centroid and moment of interia
		Vec2 c(Fixed64::Zero, Fixed64::Zero ); // centroid
		real area = Fixed64::Zero;
		real I = Fixed64::Zero;
		const real k_inv3 = Fixed64::Div(Fixed64::One, Fixed64::FromFloat(3.0f));

		for (uint32 i1 = 0; i1 < m_vertexCount; ++i1)
		{
			// Triangle vertices, third vertex implied as (0, 0)
			Vec2 p1(m_vertices[i1] );
			uint32 i2 = i1 + 1 < m_vertexCount ? i1 + 1 : 0;
			Vec2 p2(m_vertices[i2] );

			real D = Cross(p1, p2);
			real triangleArea = Fixed64::Mul(Fixed64::Half, D);

			area += triangleArea;

			// Use area to weight the centroid average, not just vertex position
			c += Fixed64::Mul(triangleArea, k_inv3) * (p1 + p2);

			real intx2 = Fixed64::Mul(p1.x, p1.x) + Fixed64::Mul(p2.x, p1.x) + Fixed64::Mul(p2.x, p2.x);
			real inty2 = Fixed64::Mul(p1.y, p1.y) + Fixed64::Mul(p2.y, p1.y) + Fixed64::Mul(p2.y, p2.y);
			I += Fixed64::Mul(Fixed64::FromFloat(0.25f), Fixed64::Mul(k_inv3, D)) * (intx2 + inty2);
		}

		c = c * Fixed64::Div(Fixed64::One, area);

		// Translate vertices to centroid (make the centroid (0, 0)
		// for the polygon in model space)
		// Not really necessary, but I like doing this anyway
		for (uint32 i = 0; i < m_vertexCount; ++i)
			m_vertices[i] -= c;

		body->m = Fixed64::Mul(density, area);
		body->im = (body->m) ? Fixed64::Div(Fixed64::One, body->m) : Fixed64::Zero;
		body->I = Fixed64::Mul(I, density);
		body->iI = body->I ? Fixed64::Div(Fixed64::One, body->I) : Fixed64::Zero;
	}

	void setMass(real mass, Body* body)
	{
		// TODO
		body->I = Fixed64::Zero;
		body->iI = Fixed64::Zero;
		body->m = Fixed64::Zero;
		body->im = Fixed64::Zero;

		//		body->m = mass;
		//		body->im = (body->m) ? Fixed64::Div(Fixed64::One , body->m) : Fixed64::Zero;
		//		body->I = Fixed64::Mul(body->m , Fixed64::Sqradius));
		//		body->iI = (body->I) ? Fixed64::Div(Fixed64::One , body->I) : Fixed64::Zero;
	}

	void SetOrient(real radians)
	{
		u.Set(radians);
	}

	Type GetType(void ) const
	{
		return ePoly;
	}

// Half width and half height
void setBox(real hw, real hh)
{
	m_vertexCount = 4;
	m_vertices[0].Set(-hw, -hh);
	m_vertices[1].Set(hw, -hh);
	m_vertices[2].Set(hw, hh);
	m_vertices[3].Set(-hw, hh);
	m_normals[0].Set(Fixed64::FromFloat(0.0f), Fixed64::FromFloat(-1.0f));
	m_normals[1].Set(Fixed64::FromFloat(1.0f), Fixed64::FromFloat(0.0f));
	m_normals[2].Set(Fixed64::FromFloat(0.0f), Fixed64::FromFloat(1.0f));
	m_normals[3].Set(Fixed64::FromFloat(-1.0f), Fixed64::FromFloat(0.0f));
	m_localAABB = AABB(-hw, -hh, hw, hh);
}

void Set(Vec2* vertices, uint32 count)
{
	// No hulls with less than 3 vertices (ensure actual polygon)
	assert(count > 2 && count <= MaxPolyVertexCount);
	count = std::min((int32)count, MaxPolyVertexCount);

	// Find the right most point on the hull
	int32 rightMost = 0;
	real highestXCoord = vertices[0].x;
	for (uint32 i = 1; i < count; ++i)
	{
		real x = vertices[i].x;
		if (x > highestXCoord)
		{
			highestXCoord = x;
			rightMost = i;
		}

		// If matching x then take farthest negative y
		else if (x == highestXCoord)
			if (vertices[i].y < vertices[rightMost].y)
				rightMost = i;
	}

	int32 hull[MaxPolyVertexCount];
	int32 outCount = 0;
	int32 indexHull = rightMost;

	for (; ; )
	{
		hull[outCount] = indexHull;

		// Search for next index that wraps around the hull
		// by computing cross products to find the most counter-clockwise
		// vertex in the set, given the previos hull index
		int32 nextHullIndex = 0;
		for (int32 i = 1; i < (int32)count; ++i)
		{
			// Skip if same coordinate as we need three unique
			// points in the set to perform a cross product
			if (nextHullIndex == indexHull)
			{
				nextHullIndex = i;
				continue;
			}

			// Cross every set of three unique vertices
			// Record each counter clockwise third vertex and add
			// to the output hull
			// See : http://www.oocities.org/pcgpe/math2d.html
			Vec2 e1 = vertices[nextHullIndex] - vertices[hull[outCount]];
			Vec2 e2 = vertices[i] - vertices[hull[outCount]];
			real c = Cross(e1, e2);
			if (c < Fixed64::Zero)
				nextHullIndex = i;

			// Cross product is Fixed64::Zero then e vectors are on same line
			// therefor want to record vertex farthest along that line
			if (c == Fixed64::Zero && e2.LenSqr() > e1.LenSqr())
				nextHullIndex = i;
		}

		++outCount;
		indexHull = nextHullIndex;

		// Conclude algorithm upon wrap-around
		if (nextHullIndex == rightMost)
		{
			m_vertexCount = outCount;
			break;
		}
	}

	// Copy vertices into shape's vertices
	for (uint32 i = 0; i < m_vertexCount; ++i)
		m_vertices[i] = vertices[hull[i]];

	// Compute face normals
	for (uint32 i1 = 0; i1 < m_vertexCount; ++i1)
	{
		uint32 i2 = i1 + 1 < m_vertexCount ? i1 + 1 : 0;
		Vec2 face = m_vertices[i2] - m_vertices[i1];

		// Ensure no Fixed64::Zero-length edges, because that's bad
		assert(face.LenSqr() > Fixed64::Sq(Fixed64::Epsilon));

		// Calculate normal with 2D cross product between vector and scalar
		m_normals[i1] = Vec2(face.y, -face.x);
		m_normals[i1].Normalize();
	}
}

// The extreme point along a direction within a polygon
Vec2 GetSupport( const Vec2& dir )
{
	real bestProjection = Fixed64::MinValue;
	Vec2 bestVertex;

	for (uint32 i = 0; i < m_vertexCount; ++i)
	{
		Vec2 v = m_vertices[i];
		real projection = Dot(v, dir);

		if (projection > bestProjection)
		{
			bestVertex = v;
			bestProjection = projection;
		}
	}

	return bestVertex;
}

// only support AABB Box (not support orient)
AABB calcAABB(Vec2& position)
{
	return AABB(m_localAABB._min + position, m_localAABB._max + position);
}

uint32 m_vertexCount;
Vec2 m_vertices[MaxPolyVertexCount];
Vec2 m_normals[MaxPolyVertexCount];
AABB m_localAABB;
};

	*/


    //public abstract class AABBShape
    //{
    //    public enum Type
    //    {
    //        Circle,
    //        Square
    //    }

    //    [SerializeField]
    //    protected Vector2 _offset;
    //    public Vector2 Offset
    //    {
    //        get { return _offset; }
    //        set { _offset = value; }
    //    }

    //    public abstract Type getShapeType();
    //    public abstract AABB calculateAABB(Vector2 position);
    //}

    //[System.Serializable]
    //public class CircleShape : AABBShape
    //{
    //    [SerializeField]
    //    protected float _radius;

    //    public CircleShape() { }

    //    public CircleShape(float radius)
    //    {
    //        _radius = radius;
    //    }

    //    public void setRadius(float radius)
    //    {
    //        _radius = radius;
    //    }

    //    public float getRadius()
    //    {
    //        return _radius;
    //    }

    //    public override Type getShapeType()
    //    {
    //        return Type.Circle;
    //    }

    //    public override AABB calculateAABB(Vector2 position)
    //    {
    //        return new AABB(position, _radius);
    //    }
    //}

    //[System.Serializable]
    //public class SquareShape : AABBShape
    //{
    //    [SerializeField]
    //    protected Vector2 _size;

    //    public SquareShape() { }

    //    public override Type getShapeType()
    //    {
    //        return Type.Square;
    //    }

    //    public Vector2 getSize()
    //    {
    //        return _size;
    //    }

    //    public void setSize(Vector2 size)
    //    {
    //        _size = size;
    //    }

    //    public override AABB calculateAABB(Vector2 position)
    //    {
    //        return new AABB(position, _size);
    //    }
    //}
}