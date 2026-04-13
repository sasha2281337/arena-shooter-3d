# Build Notes

## Target

Recommended first export:
- Windows desktop build

## Scenes

The project expects this scene order:
1. `Assets/Scenes/MainMenu.unity`
2. `Assets/Scenes/ArenaPrototype.unity`

## Unity Build Steps

1. Open the project in Unity.
2. Open `File > Build Profiles`.
3. Make sure `Windows` is the active platform.
4. Open `Scene List` and verify:
   - `MainMenu`
   - `ArenaPrototype`
5. Click `Build`.
6. Choose an output folder such as:
   - `Builds/Windows/ArenaShooter3D`

## Recommended Sharing

For source:
- GitHub repository

For playable build:
- zip the built Windows folder
- upload the `.zip` to:
  - itch.io
  - or GitHub Releases

## Practical Recommendation

Best setup for a portfolio project:
- GitHub = source code and project files
- itch.io or GitHub Releases = downloadable Windows build
