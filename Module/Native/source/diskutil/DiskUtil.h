#ifndef __DISKUTIL_H
#define __DISKUTIL_H

class DiskUtil
{
public:
    static int readAllBytes(const char* path, void** buffer, size_t* buffer_size);
    static int writeAllBytes(const char* path, void* buffer, size_t buffer_size);
};

#endif