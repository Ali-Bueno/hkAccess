# 🦋 Hollow Knight Accessibility Mod

## 🎯 Objective
Develop a full accessibility layer for *Hollow Knight* focused on **audio-based navigation**, **tactile feedback**, and **screen reader integration**, allowing blind or low-vision players to explore and complete the game.

**GitHub Repository:** https://github.com/Ali-Bueno/hkAccess

---

## 🧩 Architecture Overview
- **Engine:** Unity (Mono backend)
- **Mod Loader:** [BepInEx 5](https://github.com/BepInEx/BepInEx) (Mono) — used to inject custom C# assemblies
- **Language:** C# (.NET Framework 4.7.2)
- **Accessibility Bridge:** [Tolk](https://github.com/dkager/tolk) for screen-reader output (NVDA, JAWS, SAPI)
- **Screen Reader Integration:** P/Invoke bridge to Tolk.dll for real-time narration

### Local References
- Unity DLLs: `D:\games\steam\steamapps\common\Hollow Knight\hollow_knight_Data\Managed`
- Tolk DLLs: Located in references folder, must be copied to game root
- BepInEx Plugins: `D:\games\steam\steamapps\common\Hollow Knight\BepInEx\plugins`

---

## 📁 Project Structure
```
hkAccess/
├── hkAccess/src/           # Main mod source code
│   ├── Plugin.cs           # BepInEx plugin entry point
│   ├── MenuAccessibility.cs # Menu navigation and UI accessibility
│   ├── HKAccessibility.csproj
│   └── bin/Debug/net472/   # Build output (auto-copied to game)
├── references/             # Tolk and other external DLLs
├── hk code/               # Decompiled game code (excluded from build)
├── .gitignore
└── CLAUDE.md
```

---

## ✅ Implemented Features

### Menu Accessibility System
- ✅ **Screen Reader Integration:** Announces menu navigation via Tolk
- ✅ **Scene Title Announcements:** Reads menu titles when entering new scenes
- ✅ **UI Element Detection:** Supports buttons, sliders, toggles, and custom menu components
- ✅ **Custom Component Support:** Uses reflection to access Hollow Knight's custom UI:
  - `MenuOptionHorizontal` and subclasses (resolution, language, display mode)
  - `MenuAudioSlider` for volume controls
- ✅ **Value Change Monitoring:** Real-time coroutines announce slider/toggle/dropdown changes
- ✅ **Popup Detection:** Announces confirmation dialogs via Harmony patches
- ✅ **Text Formatting:** Cleans up newlines and formats announcements properly
- ✅ **Full Descriptions:** Reads complete option descriptions (e.g., VSync tooltips)
- ✅ **Save Slot Information:** Announces detailed save slot data (location, completion %, playtime, geo, Steel Soul mode)
- ✅ **Menu Navigation:** Announces menu names when entering different menus
- ✅ **Game Save Feedback:** Announces when the game is being saved and when save completes

### Technical Implementation
- **EventSystem Integration:** Tracks currently selected GameObject
- **Reflection API:** Accesses private fields in game's custom components and SaveStats data
- **Coroutine Monitoring:** Tracks value changes while UI element is focused
- **Cleanup System:** Proper disposal of coroutines and event handlers
- **Harmony Patches:** Intercepts game methods for announcements:
  - `UIManager` methods for confirmation dialogs and menu navigation
  - `SaveSlotButton.OnSelect` for save slot information
  - `GameManager.SaveGame` for save notifications

---

## 🛠️ Core Modules

### Completed
| Module | Status | Description |
|---------|---------|-------------|
| **Interface Narrator** | ✅ Implemented | Reads menu texts, item names, and UI states via Tolk/NVDA |
| **Screen Reader Bridge** | ✅ Implemented | P/Invoke bindings to Tolk.dll for cross-screen-reader support |

### Planned
| Module | Status | Description |
|---------|---------|-------------|
| **Audio Navigation System** | 📋 Planned | Spatial audio cues for walls, platforms, and enemies |
| **Collision Feedback** | 📋 Planned | Audio/haptic alerts for hazards (spikes, pits, traps) |
| **Object Sonar (Echo Mode)** | 📋 Planned | Ping system to reveal nearby elements |
| **Auto-Orientation** | 📋 Planned | Automatically face nearest interactable/enemy |
| **Config Layer** | 📋 Planned | In-game accessibility settings menu |

---

## 🧠 Technical Roadmap

### Phase 1: Menu Accessibility (Completed)
- ✅ BepInEx plugin setup with .NET 4.7.2
- ✅ Tolk screen reader integration via P/Invoke
- ✅ Menu navigation detection and announcement
- ✅ Custom UI component support via reflection
- ✅ Value change monitoring with coroutines
- ✅ Popup and dialog detection
- ✅ Text formatting and cleanup

### Phase 2: In-Game Accessibility (Next)
- 🔄 Player position and movement tracking
- 🔄 Collision detection and audio feedback
- 🔄 Enemy proximity detection
- 🔄 Environmental audio cues
- 🔄 Spatial audio system (HRTF simulation)

### Phase 3: Advanced Features
- 📋 Object sonar/ping system
- 📋 Auto-orientation system
- 📋 Haptic feedback (DualSense/XInput)
- 📋 Configuration menu integration

---

## 🚀 Long-Term Goals
- Port system to *Silksong* if compatible
- Add vibration feedback (DualSense/XInput)
- Community translation and voice packs
- Open-source documentation for future accessible Unity mods
- Publish on modding platforms (Nexus, Thunderstore)

---

## 🔨 Development Setup

### Prerequisites
- .NET Framework 4.7.2
- BepInEx 5 (Mono) installed in Hollow Knight directory
- Tolk.dll and nvdaControllerClient64.dll in game root
- Visual Studio or VS Code with C# support

### Building
```bash
cd hkAccess/src
dotnet build
```
Build automatically copies `HKAccessibility.dll` to BepInEx plugins folder.

### Testing
1. Close Hollow Knight if running
2. Build the project (mod DLL auto-copied)
3. Launch Hollow Knight
4. Check BepInEx log for initialization messages

### Git Repository
```bash
git remote add origin git@github.com:Ali-Bueno/hkAccess.git
git branch -M main
git push -u origin main
```

---

## 📝 Notes
- Decompiled game code in `hk code/` is excluded from builds via .csproj
- Tolk DLLs must be in game root directory (not plugins folder)
- Unity DLL references point to local Hollow Knight installation
- Screen reader must be running before launching game

## 🐛 Known Issues

### Confirmation Popups Not Being Announced
**Status:** ❌ Under investigation

**Findings from decompiled code:**
- `UIManager.cs` handles all menu prompts through `MenuScreen` objects:
  - `quitGamePrompt` (UIManager.cs:212) - Quit game confirmation
  - `returnMainMenuPrompt` (UIManager.cs:214) - Return to main menu confirmation
  - `resolutionPrompt` (UIManager.cs:216) - Resolution change confirmation
- Prompts are shown via coroutines: `GoToQuitGamePrompt()`, `GoToReturnMenuPrompt()`, `GoToResolutionPrompt()`
- The `activePrompt` field (UIManager.cs:232) tracks which prompt is currently displayed
- `MenuScreen` class contains title, content, controls as CanvasGroups

**Connect Controller Panel:**
- `ConnectControllerPanel.cs` controls visibility based on `Platform.Current.IsPausingOnControllerDisconnected`
- Uses a `CanvasGroup` with fadeRate to show/hide
- No special sortingOrder, just alpha fade

**Current mod approach:**
- Only uses `MenuAccessibility.cs` component attached to Plugin
- No Harmony patches to hook into game methods
- Relies on Unity EventSystem to detect selected GameObjects
- Detects popups by Canvas sortingOrder >= 200

**Solution implemented:**
✅ **Using Harmony patches** to intercept UIManager methods:
- Created `UIManagerPatches.cs` with patches for:
  - `UIShowQuitGamePrompt()` - Intercepts quit game confirmation
  - `UIShowReturnMenuPrompt()` - Intercepts return to menu confirmation
  - `UIShowResolutionPrompt()` - Intercepts resolution change confirmation
- Each patch waits 0.5s for the popup to appear, then reads all text and announces it
- Announces both the question and available button options
- Added `Assembly-CSharp.dll` reference to .csproj to access game classes

### "Connect Controller" Text Still Showing
**Status:** ✅ Fixed
- Added explicit filter in `CheckForPopups()` to skip canvases with "connect" and "controller" in their name
- Simplified text filtering to exclude any text containing "Conecta" or "connect"

