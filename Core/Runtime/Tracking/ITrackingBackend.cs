using System.Collections.Generic;

namespace IntegrationSDK.Core
{
    public interface ITrackingBackend
    {
        void Initialize(IntegrationSDKConfig config);
        void LogEvent(string eventName, Dictionary<string, object> parameters);
        void LogAdRevenue(AdRevenueData data);
    }
}
