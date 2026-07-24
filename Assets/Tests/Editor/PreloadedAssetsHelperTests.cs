using IntegrationSDK.Core.Editor;
using NUnit.Framework;
using UnityEngine;

public class PreloadedAssetsHelperTests
{
    [Test]
    public void AddAssetToList_AppendsAsset_WhenListEmpty()
    {
        var asset = ScriptableObject.CreateInstance<IntegrationSDK.Core.IntegrationSDKConfig>();

        var result = PreloadedAssetsHelper.AddAssetToList(new Object[0], asset);

        Assert.AreEqual(1, result.Length);
        Assert.AreSame(asset, result[0]);
    }

    [Test]
    public void AddAssetToList_DoesNotDuplicate_WhenAssetAlreadyPresent()
    {
        var asset = ScriptableObject.CreateInstance<IntegrationSDK.Core.IntegrationSDKConfig>();
        var existing = new Object[] { asset };

        var result = PreloadedAssetsHelper.AddAssetToList(existing, asset);

        Assert.AreEqual(1, result.Length);
    }

    [Test]
    public void AddAssetToList_PreservesOtherEntries()
    {
        var other = ScriptableObject.CreateInstance<IntegrationSDK.Core.IntegrationSDKConfig>();
        var asset = ScriptableObject.CreateInstance<IntegrationSDK.Core.IntegrationSDKConfig>();

        var result = PreloadedAssetsHelper.AddAssetToList(new Object[] { other }, asset);

        Assert.AreEqual(2, result.Length);
        CollectionAssert.Contains(result, other);
        CollectionAssert.Contains(result, asset);
    }
}
