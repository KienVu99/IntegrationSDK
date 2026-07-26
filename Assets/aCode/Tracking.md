# Hướng dẫn Event Tracking (Đề xuất)

> **Version**: 1.1  
> **Platform**: Firebase Analytics & AppsFlyer (thông qua `GemAPI`)

Tài liệu này chuẩn hóa tên các sự kiện (Events), tham số (Parameters) và cách triển khai code tracking quảng cáo cũng như tiến trình chơi game (Level) trong dự án.

---

## 📢 Ad Events (Theo dõi Quảng cáo)

### 1. ad_impression (Ghi nhận doanh thu quảng cáo - Tự động)
*Sự kiện này được SDK tự động gửi lên Firebase khi có ad impression từ Admob hoặc Applovin MAX.*
* **Parameters**: `ad_platform`, `ad_source`, `ad_unit_name`, `currency`, `value`, `placement`, `country_code`, `ad_format`

---

### 2. Interstitial Ads (Quảng cáo xen kẽ)

#### **show_interstitial_ads** (Yêu cầu hiển thị)
* **Parameter**: `placement` (Vị trí/Thời điểm gọi hiển thị)
* **C# Example**:
  ```csharp
  GM.LogEvent("show_interstitial_ads", "placement", "next_lv1");
  ```

#### **show_interstitial_ads_suscces** (Hiển thị thành công - Giữ nguyên chính tả cũ)
* **Parameter**: `placement`
* **C# Example**:
  ```csharp
  GM.LogEvent("show_interstitial_ads_suscces", "placement", "next_lv1");
  ```

#### **show_interstitial_ads_click** (Người dùng click quảng cáo)
* **Parameter**: `placement`
* **C# Example**:
  ```csharp
  GM.LogEvent("show_interstitial_ads_click", "placement", "next_lv1");
  ```

---

### 3. Rewarded Video Ads (Quảng cáo nhận thưởng)

#### **show_rewarded_ads** (Yêu cầu hiển thị)
* **Parameter**: `placement` (Nút bấm/Tính năng nhận thưởng)
* **C# Example**:
  ```csharp
  GM.LogEvent("show_rewarded_ads", "placement", "get_jetpack_button_lv1");
  ```

#### **show_rewarded_ads_sucess** (Xem hết quảng cáo / Nhận thưởng thành công - Giữ nguyên chính tả cũ)
* **Parameter**: `placement`
* **C# Example**:
  ```csharp
  GM.LogEvent("show_rewarded_ads_sucess", "placement", "get_jetpack_button_lv1");
  ```

#### **show_rewarded_ads_click** (Người dùng click quảng cáo)
* **Parameter**: `placement`
* **C# Example**:
  ```csharp
  GM.LogEvent("show_rewarded_ads_click", "placement", "get_jetpack_button_lv1");
  ```

---

### 4. App Open Ads - AOA (Quảng cáo lúc mở game)

#### **show_aoa_ads** (Yêu cầu hiển thị)
* **Parameter**: `placement`
* **C# Example**:
  ```csharp
  GM.LogEvent("show_aoa_ads", "placement", "loading_home");
  ```

#### **show_aoa_ads_sucess** (Hiển thị AOA thành công - Giữ nguyên chính tả cũ)
* **Parameter**: `placement`
* **C# Example**:
  ```csharp
  GM.LogEvent("show_aoa_ads_sucess", "placement", "loading_home");
  ```

#### **show_aoa_ads_click** (Người dùng click quảng cáo AOA)
* **Parameter**: `placement`
* **C# Example**:
  ```csharp
  GM.LogEvent("show_aoa_ads_click", "placement", "loading_home");
  ```

---

## 🎮 Level Events (Theo dõi Tiến trình chơi)

Hỗ trợ đánh giá độ khó của màn chơi, hành vi người chơi và thời gian hoàn thành màn.

### 1. level_start (Bắt đầu màn)
*Gọi ngay khi màn chơi được load xong và người chơi bắt đầu điều khiển.*
* **Parameters**: 
  - `level` (Tên/ID của màn chơi, ví dụ: `"level_01"`)
* **C# Example**:
  ```csharp
  GM.LogEvent("level_start", "level", "level_01");
  // Hoặc dùng helper: GM.LevelStart(1);
  ```

### 2. level_passed (Vượt qua màn thành công)
*Gọi khi người chơi hoàn thành mục tiêu màn chơi và thắng cuộc.*
* **Parameters**:
  - `level` (Tên/ID của màn chơi)
  - `time_played` (Thời gian chơi thực tế tính bằng giây từ lúc bắt đầu màn)
* **C# Example**:
  ```csharp
  GM.LogEvent("level_passed", "level", "level_01", "time_played", GM.ElapsedTime());
  // Hoặc dùng helper: GM.LevelComplete(1, GM.ElapsedTime());
  ```

### 3. level_failed (Thua cuộc / Thất bại)
*Gọi khi người chơi chết hoặc hết thời gian và thất bại màn chơi.*
* **Parameters**:
  - `level` (Tên/ID của màn chơi)
  - `time_played` (Thời gian đã chơi trước khi thua)
* **C# Example**:
  ```csharp
  GM.LogEvent("level_failed", "level", "level_01", "time_played", GM.ElapsedTime());
  // Hoặc dùng helper: GM.LevelFail(1, GM.ElapsedTime());
  ```