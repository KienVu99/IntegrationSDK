#if UNITY_EDITOR && UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using UnityEditor.iOS.Xcode;

namespace aCode.Editor
{
	public abstract class UnityIOSBuilder
	{
		
		[PostProcessBuild(0)]
		public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuildProject)
		{
			if (buildTarget != BuildTarget.iOS) return;
			AddTrackingDescription(pathToBuildProject);
			DisableBitcodeTarget(pathToBuildProject);
		}

		private static void AddTrackingDescription(string pathToBuildProject)
		{
			var plistPath = pathToBuildProject + "/Info.plist";
			var plistObj = new PlistDocument();
			plistObj.ReadFromString(File.ReadAllText(plistPath));
			var plistRoot = plistObj.root;
			plistRoot.SetString("NSUserTrackingUsageDescription", "Your data will be used to provide you a better and personalized ad experience");
			File.WriteAllText(plistPath, plistObj.WriteToString());
		}

		private static void DisableBitcodeTarget(string pathToBuildProject)
		{
			var projectPath = pathToBuildProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
			var pbxProject = new PBXProject();
			pbxProject.ReadFromFile(projectPath);
			var target = pbxProject.GetUnityMainTargetGuid();
			pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
			target = pbxProject.TargetGuidByName(PBXProject.GetUnityTestTargetName());
			pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
			target = pbxProject.GetUnityFrameworkTargetGuid();
			pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
			pbxProject.WriteToFile(projectPath);
		}
	}
}
#endif