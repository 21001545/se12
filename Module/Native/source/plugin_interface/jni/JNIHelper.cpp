#include "Precompiled.h"
#include "JNIHelper.h"


#ifdef WIN32
#include <Windows.h>
#endif

char log_buffer[256];

void JniHelper::Throw(JNIEnv* env,const char* format,...)
{
	va_list args;
	va_start( args, format);
	vsnprintf( log_buffer, 256, format, args);
	va_end( args);
	
	// java로 callback하면서 log_buffer는 복사되서 넘어가니까 문제 없겠지?
	
	jclass exception_class = env->FindClass("java/lang/Exception");
	env->ThrowNew( exception_class, log_buffer);
}
