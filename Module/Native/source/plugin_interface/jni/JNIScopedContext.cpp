#include "Precompiled.h"
#include "JNIScopedContext.h"

JNIScopedContext::JNIScopedContext(JNIEnv* env)
: _env(env)
{

}

JNIScopedContext::~JNIScopedContext()
{
    for(int i = 0; i < _stringArray.count(); ++i)
    {
        jstring obj = _stringArray.array()[ i].obj_string;
        const char* utf = _stringArray.array()[ i].utf_string;

        _env->ReleaseStringUTFChars( obj, utf);
    }
}
