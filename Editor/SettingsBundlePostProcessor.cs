using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System.IO;
using UnityEditor.iOS.Xcode;

namespace InactivityReset.Editor
{
    public static class SettingsBundlePostProcessor
    {
        private const string SettingsBundleRelativePath = "Assets/Plugins/iOS/Settings.bundle";
        private const string RootPlistFileName = "Root.plist";

        [MenuItem("Tools/Inactivity Reset/Create iOS Settings Bundle")]
        public static void CreateSettingsBundle()
        {
            var settingsBundlePath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, SettingsBundleRelativePath);
            var rootPlistPath = Path.Combine(settingsBundlePath, RootPlistFileName);

            if (File.Exists(rootPlistPath))
            {
                EditorUtility.DisplayDialog(
                    "Inactivity Reset",
                    "The iOS Settings.bundle already exists, so it was left unchanged.",
                    "OK");
                return;
            }

            Directory.CreateDirectory(settingsBundlePath);
            File.WriteAllText(rootPlistPath, RootPlistContents);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "Inactivity Reset",
                "Created Assets/Plugins/iOS/Settings.bundle with the default inactivity settings.",
                "OK");
        }

        [PostProcessBuild(1000)]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS)
            {
                return;
            }

            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            var settingsBundlePath = Path.Combine(projectRoot, SettingsBundleRelativePath);
            if (!Directory.Exists(settingsBundlePath))
            {
                return;
            }

            var builtBundlePath = Path.Combine(pathToBuiltProject, "Settings.bundle");
            if (!Directory.Exists(builtBundlePath))
            {
                FileUtil.CopyFileOrDirectory(settingsBundlePath, builtBundlePath);
            }

            var projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);

            var frameworkTargetGuid = project.GetUnityFrameworkTargetGuid();
            var frameworkBundleGuid = project.FindFileGuidByProjectPath("Frameworks/Plugins/iOS/Settings.bundle");
            if (!string.IsNullOrEmpty(frameworkBundleGuid))
            {
                project.RemoveFileFromBuild(frameworkTargetGuid, frameworkBundleGuid);
                project.RemoveFile(frameworkBundleGuid);
            }

            var targetGuid = project.GetUnityMainTargetGuid();
            var fileGuid = project.FindFileGuidByProjectPath("Settings.bundle");
            if (string.IsNullOrEmpty(fileGuid))
            {
                fileGuid = project.AddFile("Settings.bundle", "Settings.bundle");
            }
            project.AddFileToBuild(targetGuid, fileGuid);
            project.WriteToFile(projectPath);
        }

        private const string RootPlistContents = @"<?xml version=""1.0"" encoding=""UTF-8""?>
    <!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
    <plist version=""1.0"">
<dict>
    <key>PreferenceSpecifiers</key>
    <array>
        <dict>
            <key>Type</key>
            <string>PSGroupSpecifier</string>
            <key>Title</key>
            <string>Inactivity Reset</string>
        </dict>
        <dict>
            <key>Type</key>
            <string>PSTextFieldSpecifier</string>
            <key>Title</key>
            <string>Timeout (seconds)</string>
            <key>Key</key>
            <string>inactivity_timeout_seconds</string>
            <key>DefaultValue</key>
            <string>60</string>
            <key>KeyboardType</key>
            <string>NumberPad</string>
        </dict>
        <dict>
            <key>Type</key>
            <string>PSTextFieldSpecifier</string>
            <key>Title</key>
            <string>Countdown (seconds)</string>
            <key>Key</key>
            <string>inactivity_countdown_seconds</string>
            <key>DefaultValue</key>
            <string>5</string>
            <key>KeyboardType</key>
            <string>NumberPad</string>
        </dict>
    </array>
</dict>
</plist>
";
    }
}
