#ifndef __STRINGBUILDER_H
#define __STRINGBUILDER_H

#include "PrimitiveArray.h"

class StringBuilder
{
private:
    PrimitiveArray<char>    _buffer;

public:
    inline const char* toString()
    {
        _buffer.add(0);
        return _buffer.array();
    }

    void appendFormat(const char* format, ...);
};

#endif