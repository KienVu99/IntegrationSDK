using IntegrationSDK.Core;
using IntegrationSDK.Core.Editor;
using NUnit.Framework;
using UnityEngine;

public class IntegrationSDKConfigValidatorTests
{
    [Test]
    public void Validate_ReturnsNoErrors_ForFullyConfiguredConfig()
    {
        var config = ScriptableObject.CreateInstance<IntegrationSDKConfig>();
        config.maxSdkKey = "sdk-key";
        config.androidInterstitialAdUnitId = "and-int";
        config.iosInterstitialAdUnitId = "ios-int";
        config.androidRewardedAdUnitId = "and-rew";
        config.iosRewardedAdUnitId = "ios-rew";
        config.androidAppOpenAdUnitId = "and-aoa";
        config.iosAppOpenAdUnitId = "ios-aoa";

        var errors = IntegrationSDKConfigValidator.Validate(config);

        Assert.IsEmpty(errors);
    }

    [Test]
    public void Validate_ReturnsError_WhenMaxSdkKeyMissing()
    {
        var config = ScriptableObject.CreateInstance<IntegrationSDKConfig>();

        var errors = IntegrationSDKConfigValidator.Validate(config);

        CollectionAssert.Contains(errors, "AppLovin MAX SDK Key is missing.");
    }

    [Test]
    public void Validate_ReturnsError_WhenAppsFlyerEnabledButDevKeyMissing()
    {
        var config = ScriptableObject.CreateInstance<IntegrationSDKConfig>();
        config.maxSdkKey = "sdk-key";
        config.androidInterstitialAdUnitId = "x";
        config.iosInterstitialAdUnitId = "x";
        config.androidRewardedAdUnitId = "x";
        config.iosRewardedAdUnitId = "x";
        config.androidAppOpenAdUnitId = "x";
        config.iosAppOpenAdUnitId = "x";
        config.appsFlyerEnabled = true;

        var errors = IntegrationSDKConfigValidator.Validate(config);

        CollectionAssert.Contains(errors, "AppsFlyer is enabled but Dev Key is missing.");
    }

    [Test]
    public void Validate_ReturnsNull_ConfigError_WhenConfigMissing()
    {
        var errors = IntegrationSDKConfigValidator.Validate(null);

        CollectionAssert.Contains(errors, "IntegrationSDKConfig asset not found. Open Tools > Integration SDK > Settings to create one.");
    }
}
