# 🦋 Hollow Knight Accessibility Mod

## 🎯 Objective
Develop a full accessibility layer for *Hollow Knight* focused on **audio-based navigation**, **tactile feedback**, and **screen reader integration**, allowing blind or low-vision players to explore and complete the game.

**GitHub Repository:** https://github.com/Ali-Bueno/hkAccess

## ⚠️ IMPORTANT DEVELOPMENT NOTES
- **DO NOT push to GitHub unless explicitly requested by the user**
- Wait for user confirmation before publishing changes
- Keep changes local until approval

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
- ✅ **Controller Disconnect Filter:** Completely filters out "connect controller" messages from all announcement systems
- ✅ **In-Game Dialogue Reading:** Automatically announces NPC dialogues and conversation text using TextMeshPro extraction

### Cutscene Accessibility System
- ✅ **Real-Time Text Monitoring:** Continuous monitoring of cutscene text elements as they appear
- ✅ **UIState Detection:** Automatically detects when entering/exiting cutscene mode (`UIState.CUTSCENE`)
- ✅ **Alpha-Based Visibility:** Announces texts only when they become visible (alpha > 0.1f), respecting animation timing
- ✅ **Complete Text Capture:** Captures ALL TextMeshPro components in cutscenes:
  - Main body text (excerpt content)
  - Title/header text (e.g., "DE «ELEGÍA PARA HALLOWNEST»")
  - Author attribution (e.g., "POR MONOMON, LA MAESTRA")
- ✅ **Duplicate Prevention:** HashSet tracking ensures each text is announced only once
- ✅ **Interrupt Mode:** Uses interrupt flag to prevent text overlap during rapid sequences

### Technical Implementation
- **EventSystem Integration:** Tracks currently selected GameObject
- **Reflection API:** Accesses private fields in game's custom components and SaveStats data
- **Coroutine Monitoring:** Tracks value changes while UI element is focused
- **Cleanup System:** Proper disposal of coroutines and event handlers
- **Harmony Patches:** Intercepts game methods for announcements:
  - `UIManager.SetState` (Postfix) for cutscene state detection
  - `UIManager` methods for confirmation dialogs and menu navigation
  - `SaveSlotButton.OnSelect` for save slot information
  - `GameManager.SaveGame` for save notifications
  - `DialogueBox.ShowPage` for in-game dialogue announcements
- **TextMeshPro Integration:** Extracts text from TextMeshPro components with page-based reading
- **Cutscene Monitoring System:**
  - Continuous coroutine runs while `UIState == CUTSCENE`
  - Scans all TextMeshPro components every 0.1 seconds
  - Tracks visibility via alpha channel
  - Maintains HashSet of announced texts to prevent duplicates

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

### Phase 1: Menu & Cutscene Accessibility (Completed)
- ✅ BepInEx plugin setup with .NET 4.7.2
- ✅ Tolk screen reader integration via P/Invoke
- ✅ Menu navigation detection and announcement
- ✅ Custom UI component support via reflection
- ✅ Value change monitoring with coroutines
- ✅ Popup and dialog detection
- ✅ Text formatting and cleanup
- ✅ Cutscene text monitoring with real-time visibility detection
- ✅ Complete cutscene text capture (body, title, author)

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

## 🐛 Previously Known Issues (Now Fixed)

### ✅ Confirmation Popups
**Implemented Solution:**
- Harmony patches intercept UIManager methods (`UIShowQuitGamePrompt`, `UIShowReturnMenuPrompt`, `UIShowResolutionPrompt`)
- Filters out button text and technical names (like "QuitGamePrompt")
- Only announces the actual popup message (e.g., "¿Salir al menú? Se guardará el progreso")
- Added `Assembly-CSharp.dll` reference to access game classes

### ✅ Save Slot Information
**Implemented Solution:**
- Patch `SaveSlotButton.OnSelect` to announce detailed save information
- Uses reflection to access private `saveStats` field
- Announces: location, completion %, playtime, geo, Steel Soul mode
- Includes 0.15s delay to avoid conflicts with MenuAccessibility

### ✅ Connect Controller Panel
**Implemented Solution:**
- Filters ConnectControllerPanel by GameObject name
- Excludes any text containing "Conecta" or "connect"

