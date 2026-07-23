# IntegrationSDK — Thiết kế

Version: 1.0
Ngày: 2026-07-23

## Mục tiêu

Xây dựng một Unity Package (UPM) đóng gói tích hợp AppLovin MAX (mediation ads) và
AppsFlyer/Firebase (tracking), dùng chung cho nhiều project game. Mục tiêu chính là
tự động hóa tối đa: tự init SDK, tự xử lý consent (GDPR/ATT), tự bắn event tracking
đúng theo `Tracking.md` khi ad hiển thị/click/trả revenue, để game code chỉ cần gọi
1-2 API đơn giản.

Nguồn sự thật cho tên event & tham số: `Tracking.md` (Ad Events, Level Events).

## Phạm vi

**Trong phạm vi:**
- Ad formats: Interstitial, Rewarded, App Open (AOA) — đúng theo `Tracking.md`.
- Tracking backend: AppsFlyer và/hoặc Firebase Analytics, bật/tắt độc lập qua config.
- Tự động hóa: init SDK lúc khởi động, xử lý Consent (Google UMP) + ATT (iOS), auto-wire
  ad revenue (impression) → tracking, Editor Settings window để nhập & lưu config.
- Level tracking tiện ích (`LevelTracker`) tự tính `time_played`.

**Ngoài phạm vi (không làm ở version này):**
- Banner ads, Rewarded Interstitial (chưa có trong `Tracking.md`).
- Adjust hoặc tracking backend khác ngoài AppsFlyer/Firebase.
- Mediation network nào khác ngoài AppLovin MAX.

## Kiến trúc tổng quan

```
com.gemmob.integrationsdk (UPM package, phân phối qua Git URL)
├── package.json          # khai báo dependency: AppLovin MAX (git), AppsFlyer (#upm), EDM4U (git)
├── Runtime/
│   ├── Config/
│   │   └── IntegrationSDKConfig.cs      (ScriptableObject)
│   ├── Bootstrap/
│   │   └── Bootstrapper.cs              (RuntimeInitializeOnLoadMethod)
│   ├── Ads/
│   │   └── AdManager.cs                 (facade: Interstitial/Rewarded/AppOpen)
│   ├── Tracking/
│   │   ├── ITrackingBackend.cs
│   │   ├── AppsFlyerBackend.cs
│   │   ├── FirebaseBackend.cs
│   │   ├── TrackingManager.cs           (fan-out facade)
│   │   └── AdRevenueData.cs
│   └── LevelTracker.cs
└── Editor/
    ├── IntegrationSDKSettingsWindow.cs  (menu: Tools > Integration SDK > Settings)
    └── IntegrationSDKBuildValidator.cs  (IPreprocessBuildWithReport)
```

Firebase yêu cầu thêm 1 bước thủ công 1 lần/project: thêm scoped registry Google vào
`manifest.json` của project (giới hạn kỹ thuật của Unity — scoped registry không thể
khai báo từ bên trong 1 package). Bước này sẽ ghi trong README của package.

## Components

### 1. `IntegrationSDKConfig` (ScriptableObject)
Lưu trong `Assets` (không bắt buộc `Resources`). Trường dữ liệu:
- AppLovin: SDK Key, Ad Unit ID cho Interstitial/Rewarded/AppOpen theo từng platform
  (Android/iOS).
- AppsFlyer: Dev Key, iOS App ID, toggle Enabled.
- Firebase: toggle Enabled.
- Consent: toggle tự động hiện Google UMP + ATT trước khi init ads.

### 2. Editor Settings window
Menu `Tools > Integration SDK > Settings`. Lần đầu mở: cho chọn nơi lưu asset config
trong `Assets` (nhớ path qua `EditorPrefs`); các lần sau: load asset đã có để sửa.
Nút **Save**: ghi asset + tự động thêm asset vào `Player Settings > Preloaded Assets`
để nó được đóng gói vào build và truy xuất runtime qua
`Resources.FindObjectsOfTypeAll<IntegrationSDKConfig>()` mà không cần đặt trong
thư mục `Resources`.

### 3. Build validation
`IPreprocessBuildWithReport`: nếu 1 backend đang bật (AppsFlyer/Firebase) hoặc MAX mà
thiếu key/Ad Unit ID bắt buộc, cảnh báo và chặn build.

### 4. `Bootstrapper` (runtime, tự chạy — không cần đặt GameObject)
`[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]`, thứ tự:
1. Load `IntegrationSDKConfig`.
2. Nếu bật Consent: hiện Google UMP form (nếu áp dụng theo vùng) rồi request ATT (iOS).
3. Init AppLovin MAX SDK.
4. Init các backend tracking đang bật (AppsFlyer/Firebase).

### 5. `AdManager` (facade)
API: `ShowInterstitial(string placement)`, `ShowRewarded(string placement, Action onReward)`,
`ShowAppOpen(string placement)`. Tự lo load/cache/retry ẩn sau lưng game code.

Tự động bắn event theo `Tracking.md` tại đúng thời điểm (game code không tự gọi):
- Gọi `Show...` → bắn `show_interstitial_ads` / `show_rewarded_ads` / `show_aoa_ads`
  với `placement`.
- MAX báo hiển thị (`OnAdDisplayedEvent`) → bắn `..._success`.
- MAX báo click (`OnAdClickedEvent`) → bắn `..._click`.
- MAX báo trả revenue (`OnAdRevenuePaidEvent`, mọi format) → bắn `ad_impression` với
  `ad_platform, ad_source, ad_unit_name, currency, value, placement, country_code, ad_format`
  lấy trực tiếp từ dữ liệu MAX trả về.

### 6. `TrackingManager` + `ITrackingBackend`
- `ITrackingBackend.LogEvent(string name, Dictionary<string,object> parameters)` và
  `LogAdRevenue(AdRevenueData data)`.
- `AppsFlyerBackend`, `FirebaseBackend` implement interface, map generic params sang
  API riêng (`AppsFlyer.sendEvent`, `FirebaseAnalytics.LogEvent`, hàm log ad revenue
  riêng cho `ad_impression`).
- `TrackingManager.LogEvent(name, params)` là API duy nhất cho game code (event tùy ý
  ngoài ads/level); fan-out tới mọi backend đang bật, game code không cần biết SDK
  phía sau là gì.
- Lỗi/SDK chưa init: adapter log warning và bỏ qua, không crash game.

### 7. `LevelTracker`
`LevelTracker.Start(level)` lưu timestamp; `.Passed(level)` / `.Failed(level)` tự tính
`time_played` và gọi `TrackingManager.LogEvent` với đúng tên event `level_start` /
`level_passed` / `level_failed` trong `Tracking.md`.

## Error handling
- Backend/adapter lỗi hoặc SDK chưa sẵn sàng: log warning, không throw, không chặn
  luồng game.
- Build validator chặn build nếu thiếu config bắt buộc cho backend đang bật.
- Ad load fail: `AdManager` tự retry theo backoff đơn giản, không bắn event thành công.

## Testing
- EditMode test cho `TrackingManager` với `ITrackingBackend` mock: kiểm tra fan-out
  đúng backend đang bật, mapping tên event/tham số đúng `Tracking.md`.
- EditMode test cho `LevelTracker`: `time_played` tính đúng khoảng thời gian.
- Gọi SDK thật (MAX/AppsFlyer/Firebase) chỉ kiểm chứng được qua build thật trên
  device/emulator — ghi rõ trong README, không nằm trong test tự động.

## Câu hỏi còn mở (không chặn implementation)
- Tên chính xác của package (`com.gemmob.integrationsdk` là tạm đặt, có thể đổi).
- Repo Git host cho package (GitHub org nào, public/private).
