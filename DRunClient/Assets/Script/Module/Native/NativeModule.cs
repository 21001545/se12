using AOT;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Festa.Client.Module
{
	public static class NativeModule
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void fn_LogCallback(int type, string str);

		[MonoPInvokeCallback(typeof(fn_LogCallback))]
		static void logCallback(int type, string str)
		{
			if( type == 1)
			{
				Debug.LogWarning(str);
			}
			else if( type == 2)
			{
				// 너무 길면 화일로 저장한다
				if( str.Length > 4096)
				{
					MainThreadDispatcher.dispatch(() => {

						try
						{
							string path = Application.temporaryCachePath + "/native_error.log";
							System.IO.File.WriteAllText(path, str);

							Debug.LogError(str.Substring(0, 256));
						}
						catch (Exception e)
						{
							Debug.LogException(e);
						}

					});

				}
				else
				{
					Debug.LogError(str);
				}
			}
			else
			{
				Debug.Log(str);
			}
		}

#if UNITY_EDITOR
		public static IntPtr _libraryHandle;
		public delegate void fn_init(IntPtr callback);
		public delegate bool fn_BSDiff(string old_file_path, string new_file_path, string patch_file_path);
		public delegate bool fn_BSPatch(string old_file_path, string patch_file_path, string new_file_path);

		public delegate int fn_MB_createContext();
		public delegate void fn_MB_releaseContext(int context_id);
		public delegate void fn_MB_startMeshBuilder(int context_id, int extrudeHeight);
		public delegate int fn_MB_beginPath(int context_id,int slot);
		public delegate void fn_MB_addPathPoint(int context_id,int slot, int path_id, int x, int y);
		public delegate int fn_MB_build(int context_id);
		public delegate int fn_MB_getVertexCount(int context_id);
		public delegate int fn_MB_getIndexCount(int context_id);
		public delegate int fn_MB_getVertexX(int context_id, int index);
		public delegate int fn_MB_getVertexY(int context_id, int index);
		public delegate int fn_MB_getVertexZ(int context_id, int index);
		public delegate int fn_MB_getNormalX(int context_id, int index);
		public delegate int fn_MB_getNormalY(int context_id, int index);
		public delegate int fn_MB_getNormalZ(int context_id, int index);
		public delegate int fn_MB_getIndex(int context_id, int index);

		public delegate int fn_MB_LineBoundClip_beginPath(int context_id);
		public delegate void fn_MB_LineBoundClip_addPathPoint(int context_id, int path_id, int x, int y);
		public delegate void fn_MB_LineBoundClip_build(int context_id);
		public delegate int fn_MB_LineBoundClip_getResultPathCount(int context_id);
		public delegate int fn_MB_LineBoundClip_getResultPathPointCount(int context_id, int path_id);
		public delegate int fn_MB_LineBoundClip_getResultPathX(int context_id, int path_id, int index);
		public delegate int fn_MB_LineBoundClip_getResultPathY(int context_id, int path_id, int index);

		public delegate void fn_MB_Polygon_AddRing(int context_id, short[] pointArray, int pointCount);
		public delegate int fn_MB_Polygon_Build(int context_id,int extudeHeight);
		public delegate int fn_MB_Polygon_getVertexCount(int context_id);
		public delegate int fn_MB_Polygon_getIndexCount(int context_id);
		public delegate int fn_MB_Polygon_getVertexX(int context_id, int index);
		public delegate int fn_MB_Polygon_getVertexY(int context_id, int index);
		public delegate int fn_MB_Polygon_getVertexZ(int context_id, int index);
		public delegate int fn_MB_Polygon_getNormalX(int context_id, int index);
		public delegate int fn_MB_Polygon_getNormalY(int context_id, int index);
		public delegate int fn_MB_Polygon_getNormalZ(int context_id, int index);
		public delegate int fn_MB_Polygon_getIndex(int context_id, int index);

		public static fn_init init;
		public static fn_BSDiff BSDiff;
		public static fn_BSPatch BSPatch;
		public static fn_MB_createContext MB_createContext_internal;
		public static fn_MB_releaseContext MB_releaseContext_internal;
		public static fn_MB_startMeshBuilder MB_startMeshBuilder;
		public static fn_MB_beginPath MB_beginPath;
		public static fn_MB_addPathPoint MB_addPathPoint;
		public static fn_MB_build MB_build;
		public static fn_MB_getVertexCount MB_getVertexCount;
		public static fn_MB_getIndexCount MB_getIndexCount;
		public static fn_MB_getVertexX MB_getVertexX;
		public static fn_MB_getVertexY MB_getVertexY;
		public static fn_MB_getVertexZ MB_getVertexZ;
		public static fn_MB_getNormalX MB_getNormalX;
		public static fn_MB_getNormalY MB_getNormalY;
		public static fn_MB_getNormalZ MB_getNormalZ;
		public static fn_MB_getIndex MB_getIndex;
		public static fn_MB_LineBoundClip_beginPath MB_LineBoundClip_beginPath;
		public static fn_MB_LineBoundClip_addPathPoint MB_LineBoundClip_addPathPoint;
		public static fn_MB_LineBoundClip_build MB_LineBoundClip_build;
		public static fn_MB_LineBoundClip_getResultPathCount MB_LineBoundClip_getResultPathCount;
		public static fn_MB_LineBoundClip_getResultPathPointCount MB_LineBoundClip_getResultPathPointCount;
		public static fn_MB_LineBoundClip_getResultPathX MB_LineBoundClip_getResultPathX;
		public static fn_MB_LineBoundClip_getResultPathY MB_LineBoundClip_getResultPathY;
		public static fn_MB_Polygon_AddRing MB_Polygon_AddRing;
		public static fn_MB_Polygon_Build MB_Polygon_Build;
		public static fn_MB_Polygon_getVertexCount MB_Polygon_getVertexCount;
		public static fn_MB_Polygon_getIndexCount MB_Polygon_getIndexCount;
		public static fn_MB_Polygon_getVertexX MB_Polygon_getVertexX;
		public static fn_MB_Polygon_getVertexY MB_Polygon_getVertexY;
		public static fn_MB_Polygon_getVertexZ MB_Polygon_getVertexZ;
		public static fn_MB_Polygon_getNormalX MB_Polygon_getNormalX;
		public static fn_MB_Polygon_getNormalY MB_Polygon_getNormalY;
		public static fn_MB_Polygon_getNormalZ MB_Polygon_getNormalZ;
		public static fn_MB_Polygon_getIndex MB_Polygon_getIndex;


#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX

			[DllImport("__Internal")]
			private static extern IntPtr dlopen(string path,int flag);
				
			[DllImport("__Internal")]
			private static extern IntPtr dlsym(IntPtr handle,string symbolName);

			[DllImport("__Internal")]
			private static extern IntPtr dlclose(IntPtr handle);

			private static IntPtr openLibrary(string path)
			{
				IntPtr handle = dlopen( path, 0);
				if( handle == IntPtr.Zero)
				{
					throw new Exception("couldn't open native library: " + path);
				}
				Debug.Log( string.Format( "open native library : {0}", path));
				return handle;
			}

			private static void closeLibrary(IntPtr libraryHandle)
			{
				dlclose(libraryHandle);
			}

			private static T getDelegate<T>() where T : class
			{
				string functionName = typeof(T).Name.Replace( "fn_", "");
				IntPtr symbol = dlsym(_libraryHandle, functionName);
				if( symbol == IntPtr.Zero)
				{
					throw new Exception("couldn't get function: " + functionName);
				}

				return Marshal.GetDelegateForFunctionPointer( symbol, typeof(T)) as T;
			}

			private static string _library_path = "/Script/Module/Native/Plugins/macos/festa_native.bundle/Contents/MacOS/festa_native";

#elif UNITY_EDITOR_WIN

		[DllImport("kernel32")]
		public static extern IntPtr LoadLibrary(string path);

		[DllImport("kernel32")]
		public static extern IntPtr GetProcAddress(IntPtr libraryHandle, string symboleName);

		[DllImport("kernel32")]
		public static extern bool FreeLibrary(IntPtr libraryHandle);

		public static IntPtr openLibrary(string path)
		{
			IntPtr handle = LoadLibrary(path);
			if (handle == IntPtr.Zero)
			{
				throw new Exception("Couldn't open native library: " + path);
			}
			return handle;
		}

		public static void closeLibrary(IntPtr libraryHandle)
		{
			FreeLibrary(libraryHandle);
		}

		public static T getDelegate<T>() where T : class
		{
			string functionName = typeof(T).Name.Replace("fn_", "");
			IntPtr symbol = GetProcAddress(_libraryHandle, functionName);
			if (symbol == IntPtr.Zero)
			{
				throw new Exception("Couldn't get function: " + functionName);
			}

			//			Debug.Log(string.Format("getDelegate : {0}", functionName));

			return Marshal.GetDelegateForFunctionPointer(symbol, typeof(T)) as T;
		}

		private static string _library_path = "/Script/Module/Native/Plugins/win/festa_native.dll";

#endif
		public static void initialize()
		{
			_libraryHandle = openLibrary(Application.dataPath + _library_path);

			init = getDelegate<fn_init>();
			BSDiff = getDelegate<fn_BSDiff>();
			BSPatch = getDelegate<fn_BSPatch>();

			MB_createContext_internal = getDelegate<fn_MB_createContext>();
			MB_releaseContext_internal = getDelegate<fn_MB_releaseContext>();
			MB_startMeshBuilder = getDelegate<fn_MB_startMeshBuilder>();
			MB_beginPath = getDelegate<fn_MB_beginPath>();
			MB_addPathPoint = getDelegate<fn_MB_addPathPoint>();
			MB_build = getDelegate<fn_MB_build>();
			MB_getVertexCount = getDelegate<fn_MB_getVertexCount>();
			MB_getIndexCount = getDelegate<fn_MB_getIndexCount>();
			MB_getVertexX = getDelegate<fn_MB_getVertexX>();
			MB_getVertexY = getDelegate<fn_MB_getVertexY>();
			MB_getVertexZ = getDelegate<fn_MB_getVertexZ>();
			MB_getNormalX = getDelegate<fn_MB_getNormalX>();
			MB_getNormalY = getDelegate<fn_MB_getNormalY>();
			MB_getNormalZ = getDelegate<fn_MB_getNormalZ>();
			MB_getIndex = getDelegate<fn_MB_getIndex>();
			MB_LineBoundClip_beginPath = getDelegate<fn_MB_LineBoundClip_beginPath>();
			MB_LineBoundClip_addPathPoint  = getDelegate<fn_MB_LineBoundClip_addPathPoint>();
			MB_LineBoundClip_build = getDelegate<fn_MB_LineBoundClip_build>();
			MB_LineBoundClip_getResultPathCount = getDelegate<fn_MB_LineBoundClip_getResultPathCount>();
			MB_LineBoundClip_getResultPathPointCount = getDelegate<fn_MB_LineBoundClip_getResultPathPointCount>();
			MB_LineBoundClip_getResultPathX = getDelegate<fn_MB_LineBoundClip_getResultPathX>();
			MB_LineBoundClip_getResultPathY = getDelegate<fn_MB_LineBoundClip_getResultPathY>();
			MB_Polygon_AddRing = getDelegate<fn_MB_Polygon_AddRing>();
			MB_Polygon_Build = getDelegate<fn_MB_Polygon_Build>();
			MB_Polygon_getVertexCount = getDelegate<fn_MB_Polygon_getVertexCount>();
			MB_Polygon_getIndexCount = getDelegate<fn_MB_Polygon_getIndexCount>();
			MB_Polygon_getVertexX = getDelegate<fn_MB_Polygon_getVertexX>();
			MB_Polygon_getVertexY = getDelegate<fn_MB_Polygon_getVertexY>();
			MB_Polygon_getVertexZ = getDelegate<fn_MB_Polygon_getVertexZ>();
			MB_Polygon_getNormalX = getDelegate<fn_MB_Polygon_getNormalX>();
			MB_Polygon_getNormalY = getDelegate<fn_MB_Polygon_getNormalY>();
			MB_Polygon_getNormalZ = getDelegate<fn_MB_Polygon_getNormalZ>();
			MB_Polygon_getIndex = getDelegate<fn_MB_Polygon_getIndex>();


			IntPtr ptr_log_callback = Marshal.GetFunctionPointerForDelegate(new fn_LogCallback(logCallback));
			init(ptr_log_callback);
		}

		public static void release()
		{
			if (_libraryHandle != IntPtr.Zero)
			{
				Debug.Log("close native library");
				closeLibrary(_libraryHandle);
				_libraryHandle = IntPtr.Zero;
			}
		}

		private static object syncObject = new object();

		public static int MB_createContextSafe()
		{
			lock(syncObject)
			{
				return MB_createContext_internal();
			}
		}

		public static void MB_releaseContextSafe(int context_id)
		{
			lock(syncObject)
			{
				MB_releaseContext_internal(context_id);
			}
		}


#else
	
#if UNITY_IPHONE && !UNITY_EDITOR
        const string DynLib = "__Internal";
#else
        const string DynLib = "festa_native";
#endif

		[DllImport(DynLib)]
		public static extern void init(IntPtr callback);

		[DllImport(DynLib)]
		public static extern bool BSDiff(string old_file_path, string new_file_path, string patch_file_path);

		[DllImport(DynLib)]
		public static extern bool BSPatch(string old_file_path, string patch_file_path, string new_file_path);

		[DllImport(DynLib)]
		public static extern int MB_createContext();
		[DllImport(DynLib)]
		public static extern void MB_releaseContext(int context_id);
		[DllImport(DynLib)]
		public static extern void MB_startMeshBuilder(int context_id, int extrudeHeight);
		[DllImport(DynLib)]
		public static extern int MB_beginPath(int context_id,int slot);
		[DllImport(DynLib)]
		public static extern void MB_addPathPoint(int context_id,int slot, int path_id, int x, int y);
		[DllImport(DynLib)]
		public static extern int MB_build(int context_id);
		[DllImport(DynLib)]
		public static extern int MB_getVertexCount(int context_id);
		[DllImport(DynLib)]
		public static extern int MB_getIndexCount(int context_id);
		[DllImport(DynLib)]
		public static extern int MB_getVertexX(int context_id, int index);
		[DllImport(DynLib)]
		public static extern int MB_getVertexY(int context_id, int index);
		[DllImport(DynLib)]
		public static extern int MB_getVertexZ(int context_id, int index);
		[DllImport(DynLib)]
		public static extern int MB_getNormalX(int context_id, int index);
		[DllImport(DynLib)]
		public static extern int MB_getNormalY(int context_id, int index);
		[DllImport(DynLib)]
		public static extern int MB_getNormalZ(int context_id, int index);
		[DllImport(DynLib)]
		public static extern int MB_getIndex(int context_id, int index);

		[DllImport(DynLib)]
		public static extern int MB_LineBoundClip_beginPath(int context_id);
		[DllImport(DynLib)]
		public static extern void MB_LineBoundClip_addPathPoint(int context_id, int path_id, int x, int y);
		[DllImport(DynLib)]
		public static extern void MB_LineBoundClip_build(int context_id);
		[DllImport(DynLib)]
		public static extern int MB_LineBoundClip_getResultPathCount(int context_id);
		[DllImport(DynLib)]
		public static extern int MB_LineBoundClip_getResultPathPointCount(int context_id, int path_id);
		[DllImport(DynLib)]
		public static extern int MB_LineBoundClip_getResultPathX(int context_id, int path_id, int index);
		[DllImport(DynLib)]
		public static extern int MB_LineBoundClip_getResultPathY(int context_id, int path_id, int index);

		[DllImport(DynLib)]
		public static extern void MB_Polygon_AddRing(int context_id, short[] pointArray, int pointCount);
		[DllImport(DynLib)]
		public static extern int MB_Polygon_Build(int context_id,int extrudeHeight); 
		[DllImport(DynLib)]
		public static extern int MB_Polygon_getVertexCount(int context_id);
		[DllImport(DynLib)]
		public static extern int MB_Polygon_getIndexCount(int context_id);
		[DllImport(DynLib)]
		public static extern int MB_Polygon_getVertexX(int context_id, int index);
		[DllImport(DynLib)]
		public static extern int MB_Polygon_getVertexY(int context_id, int index);
		[DllImport(DynLib)]
		public static extern int MB_Polygon_getVertexZ(int context_id, int index);
		[DllImport(DynLib)]
		public static extern int MB_Polygon_getNormalX(int context_id, int index);
		[DllImport(DynLib)]
		public static extern int MB_Polygon_getNormalY(int context_id, int index);
		[DllImport(DynLib)]
		public static extern int MB_Polygon_getNormalZ(int context_id, int index);
		[DllImport(DynLib)]
		public static extern int MB_Polygon_getIndex(int context_id, int index);

		public static void initialize()
		{
			IntPtr ptr_log_callback = Marshal.GetFunctionPointerForDelegate(new fn_LogCallback(logCallback));
			init(ptr_log_callback);
		}

		public static void release()
		{
		}

		private static object syncObject = new object();

		public static int MB_createContextSafe()
		{
			lock(syncObject)
			{
				return MB_createContext();
			}
		}

		public static void MB_releaseContextSafe(int context_id)
		{
			lock(syncObject)
			{
				MB_releaseContext(context_id);
			}
		}

#endif
	}
}
