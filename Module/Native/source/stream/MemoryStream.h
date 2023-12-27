#ifndef __MEMORY_STREAM_H
#define __MEMORY_STREAM_H

#include "Stream.h"

namespace lfnative {

class MemoryStream : public Stream
{
private:
    uint8_t* _buffer;
    size_t      _buffer_length;
    size_t      _position;

    void validateBuffer(size_t length);

public:
    MemoryStream();
    MemoryStream(uint8_t* buffer,size_t length);
    ~MemoryStream();

    inline uint8_t* getBuffer()
    {
        return _buffer;
    }

    virtual void setPosition(size_t position);
    virtual size_t getPosition();
    virtual size_t getLength();
    virtual size_t write(size_t offset, const void* buffer, size_t length);
    virtual size_t read(size_t offset, void* buffer, size_t length);

public:
    static MemoryStream* createFromFile(const char* path);

};

}

#endif