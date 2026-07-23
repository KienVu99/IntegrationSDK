# IntegrationSDK — Thiết kế

Version: 1.1
Ngày: 2026-07-23

## Mục tiêu

Xây dựng các Unity Package (UPM) đóng gói tích hợp AppLovin MAX (mediation ads) và
AppsFlyer/Firebase (tracking), dùng chung cho nhiều project game. Mục tiêu chính là
tự động hóa tối đa: tự init SDK, tự xử lý consent (GDPR/ATT), tự bắn event tracking
đúng theo `Tracking.md` khi ad hiển thị/click/trả revenue, để game code chỉ cần gọi
1-2 API đơn giản. Mỗi tích hợp cụ thể (MAX/AppsFlyer/Firebase) là 1 module riêng,
add/xóa độc lập theo từng project mà không đụng vào phần lõi.

Nguồn sự thật cho tên event & tham số: `Tracking.md` (Ad Events, Level Events).

## Phạm vi

**Trong phạm vi:**
- Ad formats: Interstitial, Rewarded, App Open (AOA) — đúng theo `Tracking.md`.
- Tracking backend: AppsFlyer và/hoặc Firebase Analytics, mỗi backend là 1 module,
  bật/tắt độc lập bằng cách add/xóa package trong `manifest.json` + toggle trong config.
- Tự động hóa: init SDK lúc khởi động, xử lý Consent (Google UMP) + ATT (iOS), auto-wire
  ad revenue (impression) → tracking, Editor Settings window để nhập & lưu config.
- Level tracking tiện ích (`LevelTracker`) tự tính `time_played`.
- Kiến trúc module hóa: Core không phụ thuộc bất kỳ SDK bên thứ 3 nào; từng tích hợp
  cụ thể nằm trong module riêng, tự đăng ký vào Core lúc khởi động.

**Ngoài phạm vi (không làm ở version này):**
- Banner ads, Rewarded Interstitial (chưa có trong `Tracking.md`).
- Adjust hoặc tracking backend khác ngoài AppsFlyer/Firebase.
- Mediation network nào khác ngoài AppLovin MAX.

## Kiến trúc tổng quan — module hóa

Một repo Git duy nhất, nhiều UPM package con (giống cách project hiện tại dùng
`com.coplaydev.unity-mcp` qua `?path=/MCPForUnity#main`). Mỗi tích hợp là 1 package
độc lập, add/xóa qua `manifest.json` không đụng tới Core:

```
IntegrationSDK.git (1 repo)
│
├── Core/                              → com.gemmob.integrationsdk.core
│   ├── package.json                   (KHÔNG phụ thuộc MAX/AppsFlyer/Firebase)
│   ├── Runtime/
│   │   ├── Config/IntegrationSDKConfig.cs   (ScriptableObject)
│   │   ├── Bootstrap/Bootstrapper.cs        (RuntimeInitializeOnLoadMethod)
│   │   ├── Ads/IAdProvider.cs               (interface)
│   │   ├── Ads/AdManager.cs                 (facade, gọi qua IAdProvider đã đăng ký)
│   │   ├── Tracking/ITrackingBackend.cs
│   │   ├── Tracking/AdRevenueData.cs
│   │   ├── Tracking/TrackingManager.cs      (fan-out facade)
│   │   └── LevelTracker.cs
│   └── Editor/
│       ├── IntegrationSDKSettingsWindow.cs  (menu: Tools > Integration SDK > Settings)
│       └── IntegrationSDKBuildValidator.cs  (IPreprocessBuildWithReport)
│
├── Ads.AppLovin/                       → com.gemmob.integrationsdk.ads.applovin
│   ├── package.json                   (deps: Core + AppLovin MAX git + EDM4U)
│   └── Runtime/MaxAdProvider.cs        : IAdProvider — tự Register vào AdManager
│
├── Tracking.AppsFlyer/                → com.gemmob.integrationsdk.tracking.appsflyer
│   ├── package.json                   (deps: Core + AppsFlyer Unity Plugin #upm)
│   └── Runtime/AppsFlyerBackend.cs     : ITrackingBackend — tự Register vào TrackingManager
│
└── Tracking.Firebase/                 → com.gemmob.integrationsdk.tracking.firebase
    ├── package.json                   (deps: Core + Firebase Analytics)
    └── Runtime/FirebaseBackend.cs      : ITrackingBackend — tự Register vào TrackingManager
```

**Nguyên tắc phụ thuộc (một chiều, không vòng lặp):** mọi module tham chiếu Core;
Core không bao giờ tham chiếu ngược lại module nào. Nhờ vậy Core compile được kể cả
khi dự án không cài module nào.

**Cơ chế tự đăng ký (self-registration, không dùng reflection quét assembly):**
- Mỗi module có 1 điểm khởi tạo riêng dùng
  `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]`, gọi
  `AdManager.RegisterProvider(new MaxAdProvider())` hoặc
  `TrackingManager.RegisterBackend(new AppsFlyerBackend())`.
- **Thêm module:** add package vào `manifest.json` → assembly tồn tại → lệnh Register
  tự chạy → Core tự nhận ra provider/backend mới, không cần sửa code Core.
- **Xóa module:** xóa dòng dependency trong `manifest.json` → assembly biến mất →
  không còn lệnh Register nào gọi tới nó → Core tự tiếp tục chạy bình thường (danh
  sách backend/provider rỗng cho phần đó), không lỗi biên dịch, không crash.
- Muốn thêm tích hợp mới sau này (ví dụ Adjust) chỉ cần tạo package mới theo đúng
  pattern, implement `ITrackingBackend`, không đụng vào Core.

Firebase yêu cầu thêm 1 bước thủ công 1 lần/project: thêm scoped registry Google vào
`manifest.json` của project (giới hạn kỹ thuật của Unity — scoped registry không thể
khai báo từ bên trong 1 package). Bước này sẽ ghi trong README của package
`Tracking.Firebase`.

## Components

### 1. `IntegrationSDKConfig` (ScriptableObject, trong Core)
Lưu trong `Assets` (không bắt buộc `Resources`). Trường dữ liệu (dùng kiểu nguyên
thủy — string/enum/bool — nên không tạo phụ thuộc ngược từ Core sang SDK bên thứ 3):
- AppLovin: SDK Key, Ad Unit ID cho Interstitial/Rewarded/AppOpen theo từng platform
  (Android/iOS).
- AppsFlyer: Dev Key, iOS App ID, toggle Enabled.
- Firebase: toggle Enabled.
- Consent: toggle tự động hiện Google UMP + ATT trước khi init ads.

Nếu 1 module không được cài trong project, các field tương ứng trong config chỉ đơn
giản không được dùng tới — không gây lỗi.

### 2. Editor Settings window (Core)
Menu `Tools > Integration SDK > Settings`. Lần đầu mở: cho chọn nơi lưu asset config
trong `Assets` (nhớ path qua `EditorPrefs`); các lần sau: load asset đã có để sửa.
Nút **Save**: ghi asset + tự động thêm asset vào `Player Settings > Preloaded Assets`
để nó được đóng gói vào build và truy xuất runtime qua
`Resources.FindObjectsOfTypeAll<IntegrationSDKConfig>()` mà không cần đặt trong
thư mục `Resources`.

### 3. Build validation (Core)
`IPreprocessBuildWithReport`: nếu 1 backend đang bật (AppsFlyer/Firebase) hoặc MAX mà
thiếu key/Ad Unit ID bắt buộc, cảnh báo và chặn build.

### 4. `Bootstrapper` (Core, runtime, tự chạy — không cần đặt GameObject)
`[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]`, thứ tự:
1. Load `IntegrationSDKConfig`.
2. Nếu bật Consent: hiện Google UMP form (nếu áp dụng theo vùng) rồi request ATT (iOS)
   — phần request UMP/ATT thực thi qua `IAdProvider` đang đăng ký (module Ads.AppLovin).
3. Init ad provider đang đăng ký (module Ads.AppLovin → AppLovin MAX SDK).
4. Init các backend tracking đang đăng ký và đang bật (AppsFlyer/Firebase).

### 5. `IAdProvider` (interface, Core) + `AdManager` (facade, Core)
`AdManager` API cho game code: `ShowInterstitial(string placement)`,
`ShowRewarded(string placement, Action onReward)`, `ShowAppOpen(string placement)`.
`AdManager` tự forward các lời gọi này tới `IAdProvider` đã được module Ads.AppLovin
đăng ký; nếu chưa có provider nào đăng ký (module chưa cài), log warning và no-op.

`MaxAdProvider` (module Ads.AppLovin) tự lo load/cache/retry ẩn sau lưng game code,
và tự động bắn event theo `Tracking.md` tại đúng thời điểm (game code không tự gọi):
- Gọi `Show...` → bắn `show_interstitial_ads` / `show_rewarded_ads` / `show_aoa_ads`
  với `placement`.
- MAX báo hiển thị (`OnAdDisplayedEvent`) → bắn `..._success`.
- MAX báo click (`OnAdClickedEvent`) → bắn `..._click`.
- MAX báo trả revenue (`OnAdRevenuePaidEvent`, mọi format) → bắn `ad_impression` với
  `ad_platform, ad_source, ad_unit_name, currency, value, placement, country_code, ad_format`
  lấy trực tiếp từ dữ liệu MAX trả về.

### 6. `TrackingManager` + `ITrackingBackend` (interface, Core)
- `ITrackingBackend.LogEvent(string name, Dictionary<string,object> parameters)` và
  `LogAdRevenue(AdRevenueData data)`.
- `AppsFlyerBackend` (module Tracking.AppsFlyer), `FirebaseBackend` (module
  Tracking.Firebase) implement interface, map generic params sang API riêng
  (`AppsFlyer.sendEvent`, `FirebaseAnalytics.LogEvent`, hàm log ad revenue riêng cho
  `ad_impression`).
- `TrackingManager.LogEvent(name, params)` là API duy nhất cho game code (event tùy ý
  ngoài ads/level); fan-out tới mọi backend đã đăng ký và đang bật, game code không
  cần biết SDK phía sau là gì.
- Lỗi/SDK chưa init: adapter log warning và bỏ qua, không crash game.

### 7. `LevelTracker` (Core)
`LevelTracker.Start(level)` lưu timestamp; `.Passed(level)` / `.Failed(level)` tự tính
`time_played` và gọi `TrackingManager.LogEvent` với đúng tên event `level_start` /
`level_passed` / `level_failed` trong `Tracking.md`.

## Error handling
- Backend/adapter lỗi hoặc SDK chưa init: log warning, không throw, không chặn luồng
  game.
- Module chưa được cài (chưa có provider/backend nào đăng ký): `AdManager`/
  `TrackingManager` log warning 1 lần và no-op, không crash.
- Build validator chặn build nếu thiếu config bắt buộc cho backend/provider đang bật.
- Ad load fail: `MaxAdProvider` tự retry theo backoff đơn giản, không bắn event
  thành công.

## Testing
- EditMode test cho `TrackingManager`/`AdManager` (Core) với `ITrackingBackend`/
  `IAdProvider` mock: kiểm tra fan-out đúng backend/provider đã đăng ký, mapping tên
  event/tham số đúng `Tracking.md`, và hành vi no-op khi chưa có gì đăng ký.
- EditMode test cho `LevelTracker`: `time_played` tính đúng khoảng thời gian.
- Gọi SDK thật (MAX/AppsFlyer/Firebase) chỉ kiểm chứng được qua build thật trên
  device/emulator — ghi rõ trong README, không nằm trong test tự động.

## Câu hỏi còn mở (không chặn implementation)
- Tên chính xác của package (`com.gemmob.integrationsdk.*` là tạm đặt, có thể đổi).
- Repo Git host cho package (GitHub org nào, public/private).
