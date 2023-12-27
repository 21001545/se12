using System.IO;
using UnityEditor;
using UnityEngine;
#if UNITY_IOS
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
#endif
	public class NGPostProcessBuild
	{
#if UNITY_IOS
		[PostProcessBuild( 1 )]
		public static void OnPostprocessBuild( BuildTarget target, string buildPath )
		{
			if( target == BuildTarget.iOS )
			{
				string pbxProjectPath = PBXProject.GetPBXProjectPath( buildPath );
				string plistPath = Path.Combine( buildPath, "Info.plist" );

				PBXProject pbxProject = new PBXProject();
				pbxProject.ReadFromFile( pbxProjectPath );

				string targetGUID = pbxProject.GetUnityFrameworkTargetGuid();
				pbxProject.AddBuildProperty( targetGUID, "OTHER_LDFLAGS", "-framework Contacts" );
				pbxProject.AddBuildProperty( targetGUID, "OTHER_LDFLAGS", "-framework MessageUI" );
				
				File.WriteAllText( pbxProjectPath, pbxProject.WriteToString() );

				PlistDocument plist = new PlistDocument();
				plist.ReadFromString( File.ReadAllText( plistPath ) );

				PlistElementDict rootDict = plist.root;
				rootDict.SetString( "NSContactsUsageDescription", "Find friend" );
				File.WriteAllText( plistPath, plist.WriteToString() );
			}
		}
#endif
	}