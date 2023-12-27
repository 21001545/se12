project(NativeModule)

cmake_minimum_required(VERSION 3.4.1)

message("PROJECT_TARGET=" ${PROJECT_TARGET})

if( ${PROJECT_TARGET} STREQUAL "xcode_bundle")
	set(BUILD_MAC_BUNDLE true)
	set(PLUGIN_CSHARP true)
	add_compile_definitions(PLATFORM_XCODE_BUNDLE)
elseif( ${PROJECT_TARGET} STREQUAL "xcode_ios")
	set(BUILD_IOS true)
	set(PLUGIN_CSHARP true)

	set(CMAKE_OSX_DEPLOYMENT_TARGET 11.0)
	set(CMAKE_INSTALL_PREFIX `pwd`/_install)
	set(CMAKE_OSX_ARCHITECTURES arm64)
	set(CMAKE_XCODE_ATTRIBUTE_ONLY_ACTIVE_ARCH NO)
	set(CMAKE_IOS_INSTALL_COMBINED YES)

	add_compile_definitions(PLATFORM_XCODE_IOS)
elseif( ${PROJECT_TARGET} STREQUAL "android")
	set(BUILD_ANDROID true)
	set(PLUGIN_CSHARP true)

	add_compile_definitions(PLATFORM_ANDROID)
elseif( ${PROJECT_TARGET} STREQUAL "windows_jni")
	set(BUILD_WIN true)
	set(PLUGIN_CSHARP false)
	add_compile_definitions(PLATFORM_WINDOWS_JNI)
elseif( ${PROJECT_TARGET} STREQUAL "windows_csharp")
	set(BUILD_WIN true)
	set(PLUGIN_CSHARP true)
	add_compile_definitions(PLATFORM_WINDOWS_CSHARP)
elseif( ${PROJECT_TARGET} STREQUAL "linux_jni")
	set(PLUGIN_CSHARP false)
	set(BUILD_LINUX true)
	add_compile_definitions(PLATFORM_LINUX_JNI)
else()
	message(FATAL_ERROR "unknwon project target!! => " ${PROJECT_TARGET})
endif()

if(BUILD_WIN)
	set(CMAKE_CXX_FLAGS_RELEASE "/MT /EHa")
	set(CMAKE_CXX_FLAGS_DEBUG "/MTd /EHa")
	set(CMAKE_SHARED_LINKER_FLAGS_RELEASE "/DEBUG")
elseif(BUILD_IOS)
	set(CMAKE_XCODE_ATTRIBUTE_CLANG_CXX_LANGUAGE_STANDARD "c++11")
	set(CMAKE_XCODE_ATTRIBUTE_CLANG_CXX_LIBRARY "libc++")
	set(CMAKE_C_FLAGS "-fembed-bitcode")
	set(CMAKE_CXX_FLAGS "-fembed-bitcode -std=c++11 -stdlib=libc++")
elseif(BUILD_MAC_BUNDLE)
	set(CMAKE_XCODE_ATTRIBUTE_CLANG_CXX_LANGUAGE_STANDARD "c++11")
	set(CMAKE_XCODE_ATTRIBUTE_CLANG_CXX_LIBRARY "libc++")
	set(CMAKE_CXX_FLAGS "${CMAKE_C_FLAGS} -std=c++11 -stdlib=libc++")
endif()
	

include(main/CMakeLists.txt)
include(bsdiff/CMakeLists.txt)
include(zlib/CMakeLists.txt)
include(stream/CMakeLists.txt)
include(diskutil/CMakeLists.txt)
include(clipper/CMakeLists.txt)
include(poly2tri/CMakeLists.txt)
include(mapbox/CMakeLists.txt)
include(landbuilder/CMakeLists.txt)
include(data_structure/CMakeLists.txt)
include(collision/CMakeLists.txt)
include(plugin_interface/csharp/CMakeLists.txt)
include(plugin_interface/jni/CMakeLists.txt)

set(NATIVE_MODULE_HEADER
	${MAIN_HEADER}
	${BSDIFF_HEADER}
	${ZLIB_HEADER}
	${STREAM_HEADER}
	${DISKUTIL_HEADER}
	${CLIPPER_HEADER}
	${POLY2TRI_HEADER}
	${MAPBOX_HEADER}
	${LANDBUILDER_HEADER}
	${DATA_STRUCTURE_HEADER}
	${COLLISION_HEADER}
		)

set(NATIVE_MODULE_SOURCE
	${MAIN_SOURCE}
	${BSDIFF_SOURCE}
	${ZLIB_SOURCE}
	${STREAM_SOURCE}
	${DISKUTIL_SOURCE}
	${CLIPPER_SOURCE}
	${POLY2TRI_SOURCE}
	${MAPBOX_SOURCE}
	${LANDBUILDER_SOURCE}
	${COLLISION_SOURCE}
		)

set(NATIVE_MODULE_INCLUDE_DIRS main data_structure math bsdiff zlib stream diskutil clipper poly2tri mapbox collision landbuilder)

if(PLUGIN_CSHARP)
	list(APPEND NATIVE_MODULE_HEADER ${PLUGIN_INTERFACE_CSHARP_HEADER})
	list(APPEND NATIVE_MODULE_SOURCE ${PLUGIN_INTERFACE_CSHARP_SOURCE})
else()
	list(APPEND NATIVE_MODULE_HEADER ${PLUGIN_INTERFACE_JNI_HEADER})
	list(APPEND NATIVE_MODULE_SOURCE ${PLUGIN_INTERFACE_JNI_SOURCE})
	
	if(BUILD_WIN)
		set(JAVA_HOME "C:/Java/OpenJDK/jdk-15")
		find_package(JNI 15)

		if(JNI_FOUND)
			list(APPEND NATIVE_MODULE_INCLUDE_DIRS ${JNI_INCLUDE_DIRS})
		else()
			message(FATAL_ERROR "jni not found!!!")
		endif()
	elseif(BUILD_LINUX)
		set(JAVA_HOME "/usr/lib/jvm/java-17-openjdk-amd64")
		set(JAVA_AWT_LIBRARY NotNeeded)
		set(JAVA_AWT_INCLUDE_PATH NotNeeded)
		find_package(JNI 17)

		if(JNI_FOUND)
			list(APPEND NATIVE_MODULE_INCLUDE_DIRS ${JNI_INCLUDE_DIRS})
		else()
			message(FATAL_ERROR "jni not found!!!")
		endif()
	endif()


endif()

list(APPEND NATIVE_MODULE_SOURCE ${NATIVE_MODULE_HEADER})

include_directories(${NATIVE_MODULE_INCLUDE_DIRS})

if(BUILD_MAC_BUNDLE)
	add_library(festa_native MODULE
				${NATIVE_MODULE_SOURCE}
				main.cpp)
	set_target_properties(festa_native PROPERTIES BUNDLE TRUE)
elseif(BUILD_IOS)

	add_library(festa_native STATIC
				${NATIVE_MODULE_SOURCE}
				main.cpp)
else()

	add_library(festa_native SHARED
				${NATIVE_MODULE_SOURCE}
				main.cpp)

endif()

if(BUILD_ANDROID)
    find_library( # Sets the name of the path variable.
            log-lib

            # Specifies the name of the NDK library that
            # you want CMake to locate.
            log)
    target_link_libraries( # Specifies the target library.
			festa_native

            # Links the target library to the log library
            # included in the NDK.
            ${log-lib})
elseif(BUILD_LINUX)
	target_link_options(festa_native PRIVATE -static-libgcc -static-libstdc++)
endif()
