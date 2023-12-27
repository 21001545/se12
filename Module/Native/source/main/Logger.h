//
//  Logger.h
//  NavigationModule
//
//  Created by Romanus on 2018. 4. 3..
//  Copyright 짤 2018??Romanus. All rights reserved.
//

#ifndef Logger_h
#define Logger_h

typedef void (*LogCallback)(JNIENV_PARAM int type, const char*);


#define LOG_TYPE_INFO		0
#define LOG_TYPE_WARNING	1
#define LOG_TYPE_ERROR		2

class Logger
{
private:
	static LogCallback _log_callback;
	

public:
	static void setLogCallback(LogCallback callback);

	static void logLegacy(JNIENV_PARAM int type, const char* msg);
	
	static void log(JNIENV_PARAM int type, const char* format, ...);
	static void logInfo(JNIENV_PARAM const char* format, ...);
	static void logWarning(JNIENV_PARAM const char* format, ...);
	static void logError(JNIENV_PARAM const char* format, ...);
	static void logRAW(JNIENV_PARAM int type,const char* string);
};

#endif /* Logger_h */
