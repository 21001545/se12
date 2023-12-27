#include "Precompiled.h"
#include "PluginInterface_CSharp.h"
#include "BSDiffProcessor.h"
#include "BSPatchProcessor.h"

bool BSDiff(const char* old_file_path,const char* new_file_path,const char* patch_file_path)
{
	BSDiffProcessor processor;
	return processor.run(old_file_path, new_file_path, patch_file_path) == 0;
}

bool BSPatch(const char* old_file_path,const char* patch_file_path,const char* new_file_path)
{
	BSPatchProcessor processor;
	return processor.run(old_file_path, patch_file_path, new_file_path) == 0;
}
