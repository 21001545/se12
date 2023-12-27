#ifndef PluginInterface_CSharp_Config_h
#define PluginInterface_CSharp_Config_h

#if defined(PLATFORM_WINDOWS_CSHARP)

    #define PLUGIN_API  __declspec(dllexport)

#else

    #define PLUGIN_API

#endif

#include <stdexcept>

typedef int64_t fix32;

#endif
