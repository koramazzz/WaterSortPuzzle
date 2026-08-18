# Water Sort Puzzle

A work-in-progress Water Sort Puzzle game built with Unity 6 and C#.

## Status

**In Development**

The core gameplay, level system, progression, persistence, UI feedback, and automated EditMode tests are currently implemented. Additional content and polish are still in progress.

## Features

- Bottle selection and liquid-pouring mechanics
- Validation of legal moves and completed bottles
- JSON-based level loading and validation
- Multiple level difficulties and hidden liquid support
- Persistent level progress using PlayerPrefs
- Gold and life systems with timed life refills
- Responsive bottle grid layout
- UI feedback and bottle animations with DOTween
- Custom Unity editor tools for level progress and player resources
- EditMode tests for gameplay, level, progression, UI, and animation systems

## Architecture

The project separates game logic from Unity presentation code:

- **Models** contain gameplay and progression state
- **Services** handle game rules, persistence, validation, and rewards
- **Presentation** connects the domain logic to Unity views and scene controllers
- **Editor tools** support development and testing workflows

## Technology

- Unity 6000.3.21f1
- C#
- Unity Test Framework
- DOTween
- JSON level data
- PlayerPrefs persistence

## Project Structure

```text
Assets/_Project/
├── Editor/
├── Prefabs/
├── Scenes/
├── Scripts/
│   ├── Animations/
│   ├── Bottles/
│   ├── Levels/
│   ├── MainMenu/
│   └── Progress/
├── Tests/
└── ThirdPartyAssets/
```

## Getting Started

1. Clone the repository.
2. Open the project with Unity 6000.3.21f1.
3. Open `Assets/_Project/Scenes/MainScene.unity`.
4. Enter Play Mode.

## Tests

Open the Unity Test Runner and run the EditMode test suite.

## Gameplay Video

A gameplay video will be added as development progresses.
