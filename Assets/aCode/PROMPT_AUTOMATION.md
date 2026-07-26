# PROMPT AUTOMATION TÍCH HỢP HỆ THỐNG TRACKING & BUILD ACODE

File này chứa **One-Shot Automation Prompt** giúp tự động hóa quá trình quét mã nguồn game, refactor các event tracking cũ sang chuẩn `Tracking.md`, và tự động cấu hình Android Build khi mang thư viện `aCode` sang bất kỳ dự án Unity nào khác.

---

## 📋 Prompt Copy-Paste Dành Cho AI Agent

```markdown
Hãy thực hiện quét toàn bộ dự án và tự động refactor tracking + cấu hình build theo các bước sau:

1. QUÉT & THAY THẾ TRACKING CŨ TRONG MÃ NGUỒN GAME:
   - Đọc file quy chuẩn `Assets/aCode/Tracking.md`.
   - Tìm kiếm toàn bộ các nơi gọi tracking trong mã nguồn dự án (bao gồm `Assets/Scripts`, `Assets/aCode`, `FirebaseAnalytics.LogEvent`, `GM.LogEvent`, `GM.LevelStart`, `GM.LevelComplete`, `GM.LevelFail`, `GM.ShowInterstitial`, `GM.ShowRewarded`...).
   - Refactor và thay thế tất cả các lệnh gọi event cũ/không đúng tên sang hàm chuẩn mới:
     + Event Level: `level_start` (`level`), `level_passed` (`level`, `time_played`), `level_failed` (`level`, `time_played`).
     + Event Quảng cáo: `show_interstitial_ads`, `show_rewarded_ads`, `show_aoa_ads` truyền kèm `placement`.
     + Giữ nguyên chính tả các event đặc thù: `show_interstitial_ads_suscces`, `show_rewarded_ads_sucess`, `show_aoa_ads_sucess`.

2. CẤU HÌNH ANDROID BUILD & FIX LỖI:
   - Kiểm tra file `Assets/aCode/aCode.asmdef`, thêm tham chiếu AppsFlyer (`GUID:2a37df438292d4903b4e5159c5de3bf9`) nếu chưa có.
   - Cập nhật file `Assets/Plugins/Android/gradleTemplate.properties`, bổ sung:
     `android.jetifier.ignorelist=af-android-sdk,appsflyer`
     `android.suppressUnsupportedCompileSdk=36`
   - Cập nhật file `Assets/Plugins/Android/proguard-user.txt`, bổ sung:
     `-keep class com.appsflyer.** { *; }`
     `-dontwarn com.appsflyer.**`

3. VERIFY & REPORT:
   - Kiểm tra Unity Console đảm bảo không có lỗi biên dịch (0 compilation errors).
   - Liệt kê danh sách các file game đã được quét và cập nhật tracking.
```

---

## 💡 Hướng dẫn sử dụng khi sang dự án mới
1. Import file **`aCode_IntegrationSDK.unitypackage`** (hoặc copy thư mục `Assets/aCode`) vào dự án Unity mới.
2. Copy toàn bộ đoạn Prompt ở trên và dán vào chat cho AI Assistant (Gemini / Claude / ChatGPT).
3. AI Agent sẽ tự động quét toàn bộ code trong dự án mới, tự sửa các event tracking và cấu hình Android build hoàn chỉnh.
