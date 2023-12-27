#include "Precompiled.h"
#include "DiskUtil.h"

int DiskUtil::readAllBytes(const char* path,void** buffer,size_t* buffer_size)
{
    FILE* fp;
    int err;

#if defined(_MSC_VER)
    err = fopen_s(&fp, path, "rb");
    if (err != 0)
    {
        return -1;
    }
#else
    fp = fopen(path,"rb");
    if( fp == null)
    {
        return -1;
    }
#endif

    long begin;
    long end;

    begin = ftell(fp);
    fseek(fp, 0, SEEK_END);
    end = ftell(fp);
    fseek(fp, 0, SEEK_SET);

    size_t size = end - begin;
    if (size <= 0)
    {
        fclose(fp);
        return -2;
    }
    
    *buffer_size = size;
    *buffer = malloc(size + 1);

    size_t read_bytes = fread(*buffer, 1, size, fp);
    if (read_bytes != size)
    {
        fclose(fp);
        return -3;
    }

    fclose(fp);
    return 0;
}

int DiskUtil::writeAllBytes(const char* path,void* buffer,size_t buffer_size)
{
    FILE* fp;

#if defined(_MSC_VER)
    int err = fopen_s(&fp, path, "wb");
    if (err != 0)
    {
        return -1;
    }
#else
    fp = fopen(path, "wb");
    if( fp == null)
    {
        return -1;
    }
#endif    

    int written_bytes = (int)fwrite(buffer, 1, buffer_size, fp);
    if (written_bytes != buffer_size)
    {
        fclose(fp);
        return -2;
    }

    fclose(fp);
    return 0;
}
