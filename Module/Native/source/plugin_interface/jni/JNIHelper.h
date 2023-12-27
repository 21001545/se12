#ifndef JNIHELPER_H
#define JNIHELPER_H

#include <jni.h>

class JniHelper
{
public:
	static void Throw(JNIEnv* env,const char* format,...);
};

#endif /* JNIHELPER_H */