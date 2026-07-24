#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;

namespace aCode.Editor.Utils
{
    public static class EditorUtils
    {
        private static BuildTargetGroup[] _sWorkingBuildTargetGroups = null;
        
        public static BuildTargetGroup[] GetWorkingBuildTargetGroups()
        {
            if (_sWorkingBuildTargetGroups != null) return _sWorkingBuildTargetGroups;
            var btgType = typeof(BuildTargetGroup);
            _sWorkingBuildTargetGroups = (from name in System.Enum.GetNames(btgType) let memberInfo = btgType.GetMember(name)[0] where !System.Attribute.IsDefined(memberInfo, typeof(System.ObsoleteAttribute)) select (BuildTargetGroup)Enum.Parse(btgType, name) into g where g != BuildTargetGroup.Unknown select g).ToArray();
            return _sWorkingBuildTargetGroups;
        }

    }
}
#endif