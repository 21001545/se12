#ifndef _MAPBOXEDGE_H
#define _MAPBOXEDGE_H

#include "ReusableObject.h"
#include "MapBoxVertex.h"

class MapBoxEdge : public ResuableObject<MapBoxEdge>
{
public:
    MapBoxVertex*   p0;
    MapBoxVertex*   p1;
    int             shared_count;

    int nx;
    int ny;
    int nz;

/*
    v1[x] = v[0][x] - v[1][x];                  // Vector 1.x=Vertex[0].x-Vertex[1].x
    v1[y] = v[0][y] - v[1][y];                  // Vector 1.y=Vertex[0].y-Vertex[1].y
    v1[z] = v[0][z] - v[1][z];                  // Vector 1.z=Vertex[0].y-Vertex[1].z
    // Calculate The Vector From Point 2 To Point 1
    v2[x] = v[1][x] - v[2][x];                  // Vector 2.x=Vertex[0].x-Vertex[1].x
    v2[y] = v[1][y] - v[2][y];                  // Vector 2.y=Vertex[0].y-Vertex[1].y
    v2[z] = v[1][z] - v[2][z];                  // Vector 2.z=Vertex[0].z-Vertex[1].z
    // Compute The Cross Product To Give Us A Surface Normal
    out[x] = v1[y] * v2[z] - v1[z] * v2[y];             // Cross Product For Y - Z
    out[y] = v1[z] * v2[x] - v1[x] * v2[z];             // Cross Product For X - Z
    out[z] = v1[x] * v2[y] - v1[y] * v2[x];             // Cross Product For X - Y
*/

    inline void makeExtrudeNormal()
    {
        double dx = (double)p1->x - (double)p0->x;
        double dy = (double)p1->y - (double)p0->y;

        double length = std::sqrt(dx * dx + dy * dy);

        dx /= length;
        dy /= length;

        nx = (int)(dy * 1000);
        ny = (int)(-dx * 1000);
        nz = 0;
    }
};

#endif