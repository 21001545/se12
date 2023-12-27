//
//  Config.h
//  NavigationModule
//
//  Created by Romanus on 2018. 4. 9..
//  Copyright 짤 2018??Romanus. All rights reserved.
//

#ifndef Config_h
#define Config_h

#if defined(PLATFORM_XCODE_BUNDLE)

#elif defined(PLATFORM_XCODE_IOS)

#elif defined(PLATFORM_ANDROID)

#elif defined(PLATFORM_WINDOWS_CSHARP)

#elif defined(PLATFORM_WINDOWS_JNI)

#elif defined(PLATFORM_LINUX_JNI)
#endif


//--------------------------------------------------------------

#if defined(PLATFORM_WINDOWS_JNI) || defined(PLATFORM_LINUX_JNI)
#include <jni.h>

#define JNI_LOGGER
#define JNIENV_PARAM	JNIEnv* env,
#define JNIENV_PARAM_SINGLE	JNIEnv* env
#define JNIENV_CALL_PARAM	env,
#define JNIENV_CALL_PARAM_SINGLE env

#else

#define JNIENV_PARAM	
#define JNIENV_PARAM_SINGLE
#define JNIENV_CALL_PARAM
#define JNIENV_CALL_PARAM_SINGLE

#endif


#if defined(PLATFORM_WINDOWS_CSHARP) || defined(PLATFORM_WINDOWS_JNI)
//#include <Windows.h>
#include <assert.h>
#include <stdio.h>

#elif defined(__linux__)
#include <assert.h>
#include <stdio.h>
#include <stdarg.h>
#include <stdint.h>
#endif

#undef min
#undef max

#include <cstring> // strlen, memcpy, etc.
#include <cstdlib> // exit
//#include <cfloat>  // FLT_MAX
#include <vector>
#include <cmath>

#ifndef NULL
#define NULL 0
#endif

#ifndef null
#define null 0
#endif


#endif /* Config_h */
