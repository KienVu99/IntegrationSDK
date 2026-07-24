# GemAPI Documentation

> **Version**: 1.0.0  
> **Unity**: 2021.3+  
> **Platform**: Android, iOS

GemAPI là thư viện SDK hỗ trợ phát triển game mobile Unity, cung cấp các tính năng quảng cáo, analytics, remote config và các tiện ích thường dùng.

---

## 📦 Cài đặt

### 1. Import Package
Copy thư mục `aCode` vào `Assets/` của project Unity.

### 2. Setup Prefab
Kéo prefab `aCodeLoader` vào scene đầu tiên của game:
```
Assets/aCode/Prefabs/aCodeLoader.prefab
```

### 3. Cấu hình
Mở menu **aCode > 01. Install Package** để cấu hình:
- Chọn Ad Network (Admob / Applovin MAX)
- Bật/tắt Firebase Analytics
- Bật/tắt Firebase Messaging
- Bật/tắt Remote Config

---

## 🎯 Quick Start

```csharp
using aCode;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        // Hiển thị banner
        GM.ShowBanner();
        
        // Track screen
        GM.LogScreen("MainMenu");
    }
    
    public void OnPlayButtonClick()
    {
        // Load scene với fade effect
        GM.LoadSceneWithFade("GameScene");
        
        // Track level start
        GM.LevelStart(1);
    }
    
    public void OnWatchAdsClick()
    {
        GM.ShowRewarded(() => {
            AddCoins(100);
            GM.ShowToast("Bạn nhận được 100 coins!");
            GM.Vibrate();
        });
    }
}
```

---

## 📚 API Reference

### Ads - Banner

| Method | Description |
|--------|-------------|
| `GM.ShowBanner(bool collapsible = false)` | Hiển thị banner |
| `GM.HideBanner()` | Ẩn banner |
| `GM.BannerVisible` | Property kiểm tra banner có đang hiển thị |

```csharp
GM.ShowBanner();
GM.ShowBanner(collapsible: true);
if (GM.BannerVisible) GM.HideBanner();
```

---

### Ads - Interstitial

| Method | Description |
|--------|-------------|
| `GM.ShowInterstitial(string location, UnityAction callback = null)` | Hiển thị quảng cáo toàn màn hình |
| `GM.InterstitialReady` | Property kiểm tra sẵn sàng |

```csharp
if (GM.InterstitialReady)
{
    GM.ShowInterstitial("level_complete", () => {
        // Tiếp tục game / Load màn tiếp theo sau khi quảng cáo đóng
        LoadNextLevel();
    });
}
```

---

### Ads - Rewarded Video

| Method | Description |
|--------|-------------|
| `GM.ShowRewarded(UnityAction callback)` | Hiển thị video thưởng |
| `GM.RewardedReady` | Property kiểm tra sẵn sàng |

```csharp
if (GM.RewardedReady)
{
    GM.ShowRewarded(() => {
        coins += 200;
        GM.ShowToast("Nhận 200 coins!");
    });
}
```

---

### Ads - Debug

| Method | Description |
|--------|-------------|
| `GM.ShowDebugger()` | Mở Mediation Debugger |
| `GM.DebugMode` | Property kiểm tra debug mode |

```csharp
if (GM.DebugMode) debugButton.SetActive(true);
GM.ShowDebugger();
```

---

### Scene Management

| Method | Description |
|--------|-------------|
| `GM.LoadScene(string name)` | Load scene theo tên |
| `GM.LoadScene(int index)` | Load scene theo index |
| `GM.LoadSceneAsync(string name, Action<float> onProgress, Action onComplete)` | Load scene async với progress |
| `GM.ReloadScene()` | Reload scene hiện tại |
| `GM.CurrentScene` | Property tên scene hiện tại |
| `GM.CurrentSceneIndex` | Property index scene hiện tại |

```csharp
// Load thường
GM.LoadScene("GameScene");

// Load async với progress bar
GM.LoadSceneAsync("GameScene", 
    progress => loadingBar.value = progress,
    () => Debug.Log("Scene loaded!")
);

// Reload current scene
GM.ReloadScene();
```



### Analytics - Events

| Method | Description |
|--------|-------------|
| `GM.LogEvent(string eventName)` | Track event |
| `GM.LogEvent(string eventName, params object[] parameters)` | Track event với parameters |
| `GM.LogScreen(string screenName)` | Track screen view |
| `GM.SetUserProperty(string name, string value)` | Set user property |

```csharp
GM.LogEvent("tutorial_complete");
GM.LogEvent("purchase", "item_id", "sword", "price", 99);
GM.LogScreen("ShopScreen");
GM.SetUserProperty("vip_status", "gold");
```

---

### Analytics - Level Tracking

| Method | Description |
|--------|-------------|
| `GM.LevelStart(int level)` | Track bắt đầu level |
| `GM.LevelComplete(int level, int playTime)` | Track hoàn thành |
| `GM.LevelFail(int level, int playTime)` | Track thất bại |
| `GM.LevelReplay(int level)` | Track chơi lại |

```csharp
GM.StartTimer();
GM.LevelStart(1);
// ... gameplay ...
GM.LevelComplete(1, GM.ElapsedTime());
```

---

### Remote Config

| Method | Description |
|--------|-------------|
| `GM.GetRemoteString(string key, string defaultValue)` | Lấy string |
| `GM.GetRemoteInt(string key, int defaultValue)` | Lấy int |
| `GM.GetRemoteBool(string key, bool defaultValue)` | Lấy bool |
| `GM.RemoteReady` | Property kiểm tra sẵn sàng |

```csharp
if (GM.RemoteReady)
{
    bool feature = GM.GetRemoteBool("new_feature", false);
    int coins = GM.GetRemoteInt("daily_coins", 100);
}
```

---

### Storage (PlayerPrefs)

| Method | Description |
|--------|-------------|
| `GM.SaveInt(key, value)` | Lưu int |
| `GM.LoadInt(key, defaultValue)` | Đọc int |
| `GM.SaveFloat(key, value)` | Lưu float |
| `GM.LoadFloat(key, defaultValue)` | Đọc float |
| `GM.SaveString(key, value)` | Lưu string |
| `GM.LoadString(key, defaultValue)` | Đọc string |
| `GM.SaveBool(key, value)` | Lưu bool |
| `GM.LoadBool(key, defaultValue)` | Đọc bool |
| `GM.HasKey(key)` | Kiểm tra key tồn tại |
| `GM.DeleteKey(key)` | Xóa key |
| `GM.DeleteAll()` | Xóa tất cả |
| `GM.Counter(key)` | Tăng counter và trả về giá trị |
| `GM.GetCounter(key)` | Lấy giá trị counter |
| `GM.ResetCounter(key)` | Reset counter về 0 |

```csharp
// Save/Load
GM.SaveInt("high_score", 1500);
int score = GM.LoadInt("high_score", 0);

GM.SaveBool("music_on", true);
bool musicOn = GM.LoadBool("music_on", true);

// Counter
int plays = GM.Counter("total_plays"); // 1, 2, 3...
if (plays == 5) ShowRateDialog();
```

---

### Timer & Session

| Method | Description |
|--------|-------------|
| `GM.StartTimer()` | Bắt đầu/reset timer |
| `GM.ElapsedTime()` | Thời gian đã trôi qua (giây) |
| `GM.SessionTime()` | Tổng thời gian session (giây) |

```csharp
GM.StartTimer();
// ... do something ...
int seconds = GM.ElapsedTime();
Debug.Log($"Completed in {seconds}s");
```

---

### Delay & Coroutines

| Method | Description |
|--------|-------------|
| `GM.Delay(float seconds, Action action)` | Chạy action sau delay |
| `GM.DelayRealtime(float seconds, Action action)` | Delay với unscaled time (works when paused) |
| `GM.Run(IEnumerator routine)` | Chạy coroutine |
| `GM.Stop(Coroutine routine)` | Dừng coroutine |

```csharp
// Delay 2 seconds
GM.Delay(2f, () => {
    Debug.Log("2 seconds later!");
});

// Delay even when TimeScale = 0
GM.DelayRealtime(1f, () => {
    Time.timeScale = 1f;
});

// Run custom coroutine
var routine = GM.Run(MyCoroutine());
GM.Stop(routine);
```

---

### Logging

| Method | Description |
|--------|-------------|
| `GM.Print(string tag, object message)` | Print log với tag |
| `GM.PrintLog(object message)` | Print game log |
| `GM.PrintError(string tag, object message)` | Print error |
| `GM.PrintWarning(string tag, object message)` | Print warning |

```csharp
GM.Print("GameManager", "Player spawned");
GM.PrintLog("Game started");
GM.PrintError("Network", "Connection failed");
GM.PrintWarning("Audio", "Sound file not found");
```

---

### App & Store

| Method | Description |
|--------|-------------|
| `GM.OpenRateApp()` | Mở trang đánh giá |
| `GM.OpenURL(string url)` | Mở URL trong browser |
| `GM.Share(string text, string subject)` | Share text (Android) |
| `GM.QuitApp()` | Thoát ứng dụng |
| `GM.AppVersion` | Property version app |
| `GM.BundleId` | Property bundle identifier |

```csharp
GM.OpenRateApp();
GM.OpenURL("https://example.com");
GM.Share("Check out this game!", "My Game");
GM.QuitApp();
```

---

### Device & Platform

| Property | Description |
|----------|-------------|
| `GM.IsMobile` | Đang chạy trên mobile |
| `GM.IsAndroid` | Đang chạy trên Android |
| `GM.IsIOS` | Đang chạy trên iOS |
| `GM.IsEditor` | Đang chạy trong Unity Editor |
| `GM.ScreenWidth` | Chiều rộng màn hình |
| `GM.ScreenHeight` | Chiều cao màn hình |
| `GM.ScreenAspect` | Tỷ lệ màn hình |
| `GM.HasInternet` | Có kết nối internet |
| `GM.IsWifi` | Đang kết nối WiFi |

```csharp
if (GM.IsMobile) EnableTouchControls();
if (!GM.HasInternet) ShowOfflineMessage();
Debug.Log($"Screen: {GM.ScreenWidth}x{GM.ScreenHeight}");
```

---

### Random & Math

| Method | Description |
|--------|-------------|
| `GM.RandomInt(int min, int max)` | Random int [min, max) |
| `GM.RandomFloat(float min, float max)` | Random float |
| `GM.RandomBool()` | Random true/false |
| `GM.RandomElement<T>(T[] array)` | Random element từ array |

```csharp
int dice = GM.RandomInt(1, 7);  // 1-6
float speed = GM.RandomFloat(1f, 5f);
bool heads = GM.RandomBool();
string color = GM.RandomElement(new[] {"red", "blue", "green"});
```

---

### ID Generation

| Method | Description |
|--------|-------------|
| `GM.GenerateUID()` | Tạo unique ID mới |
| `GM.DeviceID` | Property device ID (persistent) |

```csharp
string oderId = GM.GenerateUID();
string device = GM.DeviceID;
```

---

## ⚙️ Cấu hình

### AppSettings.asset

Tạo file `Assets/Resources/aCodeSettings.asset` qua menu **Create > aCode > Create App Settings**

| Property | Description |
|----------|-------------|
| `IsDebugMode` | Bật debug mode |
| `TurnOffAds` | Tắt ads (dev only) |
| `AutoShowBanner` | Tự động show banner |
| `IntervalShowAds` | Thời gian giữa các ads (giây) |
| `BannerPosition` | Vị trí banner (Top/Bottom) |

---

## 📝 Best Practices

### 1. Kiểm tra availability trước khi show ads
```csharp
if (GM.InterstitialReady) GM.ShowInterstitial("location");
```

### 2. Sử dụng location có ý nghĩa
```csharp
GM.ShowInterstitial("level_complete");
GM.ShowInterstitial("shop_exit");
```

### 3. Đợi Remote Config
```csharp
IEnumerator Start()
{
    while (!GM.RemoteReady) yield return null;
    ApplyConfig();
}
```

### 4. Track events quan trọng
```csharp
GM.LogEvent("tutorial_complete");
GM.LogEvent("first_purchase");
GM.LevelStart(1);
```

---

*Last updated: January 2026*
