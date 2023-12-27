#include "Precompiled.h"
#include "PluginInterface_JNI.h"
#include "JNIHelper.h"
#include "JNIScopedContext.h"
#include "BSDiffProcessor.h"
#include "BSPatchProcessor.h"

#include <stdio.h>
#include <stdlib.h>
#include <fcntl.h>

JNIEXPORT jboolean JNICALL Java_com_lifefesta_drun_server_module_nativemodule_NativeBSDiff_bsdiff
(JNIEnv* env, jclass, jstring oldFile, jstring newFile, jstring patchFile)
{
    JNIScopedContext ctx(env);

    const char* old_file_path = ctx.allocUTFString(oldFile);
    const char* new_file_path = ctx.allocUTFString(newFile);
    const char* patch_file_path = ctx.allocUTFString(patchFile);

    BSDiffProcessor processor;
    return processor.run(env, old_file_path, new_file_path, patch_file_path) == 0;
}

JNIEXPORT jboolean JNICALL Java_com_lifefesta_drun_server_module_nativemodule_NativeBSDiff_bspatch
(JNIEnv* env, jclass, jstring oldFile, jstring patchFile, jstring newFile)
{
    JNIScopedContext ctx(env);

    const char* old_file_path = ctx.allocUTFString(oldFile);
    const char* new_file_path = ctx.allocUTFString(newFile);
    const char* patch_file_path = ctx.allocUTFString(patchFile);

    BSPatchProcessor processor;
    return processor.run(env, old_file_path, patch_file_path, new_file_path) == 0;
}


/*

class ScopedOutContext
{
public:
    ScopedOutContext(JNIEnv* env): 
        env(env),
        old_buffer(NULL),
        new_buffer(NULL),
        patch_buffer(NULL),
        old_file_path(NULL),
        new_file_path(NULL),
        patch_file_path(NULL),
        old_file_path_j(NULL),
        new_file_path_j(NULL),
        patch_file_path_j(NULL),
        zip(NULL)
    {

    }
    ~ScopedOutContext()
    {
        if (old_buffer != NULL)
        {
            free(old_buffer);
        }

        if (new_buffer != NULL)
        {
            free(new_buffer);
        }
        
        if (patch_buffer != NULL)
        {
            free(patch_buffer);
        }

        if (old_file_path != NULL)
        {
            env->ReleaseStringUTFChars(old_file_path_j, old_file_path);
        }
        if (new_file_path != NULL)
        {
            env->ReleaseStringUTFChars(new_file_path_j, new_file_path);
        }
        if (patch_file_path != NULL)
        {
            env->ReleaseStringUTFChars(patch_file_path_j, patch_file_path);
        }
        if (zip != NULL)
        {
            zip_close(zip);
        }
    }
    
protected:
    JNIEnv* env;

public:
    uint8_t* old_buffer;
    uint8_t* new_buffer;
    uint8_t* patch_buffer;

    const char* old_file_path;
    const char* new_file_path;
    const char* patch_file_path;

    jstring old_file_path_j;
    jstring new_file_path_j;
    jstring patch_file_path_j;

    zip_t* zip;
};

struct extracted_memory_stream
{
    JNIEnv* env;
    uint8_t* buffer;
    int length;
    int read_offset;
};


static int zip_write(struct bsdiff_stream* stream, const void* buffer, int size)
{
    zip_t* zip;

    zip = (zip_t*)stream->opaque;
    if (zip_entry_write(zip, buffer, size) < 0)
    {
        return -1;
    }

    return 0;
}

static int memory_read(const struct bspatch_stream* stream, void* buffer, int length)
{
    extracted_memory_stream* ms = (extracted_memory_stream*)stream->opaque;

    if (ms->read_offset + length > ms->length)
    {
        Logger::logError(ms->env,"memory_read fail: offset[%d] read_length[%d] >= total_length[%d]", ms->read_offset, length, ms->length);
        return -1;
    }

    memcpy(buffer, ms->buffer + ms->read_offset, length);
    ms->read_offset += length;
    return 0;
}

int readAllFileData(const char* file_path, uint8_t** buffer, off_t* buffer_size)
{
    FILE* fp;

    int result = fopen_s(&fp, file_path, "rb");
    if (result != 0)
    {
        return -1;
    }

    long begin;
    long end;

    begin = ftell(fp);
    fseek(fp, 0, SEEK_END);
    end = ftell(fp);
    fseek(fp, 0, SEEK_SET);

    *buffer_size = end - begin;
    if (*buffer_size <= 0)
    {
        fclose(fp);
        return -2;
    }

    *buffer = (uint8_t*)malloc(*buffer_size + 1);

//    Logger::logInfo("buffer_size:%d", *buffer_size);

    size_t read_bytes = fread(*buffer, 1, *buffer_size, fp);

//    Logger::logInfo("read_bytes:%d", read_bytes);
    if ( read_bytes != *buffer_size)
    {
        fclose(fp);
        return -3;
    }

    fclose(fp);
    return 0;
}

int writeAllFileData(const char* file_path, uint8_t* buffer, off_t buffer_size)
{
    FILE* fp;
    int result = fopen_s(&fp, file_path, "wb");
    if (result != 0)
    {
        return -1;
    }

    int written_bytes = (int)fwrite(buffer, 1, buffer_size, fp);
    if (written_bytes != buffer_size)
    {
        fclose(fp);
        return -3;
    }

    fclose(fp);
    return 0;
}

JNIEXPORT jboolean JNICALL Java_com_vagabond_module_nativecore_NativeBSDiff_bsdiff
  (JNIEnv *env, jclass, jstring oldFile, jstring newFile, jstring patchFile)
{
    ScopedOutContext ctx(env);

    const char* old_file_path = env->GetStringUTFChars( oldFile, NULL);
    const char* new_file_path = env->GetStringUTFChars( newFile, NULL);
    const char* patch_file_path = env->GetStringUTFChars(patchFile, NULL);

    ctx.old_file_path = old_file_path;
    ctx.old_file_path_j = oldFile;

    ctx.new_file_path = new_file_path;
    ctx.new_file_path_j = newFile;

    ctx.patch_file_path = patch_file_path;
    ctx.patch_file_path_j = patchFile;

    //-----------------------------------------------------------
    uint8_t* old_buffer;
    uint8_t* new_buffer;
    off_t old_size;
    off_t new_size;

    int err;

    //------------------------------------------------------------
    err = readAllFileData(old_file_path, &old_buffer, &old_size);
    if( err < 0)
    {
        Logger::logError(env,"read old file fail:%d", err);
        return false;
    }

    //------------------------------------------------------------
    ctx.old_buffer = old_buffer;

    //------------------------------------------------------------
    err = readAllFileData(new_file_path, &new_buffer, &new_size);
    if( err < 0)
    {
        Logger::logError(env, "read new file fail:%d", err);
        return false;
    }
    
    //------------------------------------------------------------
    ctx.new_buffer = new_buffer;

    //------------------------------------------------------------
    struct zip_t* zip = zip_open(patch_file_path, ZIP_DEFAULT_COMPRESSION_LEVEL, 'w');
    if (zip == NULL)
    {
        Logger::logError(env, "zip_open fail");
        return false;
    }
    
    ctx.zip = zip;

    //------------------------------------------------------------
    err = zip_entry_open(zip, "payload.patch");
    if ( err < 0)
    {
        Logger::logError(env, "zip_entry_open(payload.patch) fail:%d", err);
        return false;
    }

    struct bsdiff_stream stream;
    stream.malloc = malloc;
    stream.free = free;
    stream.write = zip_write;
    stream.opaque = zip;

    //------------------------------------------------------------
    err = bsdiff(old_buffer, old_size, new_buffer, new_size, &stream);
    if( err)
    {
        Logger::logError(env, "bsdiff fail:%d", err);
        return false;
    }

    zip_entry_close(zip);

    //------------------------------------------------------------
    err = zip_entry_open(zip, "info.txt");
    if (err < 0)
    {
        Logger::logError(env, "zip_entry_open(info.txt) fail:%d", err);
        return false;
    }

    uint8_t header_buffer[128];
    int header_length = sprintf_s((char*)header_buffer, 128, "%d", new_size);
    zip_entry_write(zip, header_buffer, header_length);

    zip_entry_close(zip);
    

    return true;
}

JNIEXPORT jboolean JNICALL Java_com_vagabond_module_nativecore_NativeBSDiff_bspatch
  (JNIEnv *env, jclass, jstring oldFile, jstring patchFile, jstring newFile)
{
    ScopedOutContext ctx(env);

    const char* old_file_path = env->GetStringUTFChars( oldFile, NULL);
    const char* new_file_path = env->GetStringUTFChars( newFile, NULL);
    const char* patch_file_path = env->GetStringUTFChars(patchFile, NULL);

    ctx.old_file_path = old_file_path;
    ctx.old_file_path_j = oldFile;

    ctx.new_file_path = new_file_path;
    ctx.new_file_path_j = newFile;

    ctx.patch_file_path = patch_file_path;
    ctx.patch_file_path_j = patchFile;

    //------------------------------------------------------------
    uint8_t* old_buffer;
    off_t old_size;
    int err;

    //------------------------------------------------------------
    err = readAllFileData(old_file_path, &old_buffer, &old_size);
    if (err < 0)
    {
        Logger::logError(env, "read old file fail:%d", err);
        return false;
    }

    ctx.old_buffer = old_buffer;

    //------------------------------------------------------------
    zip_t* zip = zip_open(patch_file_path, ZIP_DEFAULT_COMPRESSION_LEVEL, 'r');
    if (zip == null)
    {
        Logger::logError(env, "zip_open fail");
        return false;
    }

    ctx.zip = zip;

    //------------------------------------------------------------
    err = zip_entry_open(zip, "info.txt");
    if (err < 0)
    {
        Logger::logError(env, "zip_entry_open(info.txt) fail:%d",err);
        return false;
    }

    uint8_t* header_buffer;
    size_t header_size;
    err = (int)zip_entry_read(zip, (void**)&header_buffer, &header_size);
    if (err < 0)
    {
        Logger::logError(env, "zip_entry_read fail:%d",err);
        return false;
    }

    off_t new_size;
    sscanf_s((const char*)header_buffer, "%d", &new_size);

    free(header_buffer);

    zip_entry_close(zip);

    //------------------------------------------------------------
    uint8_t* new_buffer = (uint8_t*)malloc(new_size + 1);

    ctx.new_buffer = new_buffer;

    //------------------------------------------------------------
    err = zip_entry_open(zip, "payload.patch");
    if (err < 0)
    {
        Logger::logError(env, "zip_entry_open(payload.patch) fail:%d", err);
        return false;
    }


    //------------------------------------------------------------
    uint8_t* patch_buffer;
    off_t patch_size;
    err = (int)zip_entry_read(zip, (void**)&patch_buffer, (size_t*)&patch_size);
    if (err < 0)
    {
        Logger::logError(env, "zip_entry_read(payload.patch) fail:%d", err);
        return false;
    }

    ctx.patch_buffer = patch_buffer;

    //------------------------------------------------------------
    zip_entry_close(zip);
    zip_close(zip);

    ctx.zip = NULL;

    //------------------------------------------------------------
    struct extracted_memory_stream ms;
    ms.buffer = patch_buffer;
    ms.length = patch_size;
    ms.read_offset = 0;
    ms.env = env;

    struct bspatch_stream stream;
    stream.read = memory_read;
    stream.opaque = &ms;

    err = bspatch(old_buffer, old_size, new_buffer, new_size, &stream);
    if (err)
    {
        Logger::logError(env,"bapatch fail:%d", err);
        return false;
    }

    //
    err = writeAllFileData(new_file_path, new_buffer, new_size);
    if (err < 0)
    {
        Logger::logError(env,"writeAllFileData fail:%d", err);
        return false;
    }

    return true;
}

*/