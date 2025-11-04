# 🦋 Hollow Knight Accessibility Mod

A comprehensive accessibility mod for *Hollow Knight* that adds screen reader support and audio-based navigation to make the game playable for blind and low-vision players.

## ✨ Features

### Currently Implemented
- 🎙️ **Screen Reader Integration**: Full menu navigation support via NVDA, JAWS, and SAPI
- 📢 **Menu Announcements**: Automatically reads menu titles, buttons, and options
- 🎚️ **UI Control Support**: Announces sliders, toggles, dropdowns with real-time value changes
- 💬 **Dialog Detection**: Reads popup messages and confirmation dialogs
- 🌍 **Multi-language Support**: Works with all in-game languages

## 📦 Installation

### Prerequisites
- Hollow Knight (Steam version recommended)
- [BepInEx 5](https://github.com/BepInEx/BepInEx/releases) (Mono version)
- A screen reader (NVDA, JAWS, or Windows Narrator)

### Installation Steps
1. Install BepInEx 5 (Mono) in your Hollow Knight directory
2. Download the latest release from [Releases](https://github.com/Ali-Bueno/hkAccess/releases)
3. Extract `HKAccessibility.dll` to `BepInEx/plugins/`
4. Extract `Tolk.dll` and `nvdaControllerClient64.dll` to the Hollow Knight root directory (same folder as `hollow_knight.exe`)
5. Launch the game with your screen reader running

## 🎮 Usage

Once installed, the mod will automatically:
- Announce when the mod loads successfully
- Read menu titles when you enter a menu
- Announce selected UI elements as you navigate with arrow keys or controller
- Read value changes when adjusting sliders or toggling options
- Announce popup dialogs and confirmation messages

## 🛠️ Building from Source

### Prerequisites
- .NET Framework 4.7.2 SDK
- Hollow Knight with BepInEx 5 installed

### Build Instructions
```bash
cd hkAccess/src
dotnet build
```

The build will automatically copy the compiled DLL to your BepInEx plugins folder (if configured correctly in the `.csproj` file).

## 🗺️ Roadmap

### Phase 1: Menu Accessibility ✅ (Completed)
- ✅ Screen reader integration
- ✅ Menu navigation
- ✅ UI control announcements
- ✅ Popup detection

### Phase 2: In-Game Accessibility 🔄 (In Progress)
- 🔄 Player position and movement tracking
- 🔄 Enemy proximity detection
- 🔄 Collision and hazard warnings
- 🔄 Environmental audio cues

### Phase 3: Advanced Features 📋 (Planned)
- 📋 Object sonar/ping system
- 📋 Auto-orientation toward enemies/objectives
- 📋 Haptic feedback support
- 📋 In-game configuration menu

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

### Development Setup
1. Clone the repository
2. Ensure Unity DLL references point to your Hollow Knight installation
3. Build and test changes
4. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- [BepInEx](https://github.com/BepInEx/BepInEx) - Modding framework
- [Tolk](https://github.com/dkager/tolk) - Screen reader library
- Team Cherry - For creating Hollow Knight
- The accessibility gaming community

## 📧 Contact

For bug reports, feature requests, or questions, please [open an issue](https://github.com/Ali-Bueno/hkAccess/issues).

---

**Note**: This is a work in progress. Currently, only menu accessibility is implemented. In-game accessibility features are planned for future releases.
