using GluonGui.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using UnityEngine;

namespace Festa
{
	public class CommandLineReader
	{
		//Config
		private const string CUSTOM_ARGS_PREFIX = "-CustomArgs:";
		private const char CUSTOM_ARGS_SEPARATOR = ';';

		private Dictionary<string, string> _argMap;

		public static CommandLineReader create()
		{
			CommandLineReader reader = new CommandLineReader();
			reader.init();
			return reader;
		}

		private void init()
		{
			_argMap = readCustomArguments();

			// 
			foreach(KeyValuePair<string,string> item in _argMap)
			{
				Debug.Log($"Custom Argument: {item.Key} = {item.Value}");
			}
		}

		public string get(string key)
		{
			string value;
			if( _argMap.TryGetValue(key, out value))
			{
				return value;
			}

			return null;
		}

		public string getString(string key,string defaultValue)
		{
			string value = get(key);
			if( value == null)
			{
				return defaultValue;
			}

			return value;
		}

		public bool getBoolean(string key,bool defaultValue)
		{
			string value = get(key);
			if( value == null)
			{
				return defaultValue;
			}

			return value == "1" || value == "true";
		}

		public int getInteger(string key,int defaultValue)
		{
			string value = get(key);
			if( value == null)
			{
				return defaultValue;
			}

			int result;
			if( int.TryParse(value,out result) == true)
			{
				return result;
			}
			else
			{
				return defaultValue;
			}
		}

		private static Dictionary<string, string> readCustomArguments()
		{
			Dictionary<string, string> customArgsDict = new Dictionary<string, string>();
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			string[] customArgs;
			string[] customArgBuffer;
			string customArgsStr = "";

			try
			{
				customArgsStr = commandLineArgs.Where(row => row.Contains(CUSTOM_ARGS_PREFIX)).Single();
			}
			catch (Exception e)
			{
				Debug.LogError("CommandLineReader.cs - GetCustomArguments() - Can't retrieve any custom arguments in the command line [" + commandLineArgs + "]. Exception: " + e);
				return customArgsDict;
			}

			customArgsStr = customArgsStr.Replace(CUSTOM_ARGS_PREFIX, "");
			customArgs = customArgsStr.Split(CUSTOM_ARGS_SEPARATOR);

			foreach (string customArg in customArgs)
			{
				customArgBuffer = customArg.Split('=');
				if (customArgBuffer.Length == 2)
				{
					customArgsDict.Add(customArgBuffer[0], customArgBuffer[1]);
				}
				else
				{
					Debug.LogWarning("CommandLineReader.cs - GetCustomArguments() - The custom argument [" + customArg + "] seem to be malformed.");
				}
			}

			return customArgsDict;
		}

	}
}
