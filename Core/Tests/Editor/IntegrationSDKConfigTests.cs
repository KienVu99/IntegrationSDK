using IntegrationSDK.Core;
using NUnit.Framework;
using UnityEngine;

public class IntegrationSDKConfigTests
{
    [Test]
    public void GetInterstitialAdUnitId_ReturnsAndroidValue_WhenNotIOS()
    {
        var config = ScriptableObject.CreateInstance<IntegrationSDKConfig>();
        config.androidInterstitialAdUnitId = "android-int-id";
        config.iosInterstitialAdUnitId = "ios-int-id";

        var result = config.GetInterstitialAdUnitIdForPlatform(RuntimePlatform.Android);

        Assert.AreEqual("android-int-id", result);
    }

    [Test]
    public void GetInterstitialAdUnitId_ReturnsIOSValue_WhenIOS()
    {
        var config = ScriptableObject.CreateInstance<IntegrationSDKConfig>();
        config.androidInterstitialAdUnitId = "android-int-id";
        config.iosInterstitialAdUnitId = "ios-int-id";

        var result = config.GetInterstitialAdUnitIdForPlatform(RuntimePlatform.IPhonePlayer);

        Assert.AreEqual("ios-int-id", result);
    }
}
