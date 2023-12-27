using Festa.Client.Module;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using AOS_APP_INFO = UnityEngine.Localization.Platform.Android.AppInfo;
using IOS_APP_INFO = UnityEngine.Localization.Platform.iOS.AppInfo;

namespace Festa
{
    public class BuildApp
    {
        public static void saveBuildSettings(CommandLineReader parameters)
        {
            JsonObject json = new JsonObject();
            json.put("branch", parameters.getString("branch", "n/a"));
            json.put("default_server", parameters.getString("server", "dev-inhouse"));

            File.WriteAllText(Application.dataPath + "/Resources/build_settings.json", json.encode());
            AssetDatabase.Refresh();
        }

        public static void BuildPlayer_IOS_Jenkins()
		{
            //-------------------------------------------
            CommandLineReader parameters = CommandLineReader.create();

            bool rebuildProject = parameters.getBoolean("rebuild", true);
            int buildNumber = parameters.getInteger("buildNumber", 1);
			string packageName = parameters.getString("packageName", "com.lifefesta.drun");

			//-------------------------------------------

			string target_path;
            BuildTarget target = BuildTarget.iOS;
            BuildOptions options = BuildOptions.None;

            target_path = Application.dataPath.Replace("/Assets", "") + "/ios_build";
            options = BuildOptions.SymlinkSources;

            if( rebuildProject)
            {
                options |= BuildOptions.CleanBuildCache;
            }
            else
            {
				if(BuildPipeline.BuildCanBeAppended(target, target_path) == CanAppendBuild.Yes)
                {
					options |= BuildOptions.AcceptExternalModificationsToPlayer;
				}
			}

            string[] levels = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);

			JsonObject build_version = readBuildVersion();
			PlayerSettings.bundleVersion = build_version.getJsonObject("ios").getString("marketing_version");
			PlayerSettings.iOS.buildNumber = buildNumber.ToString();
			PlayerSettings.iOS.appleEnableAutomaticSigning = false;
            PlayerSettings.iOS.appleDeveloperTeamID = "";
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, packageName);

			saveBuildSettings(parameters);
            updateLunarConsoleConfig(parameters);
			prepareBuildConfig(packageName);
			setLocalizationSetting(packageName);

			BuildReport report = BuildPipeline.BuildPlayer(levels, target_path, target, options);
			BuildSummary summary = report.summary;

			if (summary.result != BuildResult.Succeeded && Application.isBatchMode)
			{
				EditorApplication.Exit(-1);
			}
		}

		private static JsonObject readBuildVersion()
		{
            string path = Application.dataPath + "/build_version.json";
            string txt_data = File.ReadAllText(path);

            return new JsonObject(txt_data);
        }

		[MenuItem("Tools/Build/Build Android")]
        public static void BuildPlayer_AOS_Editor()
		{
            string target_path = Application.dataPath + "/../test.apk";
            if(File.Exists(target_path))
			{
                File.Delete(target_path);
			}

			BuildPlayer_AOS(target_path, false, false, "dev.keystore", "dev", "lifefesta", false);
		}

		[MenuItem("Tools/Build/Build Android (rebuild)")]
		public static void BuildPlayer_AOS_Editor_Rebuild()
		{
			string target_path = Application.dataPath + "/../test.apk";
			if (File.Exists(target_path))
			{
				File.Delete(target_path);
			}

			BuildPlayer_AOS(target_path, false, true, "dev.keystore", "dev", "lifefesta", false);
		}

		[MenuItem("Tools/Build/Build Android (development)")]
        public static void BuildPlayer_AOS_Editor_Development()
        {
			string target_path = Application.dataPath + "/../test.apk";
			if (File.Exists(target_path))
			{
				File.Delete(target_path);
			}

			BuildPlayer_AOS(target_path, false, false, "dev.keystore", "dev", "lifefesta", true);
		}

		public static void BuildPlayer_AOS_Jenkins()
		{
			CommandLineReader parameters = CommandLineReader.create();
            int buildNumber = parameters.getInteger("buildNumber", 1);
            bool il2cpp = parameters.getBoolean("il2cpp", false);
			bool rebuildProject = parameters.getBoolean("rebuild", true);
            bool buildAppBundle = parameters.getBoolean("appbundle", false);
            string packageName = parameters.getString("packageName", "com.lifefesta.drun");
            string keystore_name = parameters.getString("keyStoreName", "dev.keystore");
            string keystore_pass = parameters.getString("keyStorePass", "dev");
            string keystore_alias = parameters.getString("keyStoreAlias", "lifefesta");
            bool appendApkPostfix = parameters.getBoolean("appendApkPostfix", false);

			JsonObject build_version = readBuildVersion();
            PlayerSettings.bundleVersion = build_version.getJsonObject("android").getString("version");
            PlayerSettings.Android.bundleVersionCode = buildNumber;
			PlayerSettings.SplashScreen.show = false;
			PlayerSettings.SplashScreen.showUnityLogo = false;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, packageName);

			saveBuildSettings(parameters);
            updateLunarConsoleConfig(parameters);
			prepareBuildConfig(packageName);
			setLocalizationSetting(packageName);


			string apkPostFix = "";
			if (appendApkPostfix)
			{
                DateTime now = DateTime.Now;
                apkPostFix = $"_{PlayerSettings.bundleVersion}({buildNumber})_{now.Year}_{now.Month}_{now.Day}";
            }

			string target_path;

			if (buildAppBundle)
            {
                EditorUserBuildSettings.buildAppBundle = true;
                target_path = $"{Application.dataPath}/../aos_build/drun{apkPostFix}.aab";
			}
			else
            {
				EditorUserBuildSettings.buildAppBundle = false;
				target_path = $"{Application.dataPath}/../aos_build/drun{apkPostFix}.apk";
			}

            if( File.Exists(target_path))
			{
                File.Delete(target_path);
			}

            BuildPlayer_AOS(target_path, il2cpp, rebuildProject, keystore_name, keystore_pass, keystore_alias, false);
        }

        public static void BuildPlayer_AOS(string target_path,bool il2cpp,bool rebuild,string keystore_filename, string keystore_password,string alias,bool isDevelop)
        {
			BuildTarget target = BuildTarget.Android;
            BuildOptions options = BuildOptions.None;

			if (rebuild)
			{
				options |= BuildOptions.CleanBuildCache;
			}

            if( isDevelop)
            {
                options |= BuildOptions.Development | BuildOptions.AllowDebugging;
            }

			string[] levels = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);

            PlayerSettings.Android.keyaliasPass = keystore_password;
            PlayerSettings.Android.keystorePass = keystore_password;
            PlayerSettings.Android.keystoreName = Application.dataPath + "/../" + keystore_filename;
			PlayerSettings.Android.keyaliasName = alias;

            if( il2cpp)
            {
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;// | AndroidArchitecture.X86 | AndroidArchitecture.X86_64;
                EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Public;
            }
            else
            {
				PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
			}

			BuildReport report = BuildPipeline.BuildPlayer(levels, target_path, target, options);
			BuildSummary summary = report.summary;

			if (summary.result != BuildResult.Succeeded && Application.isBatchMode)
			{
				EditorApplication.Exit(-1);
			}
		}

        private static void prepareBuildConfig(string packageName)
        {
            List<string> source_list = new List<string>();
            List<string> target_list = new List<string>();

            source_list.Add($"BuildConfig/{packageName}/app_manifest.xml");
			source_list.Add($"BuildConfig/{packageName}/firebase-ios.plist");
			source_list.Add($"BuildConfig/{packageName}/firebase-aos.json");
            source_list.Add($"BuildConfig/{packageName}/firebase-config.json");

            target_list.Add("Plugins/Android/AndroidManifest.xml");
            target_list.Add("GoogleService-Info.plist");
            target_list.Add("google-services.json");
            target_list.Add("StreamingAssets/google-services-desktop.json");

            string base_path = Application.dataPath;

            for(int i = 0; i < source_list.Count; ++i)
            {
                Debug.Log($"copy: {source_list[i]} -> {target_list[i]}");
                File.Copy(base_path + "/" + source_list[i], base_path + "/" + target_list[i], true);
            }

			AssetDatabase.Refresh();

			//// 이걸 지워야 firebase config가 반영되는것 같당
			//AssetDatabase.DeleteAsset("Assets/StreamingAssets/google-services-desktop");
			//AssetDatabase.Refresh();
		}

		private static void setLocalizationSetting(string packageName)
        {
            LocalizationSettings settings = LocalizationEditorSettings.ActiveLocalizationSettings;

            AOS_APP_INFO androidInfo = settings.GetMetadata().GetMetadata<AOS_APP_INFO>();
            IOS_APP_INFO iosInfo = settings.GetMetadata().GetMetadata<IOS_APP_INFO>();

            androidInfo.DisplayName = new LocalizedString("AppMeta", packageName);
            iosInfo.ShortName = new LocalizedString("AppMeta", packageName);
            iosInfo.DisplayName = new LocalizedString("AppMeta", packageName);

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
		}

        private static void updateLunarConsoleConfig(CommandLineReader parameters)
        {
            bool enable = parameters.getBoolean("enableLunarConsole", true);

            if( enable)
            {
                LunarConsoleEditorInternal.Installer.EnablePlugin();
            }
            else
            {
                LunarConsoleEditorInternal.Installer.DisablePlugin();
            }
        }
	}
}

