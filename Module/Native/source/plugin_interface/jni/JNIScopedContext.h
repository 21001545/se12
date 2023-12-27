#ifndef __JNISCOPED_CONTEXT_H
#define __JNISCOPED_CONTEXT_H

class JNIScopedContext
{
private:
    struct StringContext
    {
        jstring         obj_string;
        const char* utf_string;
    };

    JNIEnv* _env;
    PrimitiveArray<StringContext>  _stringArray;

public:
    JNIScopedContext(JNIEnv* env);
    ~JNIScopedContext();

    inline const char* allocUTFString(jstring jstring)
    {
        const char* utf_string = _env->GetStringUTFChars(jstring, null);

        StringContext ctx;
        ctx.obj_string = jstring;
        ctx.utf_string = utf_string;

        _stringArray.add(ctx);

        return utf_string;
    }
};


#endif