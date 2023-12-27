//
//  Logger.cpp
//  NavigationModule
//
//  Created by Romanus on 2018. 4. 3..
//  Copyright 짤 2018??Romanus. All rights reserved.
//

#include "Precompiled.h"

#ifdef WIN32
#include <Windows.h>
#endif

/*
	TODO: JNIEnv는 쓰레드마다 다른 객체이다
		API가 멀티쓰레드로 호출될 수 있기 때문에
		아래의 코드는 심각한 문제가 있을 수 있다, 나중에 꼭 고친다
*/

#if defined(JNI_LOGGER)

//com.lifefesta.festa.server.module.nativemodule.NativeMain;

void Logger::logLegacy(JNIEnv* env, int type, const char* msg)
{
	jstring message = env->NewStringUTF(msg);
	jclass clz = env->FindClass("com/lifefesta/drun/server/module/nativemodule/NativeMain");
	jmethodID method_id = env->GetStaticMethodID(clz, "logCallback", "(ILjava/lang/String;)V");

	env->CallStaticVoidMethod(clz, method_id, (jint)type, message);
}

#else
void Logger::logLegacy(int type, const char* msg)
{
	printf("[%d] %s\n", type, msg);
	fflush(stdout);
}

#endif

LogCallback Logger::_log_callback = Logger::logLegacy;

void Logger::setLogCallback(LogCallback callback)
{
	_log_callback = callback;
}

void Logger::logRAW(JNIENV_PARAM int type, const char* string)
{
	_log_callback(JNIENV_CALL_PARAM type, string);
}

void Logger::log(JNIENV_PARAM int type, const char* format, ...)
{
	char log_buffer[256];

	va_list args;
	va_start( args, format);
	vsnprintf( log_buffer, 256, format, args);
	_log_callback(JNIENV_CALL_PARAM type, log_buffer);
	va_end( args);
}

void Logger::logInfo(JNIENV_PARAM const char* format, ...)
{
	char log_buffer[256];

	va_list args;
	va_start(args, format);
	vsnprintf(log_buffer, 256, format, args);
	_log_callback(JNIENV_CALL_PARAM LOG_TYPE_INFO, log_buffer);
	va_end(args);
}

void Logger::logWarning(JNIENV_PARAM const char* format, ...)
{
	char log_buffer[256];

	va_list args;
	va_start(args, format);
	vsnprintf(log_buffer, 256, format, args);
	_log_callback(JNIENV_CALL_PARAM LOG_TYPE_WARNING, log_buffer);
	va_end(args);
}

void Logger::logError(JNIENV_PARAM const char* format, ...)
{
	char log_buffer[256];

	va_list args;
	va_start(args, format);
	vsnprintf(log_buffer, 256, format, args);
	_log_callback(JNIENV_CALL_PARAM LOG_TYPE_ERROR, log_buffer);
	va_end(args);
}

