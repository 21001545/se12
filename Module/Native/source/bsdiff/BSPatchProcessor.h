#ifndef __BSPATCHPROCESSOR_H
#define __BSPATCHPROCESSOR_H

#include "MemoryStream.h"

class BSPatchProcessor
{
private:
	lfnative::MemoryStream* _oldData;
	lfnative::MemoryStream* _newData;
	lfnative::MemoryStream* _patchData;

public:
	BSPatchProcessor();
	~BSPatchProcessor();

	int run(JNIENV_PARAM const char* old_file_path, const char* patch_file_path, const char* new_file_path);
};

#endif