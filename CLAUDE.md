# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity 6 (6000.2.6f2) project using the Universal Render Pipeline (URP). The project is configured with the New Input System and includes multiplayer networking support through Unity's Multiplayer Center package.

## Key Technologies

- **Unity Version**: 6000.2.6f2
- **Render Pipeline**: Universal Render Pipeline (URP) 17.2.0
- **Input System**: New Input System (1.14.2) - configured in `Assets/InputSystem_Actions.inputactions`
- **API Compatibility Level**: .NET Standard 2.1
- **Scripting Backend**: Mono (IL2CPP for Android builds)

## Essential Commands

Unity projects are managed through the Unity Editor, not command-line build tools. However, the following are useful:

### Opening the Project
- Open Unity Hub and select this project directory
- Unity will load using version 6000.2.6f2

### Building the Project
- Use Unity Editor: File > Build Settings > Build
- For automated builds, use Unity command line:
  ```
  "C:\Program Files\Unity\Hub\Editor\6000.2.6f2\Editor\Unity.exe" -quit -batchmode -projectPath "C:\DEVELOPMENT\Systems" -buildWindows64Player "Build/Systems.exe"
  ```

### Running Tests
- Unity Editor: Window > General > Test Runner
- Command line: Add `-runTests` and `-testResults` flags to Unity command

## Project Structure

### Core Directories

- **Assets/**: All game assets, scripts, scenes, and settings
  - **Scenes/**: Unity scene files (currently contains `SampleScene.unity`)
  - **Settings/**: URP rendering settings and profiles
    - `PC_RPAsset.asset` / `PC_Renderer.asset`: PC rendering configuration
    - `Mobile_RPAsset.asset` / `Mobile_Renderer.asset`: Mobile rendering configuration
    - Volume profiles for post-processing effects
  - **TutorialInfo/**: Template files (can be removed for production)

- **ProjectSettings/**: Unity project configuration
  - `ProjectSettings.asset`: Main project settings (build targets, player settings)
  - `InputManager.asset`: Legacy input configuration
  - `QualitySettings.asset`: Graphics quality presets
  - URP-specific settings in `URPProjectSettings.asset`

- **Packages/**: Package dependencies managed via Package Manager
  - `manifest.json`: Lists all package dependencies

- **Library/**: Unity-generated cache (gitignored, regenerated on project load)

### Solution Structure

The project generates two C# assemblies:
- **Assembly-CSharp**: Runtime game scripts
- **Assembly-CSharp-Editor**: Editor-only scripts

## Input System

The project uses the New Input System with predefined actions in `Assets/InputSystem_Actions.inputactions`:

**Player Action Map**:
- **Move**: Vector2 input for character movement
- **Look**: Vector2 input for camera control
- **Attack**: Button for attack actions
- **Interact**: Hold button interaction
- **Crouch**: Toggle crouch state

To regenerate the C# input wrapper:
1. Select `InputSystem_Actions.inputactions` in Unity
2. Enable "Generate C# Class" in the Inspector
3. Set the class name and namespace as needed

## Architecture Notes

### Render Pipeline Configuration

The project uses URP with separate configurations for PC and mobile:
- PC configuration prioritizes visual quality
- Mobile configuration optimizes for performance
- Both are located in `Assets/Settings/`

Switch between render pipelines in Edit > Project Settings > Graphics.

### Package Dependencies

Key packages installed:
- **AI Navigation (2.0.9)**: NavMesh pathfinding
- **Input System (1.14.2)**: New input handling
- **Multiplayer Center (1.0.0)**: Multiplayer project setup tools
- **Visual Scripting (1.9.7)**: Node-based scripting
- **Timeline (1.8.9)**: Cinematic sequencing

## C# Scripting Guidelines

### Unity-Specific Patterns

- Use `MonoBehaviour` for game objects attached to Unity GameObjects
- Use `ScriptableObject` for data containers (see `Assets/TutorialInfo/Scripts/Readme.cs`)
- Lifecycle methods: `Awake()`, `Start()`, `Update()`, `FixedUpdate()`, `LateUpdate()`
- Coroutines for time-based operations: `StartCoroutine()`, `yield return`

### Input System Usage

Access input actions via the generated C# class:
```csharp
var actions = new InputSystem_Actions();
actions.Player.Move.performed += ctx => HandleMove(ctx.ReadValue<Vector2>());
actions.Enable();
```

### URP Considerations

- Custom shaders should use URP shader graph or URP-compatible shader code
- Post-processing effects are configured via Volume Profiles in `Assets/Settings/`
- Use `UniversalRenderPipelineAsset` for global render settings

## Building and Deployment

### Build Process

1. Configure build settings: File > Build Settings
2. Select target platform (PC, Android, WebGL, etc.)
3. Add scenes to build (currently only SampleScene)
4. Click "Build" or "Build And Run"

### Platform Notes

- **Standalone (PC)**: Default configuration, no additional setup needed
- **Android**: Uses IL2CPP scripting backend, requires Android SDK/NDK
- **WebGL**: Requires WebGL support in Unity Hub installation

## Common Workflows

### Adding New Scripts

1. Right-click in Project window > Create > C# Script
2. Scripts are automatically added to Assembly-CSharp
3. For editor scripts, place in an `Editor/` folder

### Creating Prefabs

1. Create GameObject in scene with desired components
2. Drag from Hierarchy to Project window to save as prefab
3. Edit via Prefab mode (double-click prefab asset)

### Scene Management

- Main scene: `Assets/Scenes/SampleScene.unity`
- Add new scenes via File > New Scene
- Register scenes in Build Settings for runtime loading
