#include "Precompiled.h"
#include "StringBuilder.h"

#ifdef WIN32
#include <Windows.h>
#endif

void StringBuilder::appendFormat(const char* format, ...)
{
	char log_buffer[256];

	va_list args;
	va_start(args, format);
	size_t len = vsnprintf(log_buffer, 256, format, args);

	_buffer.addArray(log_buffer, (int)len);

	va_end(args);

}