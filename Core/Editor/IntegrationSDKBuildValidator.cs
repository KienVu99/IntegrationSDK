using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace IntegrationSDK.Core.Editor
{
    public class IntegrationSDKBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var config = Resources.FindObjectsOfTypeAll<IntegrationSDKConfig>().FirstOrDefault();
            var errors = IntegrationSDKConfigValidator.Validate(config);

            if (errors.Count > 0)
            {
                throw new BuildFailedException("IntegrationSDK config errors:\n- " + string.Join("\n- ", errors));
            }
        }
    }
}
