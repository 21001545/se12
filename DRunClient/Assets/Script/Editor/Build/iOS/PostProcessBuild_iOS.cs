using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_IPHONE

using UnityEditor.iOS.Xcode;
using System.IO;
//using UnityEditor.iOS.Xcode.Extensions;
using Festa.Client.Module;

namespace Festa
{
    public static class PostProcessBuild_InfoPList
	{
        private static JsonObject _buildVersion;

        [PostProcessBuild(45)]
        private static void OnPostProcessBuild_ForPOD(BuildTarget buildTarget,string path)
        {
           if( buildTarget != BuildTarget.iOS)
           {
               return;
           }

           readBuildVersion();

           //addNotificationService(path);
           //addNotificationContent(path);

           //string pod_file_source = File.ReadAllText( Application.dataPath + "/Script/Editor/Build/iOS/PodFile");
           //File.WriteAllText( path + "/PodFile", pod_file_source);

           // swpark, iOS keyboard 툴바를 가리기 위한.. 코드 대체
           // 2021.2.121f 버전
           string keyboard_file_source = File.ReadAllText( Application.dataPath + "/Script/Editor/Build/iOS/Keyboard.mm");
           File.WriteAllText( path + "/Classes/UI/Keyboard.mm", keyboard_file_source);
        }

        [PostProcessBuild(9999)]
        public static void OnPostProcessBuild(BuildTarget buildTarget,string path)
		{
            if( buildTarget != BuildTarget.iOS)
			{
                return;
			}

            changeInfoPList(path);
            addLinkToSQLiteIos(path);
            setupEntitlements(path);
            setupProjectVersion(path);
            setupBitcode(path,false);
            //copyFastlane(path);
        }

        private static void readBuildVersion()
        {
            string path = Application.dataPath + "/build_version.json";
            string txt_data = File.ReadAllText(path);

            JsonObject json = new JsonObject(txt_data);
            _buildVersion = json.getJsonObject("ios");
        }

        private static void changeInfoPList(string path)
		{
            string infoPlistPath = path + "/Info.plist";

            PlistDocument plistDoc = new PlistDocument();
            plistDoc.ReadFromFile(infoPlistPath);
            if (plistDoc.root == null)
            {
                Debug.LogError("Can't open " + infoPlistPath);
                return;
            }


            // 2022.12.02 이강희 healtkit 제거
            //plistDoc.root.SetString("NSHealthShareUsageDescription", "access step count to claim reward for walking");
            //plistDoc.root.SetString("NSHealthUpdateUsageDescription", "access step count to claim reward for walking");
            
            plistDoc.root.SetString("ITSAppUsesNonExemptEncryption", "NO");
            plistDoc.root.SetString("NSMotionUsageDescription", "access step count to claim reward for walking");
            plistDoc.root.SetString("NSLocationWhenInUseUsageDescription", "access location for trip logging");
            plistDoc.root.SetString("NSLocationUsageDescription", "access location for trip logging");
            plistDoc.root.SetString("NSLocationAlwaysUsageDescription", "access location for trip logging");
            plistDoc.root.SetString("NSLocationAlwaysAndWhenInUseUsageDescription", "access location for trip logging");
            plistDoc.root.SetBoolean("UIViewControllerBasedStatusBarAppearance", false);
            plistDoc.root.SetBoolean("UIStatusBarHidden", false);
            //plistDoc.root.SetString("UIStatusBarStyle", "UIStatusBarStyleDarkContent");
            plistDoc.root.SetString("UIStatusBarStyle", "UIStatusBarStyleLightContent");
            plistDoc.root.SetString("CFBundleShortVersionString", "$(MARKETING_VERSION)");
            plistDoc.root.SetString("CFBundleVersion", "$(CURRENT_PROJECT_VERSION)");

            plistDoc.WriteToFile(infoPlistPath);

        }

        private static void addLinkToSQLiteIos(string pathToBuiltProject)
        {
            string projPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
            PBXProject proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));
            string targetGUID = proj.GetUnityMainTargetGuid();
            proj.AddBuildProperty(targetGUID, "OTHER_LDFLAGS", "-lsqlite3");
            File.WriteAllText(projPath, proj.WriteToString());
        }

        private static void copyFilesForNotificationService(string pathToBuiltProject)
        {
            string src_dir = Application.dataPath + "/Script/NotificationServiceExtension";
            string target_dir = pathToBuiltProject + "/NotificaionServiceExtension";

            if( Directory.Exists( target_dir) == false)
            {
                Directory.CreateDirectory( target_dir);
            }

            File.Copy( src_dir + "/Info.plist", target_dir + "/Info.plist", true);
            File.Copy( src_dir + "/NotificationService.h", target_dir + "/NotificationService.h", true);
            File.Copy( src_dir + "/NotificationService.m", target_dir + "/NotificationService.m", true);
        }

  //      private static void addNotificationService(string pathToBuiltProject)
		//{
  //          //copyFilesForNotificationService(pathToBuiltProject);

  //          var projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
  //          PBXProject proj = new PBXProject();
  //          proj.ReadFromFile(projPath);

  //          string targetGUID = proj.GetUnityMainTargetGuid();

  //          var plistPath = pathToBuiltProject + "/Info.plist";
  //          PlistDocument plist = new PlistDocument();
  //          plist.ReadFromFile(plistPath);

  //          //var pathToNotificationServiceSource = ;
  //          var pathToNotificationService = Application.dataPath + "/Script/NotificationServiceExtension";//pathToBuiltProject + "/NotificaionServiceExtension";
  //          var notificationServicePlistPath = pathToNotificationService + "/Info.plist";
  //          PlistDocument notificationServicePlist = new PlistDocument();
  //          notificationServicePlist.ReadFromFile(notificationServicePlistPath);
  //          notificationServicePlist.root.SetString("CFBundleShortVersionString", _buildVersion.getString("marketing_version"));
  //          notificationServicePlist.root.SetString("CFBundleVersion", _buildVersion.getInteger("project_version").ToString());
  //          string notificationServiceTarget = proj.TargetGuidByName( "NotificationService");
            
  //          if( notificationServiceTarget == null)
  //          {
  //             notificationServiceTarget = PBXProjectExtensions.AddAppExtension(proj, targetGUID, "NotificationService", PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS) + ".notificationservice", notificationServicePlistPath);
  //          }
  //          //proj.AddFileToBuild(notificationServiceTarget, proj.AddFile(pathToNotificationService + "/NotificationService.h", "NotificationService/NotificationService.h"));
  //          //proj.AddFileToBuild(notificationServiceTarget, proj.AddFile(pathToNotificationService + "/NotificationService.m", "NotificationService/NotificationService.m"));

  //          proj.AddFile(pathToNotificationService + "/Info.plist", "NotificationService/Info.plist");
  //          proj.AddFile(pathToNotificationService + "/NotificationService.h", "NotificationService/NotificationService.h");
  //          proj.AddFileToBuild(notificationServiceTarget, proj.AddFile(pathToNotificationService + "/NotificationService.m", "NotificationService/NotificationService.m"));

  //          proj.AddFrameworkToProject(notificationServiceTarget, "NotificationCenter.framework", true);
  //          proj.AddFrameworkToProject(notificationServiceTarget, "UserNotifications.framework", true);
  //          proj.SetBuildProperty(notificationServiceTarget, "ARCHS", "$(ARCHS_STANDARD)");
  //          proj.SetBuildProperty(notificationServiceTarget, "DEVELOPMENT_TEAM", PlayerSettings.iOS.appleDeveloperTeamID);
  //          notificationServicePlist.WriteToFile(notificationServicePlistPath);

  //          proj.WriteToFile(projPath);
  //          plist.WriteToFile(plistPath);
  //      }

  //      private static void addNotificationContent(string pathToBuiltProject)
  //      {
  //          var projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
  //          PBXProject proj = new PBXProject();
  //          proj.ReadFromFile(projPath);
  //          string targetGUID = proj.GetUnityMainTargetGuid();

  //          var plistPath = pathToBuiltProject + "/Info.plist";
  //          PlistDocument plist = new PlistDocument();
  //          plist.ReadFromFile(plistPath);

  //          var pathToNotificationContent = Application.dataPath + "/Script/NotificationContentExtension";//pathToBuiltProject + "/NotificaionServiceExtension";
  //          var notificationContentPlistPath = pathToNotificationContent + "/Info.plist";
  //          PlistDocument notificationContentPlist = new PlistDocument();
  //          notificationContentPlist.ReadFromFile(notificationContentPlistPath);
  //          notificationContentPlist.root.SetString("CFBundleShortVersionString", _buildVersion.getString("marketing_version"));
  //          notificationContentPlist.root.SetString("CFBundleVersion", _buildVersion.getInteger("project_version").ToString());
  //          string notificationContentTarget = proj.TargetGuidByName("NotificationContent");
            
  //          if( notificationContentTarget == null)
  //          {
  //              notificationContentTarget = PBXProjectExtensions.AddAppExtension(proj, targetGUID, "NotificationContent", PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS) + ".notificationcontent", notificationContentPlistPath);
  //          }

  //          proj.AddFile(pathToNotificationContent + "/Info.plist", "NotificationContent/Info.plist");
  //          proj.AddFile(pathToNotificationContent + "/NotificationViewController.h", "NotificationContent/NotificationViewController.h");
  //          proj.AddFileToBuild(notificationContentTarget, proj.AddFile(pathToNotificationContent + "/Base.lproj/NotificationView.storyboard", "NotificationContent/NotificationView.storyboard"));
  //          proj.AddFileToBuild(notificationContentTarget, proj.AddFile(pathToNotificationContent + "/NotificationViewController.m", "NotificationContent/NotificationViewController.m"));

  //          //proj.AddFrameworkToProject(notificationContentTarget, "NotificationCenter.framework", true);
  //          proj.AddFrameworkToProject(notificationContentTarget, "UserNotifications.framework", true);
  //          proj.AddFrameworkToProject(notificationContentTarget, "UserNotificationsUI.framework", true);
  //          proj.AddFrameworkToProject(notificationContentTarget, "UIKit.framework", true);
  //          proj.SetBuildProperty(notificationContentTarget, "ARCHS", "$(ARCHS_STANDARD)");
  //          proj.SetBuildProperty(notificationContentTarget, "DEVELOPMENT_TEAM", PlayerSettings.iOS.appleDeveloperTeamID);
  //          notificationContentPlist.WriteToFile(notificationContentPlistPath);

  //          proj.WriteToFile(projPath);
  //          plist.WriteToFile(plistPath);
  //      }

        // entitlements에서 healthkit 제거
        private const string entitlements = @"
        <?xml version=""1.0"" encoding=""UTF-8\""?>
            <!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
            <plist version=""1.0"">
            <dict>
                <key>aps-environment</key>
                <string>development</string>
               </dict>
        </plist>";

        private static void setupEntitlements(string path)
        {
 
            var file_name = "unity.entitlements";
            var proj_path = PBXProject.GetPBXProjectPath(path);
            var proj = new PBXProject();
            proj.ReadFromFile(proj_path);
            
            var target_guid = proj.GetUnityMainTargetGuid();       
            var dst = path + "/" + file_name;
            try
            {
                File.WriteAllText(dst, entitlements);
                proj.AddFile(file_name, file_name);
                proj.AddBuildProperty(target_guid, "CODE_SIGN_ENTITLEMENTS", file_name);
                proj.WriteToFile(proj_path);
            }
            catch (IOException e)
            {
                Debug.LogException(e);
            }
        }

        private static void setupProjectVersion(string path)
        {
			CommandLineReader parameters = CommandLineReader.create();
			int buildNumber = parameters.getInteger("buildNumber", 1);

			var proj_path = PBXProject.GetPBXProjectPath(path);
            var proj = new PBXProject();
            proj.ReadFromFile(proj_path);

            var target_guid = proj.GetUnityMainTargetGuid();

            Debug.Log($"marketing_version:{_buildVersion.getString("marketing_version")} buildNumber:{buildNumber}");

            try
            {
                proj.SetBuildProperty(target_guid, "MARKETING_VERSION", _buildVersion.getString("marketing_version"));
                proj.SetBuildProperty(target_guid, "CURRENT_PROJECT_VERSION", buildNumber.ToString());
                proj.WriteToFile(proj_path);
            }
            catch (IOException e)
            {
                Debug.LogException(e);
            }
        }

        private static void setupBitcode(string pathToBuiltProject,bool enableBitcode) {
            var project = new PBXProject();
            var pbxPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            project.ReadFromFile(pbxPath);
            setupBitcodeFramework(project,enableBitcode);
            setupBitcodeMain(project,enableBitcode);
            project.WriteToFile(pbxPath);
        }
 
        private static void setupBitcodeFramework(PBXProject project,bool enableBitcode) {
            setupBitcode(project, project.GetUnityFrameworkTargetGuid(), enableBitcode);
        }
 
        private static void setupBitcodeMain(PBXProject project,bool enableBitcode) {
            setupBitcode(project, project.GetUnityMainTargetGuid(),enableBitcode);
        }
 
        private static void setupBitcode(PBXProject project, string targetGUID,bool enableBitcode) {
            project.SetBuildProperty(targetGUID, "ENABLE_BITCODE", enableBitcode ? "YES" : "NO");
        }

        private static void copyFastlane(string path)
		{
            string src_dir = Application.dataPath + "/Script/Editor/Build/iOS/fastlane";
            string target_dir = path + "/fastlane";
            if( Directory.Exists(target_dir) == false)
			{
                Directory.CreateDirectory(target_dir);
			}

            string[] file_list = { 
                "AppFile",
                "Fastfile",
                "README.md",
                "report.xml"
            };
            
            foreach(string file in file_list)
			{
                File.Copy(src_dir + "/" + file, target_dir + "/" + file, true);
			}
        }
    }
}

#endif