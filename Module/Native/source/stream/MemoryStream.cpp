#include "Precompiled.h"
#include "MemoryStream.h"
#include "DiskUtil.h"

namespace lfnative {

#define INFLATE_BUFFER_MARGIN 512

MemoryStream::MemoryStream()
: _buffer(null), _buffer_length(0), _position(0)
{

}

MemoryStream::MemoryStream(uint8_t* buffer,size_t length)
: _buffer(buffer), _buffer_length(length), _position(0)
{

}

MemoryStream::~MemoryStream()
{
    if( _buffer != null)
    {
        free(_buffer);
    }
}

void MemoryStream::validateBuffer(size_t length)
{
    if( length <= _buffer_length)
    {
        return;
    }

    size_t new_buffer_size = _buffer_length + INFLATE_BUFFER_MARGIN;
    while(length > new_buffer_size)
    {
        new_buffer_size += INFLATE_BUFFER_MARGIN;
    }

    uint8_t* new_buffer = (uint8_t*)malloc( new_buffer_size);
    
    if( _buffer != null)
    {
        memcpy( new_buffer, _buffer, _buffer_length);
    }

    _buffer = new_buffer;
    _buffer_length = new_buffer_size;
}

void MemoryStream::setPosition(size_t position)
{
    _position = position;
}

size_t MemoryStream::getPosition()
{
    return _position;
}

size_t MemoryStream::getLength()
{
    return _buffer_length;
}

size_t MemoryStream::write(size_t offset,const void* buffer,size_t length)
{
    validateBuffer( _position + length);

    memcpy( _buffer + _position, (uint8_t*)buffer + offset, length);
    _position += length;

    return length;
}

size_t MemoryStream::read(size_t offset,void* buffer,size_t length)
{
    if( _position + length > _buffer_length)
    {
        length = _buffer_length - _position;
    }

    if( length <= 0)
    {
        return 0;
    }

    memcpy((uint8_t*)buffer + offset, _buffer + _position, length);
    _position += length;

    return length;
}

MemoryStream* MemoryStream::createFromFile(const char* path)
{
    uint8_t* buffer;
    size_t buffer_size;
    
    if (DiskUtil::readAllBytes(path, (void**)&buffer, &buffer_size) != 0)
    {
        return null;
    }

    return new MemoryStream(buffer, buffer_size);
}

    
}
