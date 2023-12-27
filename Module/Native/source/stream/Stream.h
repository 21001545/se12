#ifndef __STREAM_H
#define __STREAM_H

namespace lfnative {

class Stream
{
public:
    virtual void setPosition(size_t position) = 0;
    virtual size_t getPosition() = 0;
    virtual size_t getLength() = 0;
    virtual size_t write(size_t offset, const void* buffer, size_t length) = 0;
    virtual size_t read(size_t offset, void* buffer, size_t length) = 0;
};

}

#endif