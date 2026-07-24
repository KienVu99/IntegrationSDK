using System.Linq;
using UnityEditor;
using UnityEngine;

namespace IntegrationSDK.Core.Editor
{
    public static class PreloadedAssetsHelper
    {
        public static Object[] AddAssetToList(Object[] existing, Object asset)
        {
            if (asset == null)
            {
                return existing;
            }

            if (existing.Any(o => o == asset))
            {
                return existing;
            }

            var result = new Object[existing.Length + 1];
            existing.CopyTo(result, 0);
            result[existing.Length] = asset;
            return result;
        }

        public static void RegisterInPlayerSettings(Object asset)
        {
            var updated = AddAssetToList(PlayerSettings.GetPreloadedAssets(), asset);
            PlayerSettings.SetPreloadedAssets(updated);
        }
    }
}
