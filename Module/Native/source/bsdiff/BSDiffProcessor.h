#ifndef __BSDIFFPROCESSOR_H
#define __BSDIFFPROCESSOR_H

#include "MemoryStream.h"

class BSDiffProcessor
{
private:
	lfnative::MemoryStream* _oldData;
	lfnative::MemoryStream* _newData;
	lfnative::MemoryStream* _patchData;

public:
	BSDiffProcessor();
	~BSDiffProcessor();

	int run(JNIENV_PARAM const char* old_file_path, const char* new_file_path, const char* patch_file_path);
};

#endif