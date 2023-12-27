#include "Precompiled.h"
#include "BSPatchProcessor.h"
#include "zip.h"
#include "bspatch.h"
#include "DiskUtil.h"

BSPatchProcessor::BSPatchProcessor()
	:_oldData(null),_newData(null),_patchData(null)
{

}

BSPatchProcessor::~BSPatchProcessor()
{
	if (_oldData != null)
	{
		delete _oldData;
	}

	if (_newData != null)
	{
		delete _newData;
	}

	if (_patchData != null)
	{
		delete _patchData;
	}
}

static int memory_read(const struct bspatch_stream* stream, void* buffer, int length)
{
	lfnative::MemoryStream* ms = (lfnative::MemoryStream*)stream->opaque;
	ms->read(0, buffer, length);
	return 0;
}

int BSPatchProcessor::run(JNIENV_PARAM const char* old_file_path, const char* patch_file_path, const char* new_file_path)
{
	_oldData = lfnative::MemoryStream::createFromFile(old_file_path);
	if (_oldData == null)
	{
		Logger::logError(JNIENV_CALL_PARAM "old file open fail", old_file_path);
		return -1;
	}

	struct zip_t* zip = zip_open(patch_file_path, ZIP_DEFAULT_COMPRESSION_LEVEL, 'r');
	if (zip == null)
	{
		Logger::logError(JNIENV_CALL_PARAM "patch file open fail:%s", patch_file_path);
		return -2;
	}

	zip_entry_open(zip, "payload.patch");
	
	uint8_t* patch_buffer;
	size_t patch_size;
	int err = (int)zip_entry_read(zip, (void**)&patch_buffer, &patch_size);
	if (err < 0)
	{
		zip_entry_close(zip);
		zip_close(zip);
		Logger::logError(JNIENV_CALL_PARAM "zip_entry_read fail:%d", err);
		return -3;
	}

	Logger::logInfo(JNIENV_CALL_PARAM "patch_size :%d", patch_size);

	_patchData = new lfnative::MemoryStream(patch_buffer, patch_size);

	zip_entry_close(zip);
	zip_close(zip);

	size_t new_size;
	_patchData->read(0, &new_size, sizeof(size_t));
	
	Logger::logInfo(JNIENV_CALL_PARAM "new_size :%d", new_size);

	_newData = new lfnative::MemoryStream( (uint8_t*)malloc(new_size+1), new_size);

	//
	struct bspatch_stream stream;
	stream.read = memory_read;
	stream.opaque = _patchData;

	err = bspatch(_oldData->getBuffer(), _oldData->getLength(), _newData->getBuffer(), _newData->getLength(), &stream);
	if (err)
	{
		Logger::logError(JNIENV_CALL_PARAM "bspatch fail:%d", err);
		return -4;
	}

	err = DiskUtil::writeAllBytes(new_file_path, _newData->getBuffer(), _newData->getLength());
	if (err < 0)
	{
		Logger::logError(JNIENV_CALL_PARAM "writeAllBytes fail:%s", new_file_path);
		return -5;
	}

	return 0;

}
