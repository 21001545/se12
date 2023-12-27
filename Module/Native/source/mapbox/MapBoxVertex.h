#ifndef _MAPBOXVERTEX_H
#define _MAPBOXVERTEX_H

#include "ReusableObject.h"

class MapBoxVertex : public ResuableObject<MapBoxVertex> 
{
public:
    int x;
    int y;
    int z;
    int nx;
    int ny;
    int nz;
    int index;

    inline void set(int x, int y, int z, int nx, int ny, int nz, int index)
    {
        this->x = x;
        this->y = y;
        this->z = z;
        this->nx = nx;
        this->ny = ny;
        this->nz = nz;
        this->index = index;
    }

    inline void copyFrom(MapBoxVertex* v)
    {
        x = v->x;
        y = v->y;
        z = v->z;
        nx = v->nx;
        nx = v->ny;
        nx = v->nz;
    }

    inline bool isEqual(int x, int y, int z, int nx, int ny, int nz)
    {
        return x == this->x &&
            y == this->y &&
            z == this->z &&
            nx == this->nx &&
            ny == this->nz &&
            nz == this->nz;
    }

    inline bool isEqual(MapBoxVertex* v)
    {
        return x == v->x &&
            y == v->y &&
            z == v->z &&
            nx == v->nx &&
            ny == v->ny &&
            nz == v->nz;
    }
};

#endif