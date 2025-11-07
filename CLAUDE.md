# 🦋 Hollow Knight Accessibility Mod

## 🎯 Objective
Develop a full accessibility layer for *Hollow Knight* focused on **audio-based navigation**, **tactile feedback**, and **screen reader integration**, allowing blind or low-vision players to explore and complete the game.

**GitHub Repository:** https://github.com/Ali-Bueno/hkAccess

## ⚠️ IMPORTANT DEVELOPMENT NOTES
- **DO NOT push to GitHub unless explicitly requested by the user**
- Wait for user confirmation before publishing changes
- Keep changes local until approval

---

## 🎯 Development Methodology

### Code-First Approach
**ALWAYS analyze the decompiled game code before implementing any accessibility feature.**

#### Mandatory Process:
1. **Check Existing Code First** - BEFORE implementing any new feature:
   - Review existing patches in `UIManagerPatches.cs`, `MenuAccessibility.cs`, and `Plugin.cs`
   - Look for similar patterns, helper methods, or existing approaches
   - Reuse existing helper functions (e.g., `GetGameObjectPath`, text filtering logic)
   - Ensure consistency with existing code style and patterns
   - **Never duplicate code or implement worse versions of existing solutions**

2. **Research Game Code** - When implementing any new accessibility feature:
   - Search through `hk code/` directory for relevant game classes and methods
   - Study how the game implements the functionality you want to make accessible
   - Identify the exact methods, properties, and state changes involved

3. **Harmony Patching** - Use Harmony to intercept game methods:
   - Prefer patching existing game methods over generic Unity component monitoring
   - Use `[HarmonyPatch]` attributes to target specific game classes and methods
   - Choose appropriate patch types (Prefix/Postfix) based on needs

4. **Never Guess or Invent** - Do NOT implement generic solutions without understanding the game code:
   - ❌ BAD: Generic `Text.text` setter patches that catch everything
   - ✅ GOOD: Specific patches on `DialogueBox.ShowPage` after analyzing the code
   - ❌ BAD: Monitoring all GameObject activations hoping to catch something
   - ✅ GOOD: Patching `UIManager.SetState` after finding it controls cutscenes
   - ❌ BAD: Duplicating existing text filtering logic in multiple places
   - ✅ GOOD: Reusing existing helper methods and patterns

5. **Iterative Analysis** - If a feature isn't working:
   - Go back to the decompiled code
   - Search for related classes, methods, enums, and state managers
   - Ask user to review logs and provide feedback
   - Refine patches based on actual game behavior

#### Examples of Correct Methodology:

**Cutscene Text (CORRECT):**
1. **Checked existing code** - Saw we already had `GetGameObjectPath` helper and TextMeshPro monitoring patterns
2. **Searched game code** - Found `OpeningSequence.cs` uses `UIState.CUTSCENE`
3. **Analyzed further** - Found `ChangeFontByLanguage.cs` reveals `ExcerptAuthor` font type exists
4. **Implemented patch** - Patched `UIManager.SetState` to detect cutscene mode
5. **Reused patterns** - Used existing alpha-based visibility monitoring approach from dialogue system

**Save Slot Info (CORRECT):**
1. **Checked existing code** - Saw we had reflection patterns for accessing private fields
2. **Analyzed game code** - Studied `SaveSlotButton.cs` and `PresentSaveSlot` method
3. **Found data source** - Located private `saveStats` field contains all information
4. **Implemented patch** - Patched `OnSelect` method and used reflection pattern
5. **Announced consistently** - Used same announcement format as other UI elements

**Menu Option Changes (CORRECT):**
1. **Identified problem** - Generic coroutine monitoring wasn't detecting value changes
2. **Analyzed game code** - Found `MenuAudioSlider.cs` has `UpdateTextValue()` method called on changes
3. **Found pattern** - `MenuOptionHorizontal` and subclasses all call `UpdateText()` when values change
4. **Implemented patches** - Created specific Harmony patches for these exact methods
5. **Result** - Immediate announcements without polling/monitoring

**What NOT to Do (INCORRECT):**
- ❌ Creating generic text monitoring without knowing what components the game uses
- ❌ Assuming Unity standard components when game uses custom systems
- ❌ Implementing features without checking if game already has the infrastructure
- ❌ **Duplicating existing code instead of reviewing what's already implemented**
- ❌ **Implementing new patterns when similar working patterns already exist**
- ❌ **Creating new helper methods when similar ones already exist**

### Available Resources:
- **Our Mod Code:** Review `UIManagerPatches.cs`, `MenuAccessibility.cs`, `Plugin.cs` for existing patterns
- **Decompiled Code:** `D:\code\unity and such\hk access\hk code\`
  - **IMPORTANT:** The game code directory is in `.gitignore` and excluded from version control, but is fully accessible for reading and analysis
  - Claude Code can freely read and analyze all files in `hk code/` regardless of .gitignore restrictions
  - Always analyze the decompiled game code before implementing features
- **Game Assemblies:** `D:\games\steam\steamapps\common\Hollow Knight\hollow_knight_Data\Managed\`
- **Tools:** Use Grep, Task, or Explore agents to search both our code and game code efficiently

### Code Review Checklist Before Implementation:
- [ ] Have I reviewed existing mod code for similar functionality?
- [ ] Can I reuse any existing helper methods or patterns?
- [ ] Have I searched the decompiled game code for relevant classes?
- [ ] Am I using Harmony patches on specific game methods (not generic Unity monitoring)?
- [ ] Is this implementation consistent with our existing code style?

---

## 📦 Modular Architecture Principles

### CRITICAL: Keep Code Modular and Maintainable

**The codebase MUST follow a modular architecture with small, focused files.**

#### File Size Guidelines:
- **Preferred:** Files should be 50-200 lines of code
- **Maximum:** Files should NOT exceed 300-400 lines
- **When to Split:** If a file grows beyond 300 lines, split it into smaller, focused modules

#### Organization Principles:
1. **Separation of Concerns**
   - Each file should handle ONE specific responsibility
   - ✅ GOOD: `DialogueBoxPatches.cs` - Only handles dialogue announcements
   - ✅ GOOD: `MapPatches.cs` - Only handles map and area announcements
   - ❌ BAD: `AllPatches.cs` - Contains all patches in one 2000-line file

2. **Patches Organization**
   - All Harmony patches go in `Patches/` directory
   - Group related patches by **functionality**, not by game class
   - ✅ GOOD: `InventoryPatches.cs` - All inventory-related patches (charms, items, etc.)
   - ✅ GOOD: `GameStatePatches.cs` - All game state patches (save, menus, prompts)
   - ❌ BAD: `PlayerDataPatches.cs` - Contains unrelated PlayerData patches

3. **Utility Classes**
   - Create focused utility classes for specific purposes
   - ✅ GOOD: `SpokenTextHistory.cs` - Single responsibility: prevent duplicate announcements
   - ✅ GOOD: `TolkScreenReader.cs` - Single responsibility: screen reader wrapper
   - ❌ BAD: `Utilities.cs` - Mixed bag of unrelated helper functions

4. **Current Modular Structure**
   ```
   hkAccess/src/
   ├── Plugin.cs                         # Entry point (~140 lines)
   ├── TolkScreenReader.cs               # Screen reader wrapper (~130 lines)
   ├── MenuNavigationMonitor.cs          # UI selection announcements (~310 lines)
   ├── InventoryReader.cs                # Inventory text monitoring and aggregation (~593 lines)
   ├── InputManager.cs                   # Accessibility input handling (~30 lines)
   ├── Patches/
   │   ├── MenuAudioSliderPatches.cs     # Volume slider changes (~50 lines)
   │   ├── MenuOptionHorizontalPatches.cs # All horizontal options + language (~135 lines)
   │   ├── SceneTitlePatches.cs          # Scene/menu title announcements (~100 lines)
   │   ├── MenuSelectablePatches.cs      # Menu element selection (~260 lines)
   │   ├── InventoryPatches.cs           # Charm selection via Harmony (~87 lines)
   │   ├── DialogueBoxPatches.cs         # In-game dialogue (~40 lines)
   │   ├── MapPatches.cs                 # Area and map announcements (~80 lines)
   │   ├── GameStatePatches.cs           # Saves, prompts, menus (~260 lines)
   │   └── CutscenePatches.cs            # Cutscene text monitoring (~70 lines)
   ```

   **Note:** `InventoryReader.cs` has grown to 593 lines. Consider splitting if functionality expands:
   - Potential split: Separate charm monitoring from item monitoring
   - Alternative: Extract text aggregation logic into utility class

5. **When Files Grow Too Large**
   - If a patch file exceeds 300 lines, split it by sub-functionality
   - Example: `GameStatePatches.cs` could be split into:
     - `SaveSlotPatches.cs` - Save slot announcements
     - `PromptPatches.cs` - Confirmation prompts
     - `MenuNavigationPatches.cs` - Menu navigation
   - Only split when it improves clarity, not just for size

6. **Benefits of Modularity**
   - ✅ Easier to understand and review
   - ✅ Faster to locate bugs and issues
   - ✅ Simpler to test individual features
   - ✅ Reduces merge conflicts
   - ✅ Easier for new developers to contribute
   - ✅ Better code reusability

#### Anti-Patterns to Avoid:
- ❌ Giant "god" files with thousands of lines
- ❌ Multiple unrelated responsibilities in one file
- ❌ Deep nesting (more than 3 levels)
- ❌ Duplicate code across multiple files
- ❌ Circular dependencies between modules

#### Before Creating New Files:
1. Check if existing module can be extended (without exceeding size limits)
2. Ensure the new module has a clear, single responsibility
3. Choose a descriptive, specific name
4. Document the module's purpose in XML comments

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
- ✅ **Scene Title Announcements:** Reads menu titles when entering new scenes via `SceneTitlePatches.cs`
- ✅ **UI Element Detection:** Supports buttons, sliders, toggles, and custom menu components
- ✅ **Real-Time Value Changes via Harmony Patches:**
  - `MenuAudioSliderPatches.cs` - Patches `UpdateTextValue()` for volume sliders (Master, Music, Sound)
  - `MenuOptionHorizontalPatches.cs` - Patches `UpdateText()` for all horizontal options:
    - Resolution, Display Mode, Frame Cap
    - VSync (On/Off), Fullscreen mode
    - Backer Credits, Native Achievements
    - Controller Rumble
  - `MenuLanguageSettingPatches.cs` - Patches `UpdateText()` specifically for language changes
- ✅ **Initial Selection Announcements:** `MenuNavigationMonitor.cs` announces UI elements when first selected
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

### Inventory Accessibility System
- ✅ **Inventory Open/Close Detection:** Monitors inventory state via `GameManager.inventoryFSM` and "Charms Pane" tag
- ✅ **Charm Selection Announcements:**
  - Harmony patch on `InvCharmBackboard.SelectCharm` for immediate selection feedback
  - PlayMakerFSM monitoring for charm navigation
  - Localized charm names and descriptions via `Language.Language` system
  - Notch cost announcement (e.g., "Costo: 2 muescas")
  - Equipped/unequipped state indication
  - "New" charm indicator
- ✅ **Item Selection Announcements:**
  - Multi-component text monitoring (TextMeshPro, TextMeshProUGUI, tk2dTextMesh)
  - Intelligent name/description pairing via vertical proximity and text length heuristics
  - Quantity announcements for collectibles (Geo, Simple Keys, Pale Ore, Rancid Eggs)
  - Filters out headers, numbers, and controller prompts
- ✅ **Text Aggregation System:**
  - Initial snapshot on inventory open to suppress static UI labels
  - Change detection based on component instance IDs
  - 120ms debounce delay for UI text updates
  - Longest text heuristic to identify descriptions
  - Shortest nearby text for item names
- ✅ **Multiple Text Component Support:**
  - TextMeshPro (TMP) - Unity's modern text system
  - TextMeshProUGUI (Canvas-based TMP)
  - tk2dTextMesh - 2D Toolkit legacy system used in original game UI

### Technical Implementation
- **EventSystem Integration:** Tracks currently selected GameObject for initial announcements
- **Reflection API:** Accesses private fields in game's custom components and SaveStats data
- **Harmony Patches (Game-Specific Methods):** Intercepts exact game methods that handle value changes:
  - **Menu Value Changes:**
    - `MenuAudioSlider.UpdateTextValue()` - Volume slider changes
    - `MenuOptionHorizontal.UpdateText()` - All horizontal options (resolution, VSync, etc.)
    - `MenuLanguageSetting.UpdateText()` - Language changes (overrides base method)
  - **Menu Navigation:**
    - `UIManager.SetState` (Postfix) for cutscene state detection
    - `UIManager` methods for confirmation dialogs and menu navigation
    - `SaveSlotButton.OnSelect` for save slot information
  - **Inventory Navigation:**
    - `InvCharmBackboard.SelectCharm` (Postfix) for charm selection announcements
  - **Game State:**
    - `GameManager.SaveGame` for save notifications
    - `DialogueBox.ShowPage` for in-game dialogue announcements
- **TextMeshPro Integration:** Extracts text from TextMeshPro components with page-based reading
- **Cutscene Monitoring System:**
  - Continuous coroutine runs while `UIState == CUTSCENE`
  - Scans all TextMeshPro components every 0.1 seconds
  - Tracks visibility via alpha channel
  - Maintains HashSet of announced texts to prevent duplicates
- **Inventory Monitoring System:**
  - Continuous coroutine monitors inventory open state every 0.1 seconds
  - PlayMakerFSM access for charm selection state
  - Multi-component text scanning (TMP, TMPUGUI, tk2dTextMesh)
  - Instance ID tracking for change detection
  - Intelligent text aggregation with spatial proximity analysis
- **No Generic Monitoring (Where Possible):** Prefers game-specific method patches over generic Unity component monitoring

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

### Initial Setup
**Configure Hollow Knight Installation Path:**

The project uses a local configuration file to locate your Hollow Knight installation. This allows the project to work in any environment without hardcoded paths.

1. Navigate to `hkAccess/src/`
2. Copy `HollowKnight.props.example` to `HollowKnight.props`
3. Edit `HollowKnight.props` and update the path to match your installation:
   ```xml
   <HollowKnightRootPath>C:\Your\Path\To\Hollow Knight</HollowKnightRootPath>
   ```
4. The `HollowKnight.props` file is git-ignored and won't be committed

**Note:** If the file doesn't exist, the build will fall back to a relative path (assumes project is 3 levels deep in the game directory).

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
- **Game code is in .gitignore but fully accessible:** The `hk code/` directory is excluded from version control but Claude Code can freely read and analyze all files for implementation guidance
- Tolk DLLs must be in game root directory (not plugins folder)
- **Unity DLL references:** Configured via `HollowKnight.props` file (local, git-ignored)
  - Each developer can have their own installation path
  - Use `HollowKnight.props.example` as a template
  - Falls back to relative paths if config file doesn't exist
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

### ✅ Scene Change Announcements (Harmony Error)
**Problem:** SceneTitlePatches tried to patch `Internal_ActiveSceneChanged`, a Unity internal method not accessible to Harmony
**Implemented Solution:**
- Analyzed game code and found `GameManager.BeginScene()` is called when scenes start
- Changed from trying to use Unity events to patching game-specific method
- Uses `[HarmonyPatch(typeof(GameManager), nameof(GameManager.BeginScene))]`
- Eliminated Harmony error while maintaining functionality

### ✅ Menu Value Initialization Spam
**Problem:** Menu value patches announced ALL settings when entering menus (not just user changes)
**Implemented Solution:**
- Added `EventSystem.current.currentSelectedGameObject` verification
- Patches now only announce when the element is actively selected by the user
- Applied to: `MenuAudioSliderPatches`, `MenuOptionHorizontalPatches`, `MenuLanguageSettingPatches`
- Eliminates announcement spam during menu initialization

### ✅ Generic "Activado" Announcements
**Problem:** Unlabeled toggles announced generic "activado" when returning to main menu
**Implemented Solution:**
- Modified `MenuNavigationMonitor.BuildToggleDescription()` to return empty string for toggles without labels
- Prevents meaningless announcements

### ✅ Inventory Navigation
**Problem:** Initial EventSystem-based approach didn't detect inventory navigation
**Implemented Solution:**
- Created dedicated `InventoryReader.cs` module (~593 lines) with continuous monitoring
- Harmony patch on `InvCharmBackboard.SelectCharm` for immediate charm selection
- PlayMakerFSM access to read charm selection state from "UI Charms" FSM
- Multi-component text scanning (TextMeshPro, TextMeshProUGUI, tk2dTextMesh)
- Intelligent text aggregation using spatial proximity and length heuristics
- Announces charm names, descriptions, notch costs, and equipped status
- Announces item names with quantities (Geo, Keys, Ore, Eggs)
- Filters static UI labels via initial snapshot on inventory open

### ✅ Item Name Localization (Multi-Language Support)
**Problem:** Item names were hardcoded in Spanish, failing to announce correctly in other languages. Button prompts like "Press" or "Pressione" were incorrectly identified as item names.
**Implemented Solution:**
- **Enhanced Text Filtering:** Comprehensive multi-language filtering in `CleanText()` method (InventoryReader.cs:440-479)
  - Filters button prompts in 6 languages: English, Spanish, Portuguese, Italian, French, German
  - Removes "Press", "Hold", "Tap" and equivalents ("Presiona", "Pressione", "Premi", etc.)
  - Filters controller connection prompts in all languages
- **Language-Agnostic Quantity Detection:** Removed hardcoded Spanish name matching
  - Detects quantities by finding numbers near item names using spatial proximity
  - Works automatically in all supported game languages
- **Result:** Item names and quantities now announce correctly in English, Spanish, Portuguese, Italian, French, and German without code changes

### ✅ Mod Localization Strategy
**Problem:** Initial approach used hardcoded translations in 10 languages within ModLocalization.cs (~400 lines), which caused game control blocking when accessing Language.Language.CurrentLanguage() during mod initialization. Hardcoded menu names ("Opciones", "Partida guardada") didn't respect game language changes.
**Implemented Solution:**
- **Simplified ModLocalization.cs:** Reduced to 37 lines with only 3 universal English messages for mod-specific features:
  - "Hollow Knight Accessibility Mod loaded"
  - "Hollow Knight Accessibility Mod unloaded"
  - "Inventory opened"
- **No Language Detection During Init:** Completely removed all Language.Language access during mod startup to prevent input system blocking
- **Read Game UI Directly:** Menu titles, save slot information, and game terms are read directly from the game's localized UI
  - `SceneTitlePatches.cs` reads menu titles from visible Text components (automatically localized by game)
  - Save slot info uses game's own locationText, completionText, playTimeText (already localized)
  - Charm names/descriptions use Language.Language.Get() with proper keys after game is loaded
- **No Hardcoded Translations:** Eliminated all hardcoded Spanish/Portuguese translations
  - Menu names now adapt automatically when user changes game language
  - Works in all 10 game languages without code changes
- **Result:** Controls work correctly, menu titles display in user's selected language, mod messages use simple universal English

## 🚧 Known Issues (In Progress)

*No known issues at this time.*

