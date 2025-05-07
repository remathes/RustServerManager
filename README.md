# RustServerManager - Development Notes

## ✅ Completed Features
- Add new server instances via dialog (including MySQL insert).
- Load all instances from MySQL on app start.
- Per-instance metrics with LiveCharts2 (CPU %, RAM usage, gauges).
- ToggleButtons for AutoStart and AutoUpdate with binding.
- Server startup launches console and begins metrics polling.
- RustUpdate.exe integration for updates on server start (basic).

---

## 🔧 In Progress / To-Do

### 🛠 AutoUpdate Integration
- [ ] On server start, if `AutoUpdate == true`, run `RustUpdate.exe` silently.
- [ ] If `AutoUpdate == false`, show normal `RustUpdate` UI.
- [ ] After update completes, disable the "Update Server" and "Update Oxide" buttons.
- [ ] Display a full-page overlay while `RustUpdate.exe` is running to lock UI.

---

### ⚙ Port Assignment Logic
- [x] Pull last-used ports from MySQL.
- [ ] If default ports (28015–28017) are unused, reassign them to new instance.
- [ ] Show newly assigned ports on the New Instance form before install.

---

### 🔄 AutoStart / AutoUpdate Binding
- [x] Toggle switches reflect current state.
- [ ] Update MySQL `AutoStart` / `AutoUpdate` values when toggled.
- [ ] Persist toggle changes without needing to restart app.

---

### 🧼 UI/UX Improvements
- [x] Instance card now shows banner, status dot, and basic info.
- [ ] Refine layout so toggles/text don’t squish or overlap.
- [ ] Add visual feedback when server is updating (progress, overlay).
- [ ] MapName is auto-filled after first server run and saved to DB.

---

### 🐞 Minor Fixes & Enhancements
- [x] Safe parsing of `MapName` from `.map` filename.
- [ ] Show validation warnings when installing without SteamCMD / Install path.
- [ ] Add logging/error handling to `RustUpdate.exe` and instance installer.

---

## 🧠 Notes
- Data is stored per-instance in MySQL (not yet using a separate local cache).
- Most configuration is driven through the `RustInstanceEditViewModel`.
- Metrics gathering uses PerformanceCounter, WorkingSet64, and basic Process matching.
- Multiple servers are displayed using card-style UI + navigation arrows.

---