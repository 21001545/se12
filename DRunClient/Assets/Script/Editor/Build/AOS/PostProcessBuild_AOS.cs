using Festa.Client.Module;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Festa
{
    public static class PostProcessBuild_AOS
    {
		[PostProcessBuild(9999)]
        public static void OnPostProcessBuild(BuildTarget buildTarget,string path)
		{
            if( buildTarget != BuildTarget.Android)
			{
                return;
			}

            //copyFastlane( Path.GetDirectoryName(path));
		}

        private static void copyFastlane(string path)
        {
            string src_dir = Application.dataPath + "/Script/Editor/Build/AOS/fastlane";
            string target_dir = path + "/fastlane";
            if (Directory.Exists(target_dir) == false)
            {
                Directory.CreateDirectory(target_dir);
            }

            string[] file_list = {
                "AppFile",
                "Fastfile",
                "README.md",
                "report.xml"
            };

            foreach (string file in file_list)
            {
                File.Copy(src_dir + "/" + file, target_dir + "/" + file, true);
            }
        }
    }

}


