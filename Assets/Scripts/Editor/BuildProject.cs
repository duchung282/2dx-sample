namespace Builder
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEditor.Android;
    using UnityEditor.Build.Reporting;
    using UnityEngine; 
    
    public class BuildProject
    {
        private const string AndroidSDKPath = @"D:\Android Stuff\SDK";
        static string[] GetScenePaths()
        {
            List<string> scenes = new List<string>();
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                var scene = EditorBuildSettings.scenes[i];
                if (scene.enabled)
                {
                    scenes.Add(scene.path);
                }
            }

            return scenes.ToArray();
        }
        
        public static void BuildAndroidAPK()
        {
            // APK Expansion: https://docs.unity3d.com/Manual/android-OBBsupport.html
            PlayerSettings.Android.useAPKExpansionFiles = false;
            
            //AndroidExternalToolsSettings.sdkRootPath = AndroidSDKPath;
            EditorUserBuildSettings.buildAppBundle   = false;     // disable aab build
            
            GeneralAndroidSettings(out int code, out string oldVersion);

            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;

            //Get the apk file to be built from the command line argument
            string output = Application.dataPath.Replace("Assets", "Builds") + $"/android.apk";
                
            var report = BuildPipeline.BuildPlayer(GetScenePaths(), output, BuildTarget.Android, BuildOptions.CompressWithLz4HC);

            PlayerSettings.Android.bundleVersionCode = code;
            PlayerSettings.bundleVersion             = oldVersion;
            
            if (report.summary.result != BuildResult.Succeeded)
            {
                EditorApplication.Exit(1);
            }
        }

        public static void BuildAndroidAAB()
        {
            // APK Expansion: https://docs.unity3d.com/Manual/android-OBBsupport.html
            PlayerSettings.Android.useAPKExpansionFiles = true;
            
            //AndroidExternalToolsSettings.sdkRootPath = AndroidSDKPath;
            EditorUserBuildSettings.buildAppBundle   = true;      // enable aab build
            
            GeneralAndroidSettings(out int code, out string oldVersion);

            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;

            //Get the apk file to be built from the command line argument
            string output = Application.dataPath.Replace("Assets", "Builds") + $"/android.aab";
                
            var report = BuildPipeline.BuildPlayer(GetScenePaths(), output, BuildTarget.Android, BuildOptions.CompressWithLz4HC);

            PlayerSettings.Android.bundleVersionCode = code;
            PlayerSettings.bundleVersion             = oldVersion;

            if (report.summary.result != BuildResult.Succeeded)
            {
                EditorApplication.Exit(1);
            }
        }

        private static void GeneralAndroidSettings(out int oldCode, out string oldVersion)
        {
            //Available PlayerSettings: https://docs.unity3d.com/ScriptReference/PlayerSettings.Android.html

            int code = PlayerSettings.Android.bundleVersionCode;
            oldCode = code;
            
            string version = PlayerSettings.bundleVersion;
            oldVersion = version;
            
            var    arr     = version.Split('.');
            if (arr.Length > 1)
            {
                int lastSpot = arr.Length - 1;
                if (int.TryParse(arr[lastSpot], out int last))
                {
                    last++;
                    arr[lastSpot] = last.ToString();

                    string newVersion = string.Join(".", arr);
                    PlayerSettings.bundleVersion = newVersion;
                }
            }
            
            //set the internal apk version to the current unix timestamp, so this increases with every build
            PlayerSettings.Android.bundleVersionCode = code + 1;

            //set the other settings from environment variables
            
            var folderToFind = Application.dataPath.Replace("/Assets", "");

            string[] files = Directory.GetFiles(folderToFind, "*.keystore");

            if (files.Length > 0)
            {
                PlayerSettings.Android.keystoreName = files[0];
            }
            else
            {
                PlayerSettings.Android.keystoreName = KeystoreName;
            }
            
            PlayerSettings.Android.keystorePass = KeystorePass;
            PlayerSettings.Android.keyaliasName = KeystoreName;
            PlayerSettings.Android.keyaliasPass = KeystorePass;
        }
        
        private const string KeystoreName = "2dx";
        private const string KeystorePass = "123456";
    }
}