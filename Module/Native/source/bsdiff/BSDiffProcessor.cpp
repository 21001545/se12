#include "Precompiled.h"
#include "BSDiffProcessor.h"
#include "bsdiff.h"
#include "zip.h"

BSDiffProcessor::BSDiffProcessor()
	:_oldData(null),_newData(null), _patchData(null)
{

}

BSDiffProcessor::~BSDiffProcessor()
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

static int stream_write(struct bsdiff_stream* stream, const void* buffer, int size)
{
	lfnative::MemoryStream* ms = (lfnative::MemoryStream*)stream->opaque;
	ms->write(0, buffer, size);
	return 0;
}

int BSDiffProcessor::run(JNIENV_PARAM const char* old_file_path, const char* new_file_path, const char* patch_file_path)
{
	_oldData = lfnative::MemoryStream::createFromFile(old_file_path);
	if (_oldData == null)
	{
		Logger::logError(JNIENV_CALL_PARAM "old file open fail:%s", old_file_path);
		return -1;
	}
	
	_newData = lfnative::MemoryStream::createFromFile(new_file_path);
	if (_newData == null)
	{
		Logger::logError(JNIENV_CALL_PARAM "new file open fail:%s", new_file_path);
		return -2;
	}
	
	_patchData = new lfnative::MemoryStream();

	size_t new_size = _newData->getLength();
	_patchData->write(0, &new_size, sizeof(size_t));

	struct bsdiff_stream stream;
	stream.malloc = malloc;
	stream.free = free;
	stream.write = stream_write;
	stream.opaque = _patchData;
	int err = bsdiff(_oldData->getBuffer(), _oldData->getLength(), _newData->getBuffer(), _newData->getLength(), &stream);
	if (err)
	{
		Logger::logError(JNIENV_CALL_PARAM "bsdiff fail:%d", err);
		return -3;
	}

	struct zip_t* zip = zip_open(patch_file_path, ZIP_DEFAULT_COMPRESSION_LEVEL, 'w');
	if (zip == null)
	{
		Logger::logError(JNIENV_CALL_PARAM "zip open fail:%s", patch_file_path);
		return -4;
	}

	err = zip_entry_open(zip, "payload.patch");
	if (err < 0)
	{
		Logger::logError(JNIENV_CALL_PARAM "zip_entry_open fail:%d", err);
		return -5;
	}

	err = zip_entry_write(zip, _patchData->getBuffer(), _patchData->getPosition());
	if (err < 0)
	{
		Logger::logError(JNIENV_CALL_PARAM "zip_entry_write fail:%d", err);
		return -6;
	}

	zip_entry_close(zip);
	zip_close(zip);
	return 0;
}
