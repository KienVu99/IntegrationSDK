# Event tracking đề xuất
Version: 1.0

### Ad Events
- **ad_impression**
  - Parameter: `ad_platform`, `ad_source`, `ad_unit_name`, `currency`, `value`, `placement`, `country_code`, `ad_format`
  - Meaning: Giá trị của 1 quảng cáo

- **show_interstitial_ads**
  - Parameter: `placement`
  - Ví dụ:
    - show_interstitial_ads: "replay_lv1"
    - show_interstitial_ads: "next_lv1"

- **show_interstitial_ads_suscces**
  - Parameter: `placement`
  - Ví dụ:
    - show_interstitial_ads: "replay_lv1"
    - show_interstitial_ads: "next_lv1"

- **show_interstitial_ads_click**
  - Parameter: `placement`
  - Ví dụ:
    - show_interstitial_ads: "replay_lv1"
    - show_interstitial_ads: "next_lv1"

- **show_rewarded_ads**
  - Parameter: `placement`
  - Ví dụ:
    - show_rewarded_ads: "get_jetpack_button_lv1"
    - show_rewarded_ads: "use_booster_doublejump_lv1"

- **show_rewarded_ads_sucess**
  - Parameter: `placement`
  - Ví dụ:
    - show_rewarded_ads: "get_jetpack_button_lv1"
    - show_rewarded_ads: "use_booster_doublejump_lv1"

- **show_rewarded_ads_click**
  - Parameter: `placement`
  - Ví dụ:
    - show_rewarded_ads: "get_jetpack_button_lv1"
    - show_rewarded_ads: "use_booster_doublejump_lv1"

- **show_aoa_ads**
  - Parameter: `placement`
  - Ví dụ:
    - show_aoa_ads: "loading_home"
    - show_aoa_ads: "back_lv1"

- **show_aoa_ads_sucess**
  - Parameter: `placement`
  - Ví dụ:
    - show_aoa_ads: "loading_home"
    - show_aoa_ads: "back_lv1"

- **show_aoa_ads_click**
  - Parameter: `placement`
  - Ví dụ:
    - show_aoa_ads: "loading_home"
    - show_aoa_ads: "back_lv1"

### Level Events
- **level_start**: `level`, `time_played`
- **level_passed**: `level`, `time_played`
- **level_failed**: `level`, `time_played`