# Fly&Locate

BepInEx 6 Unity IL2CPP mod for Big Walk. Flight toggles with F; locator toggles with L. While flying, WASD moves horizontally, Space/E ascends, LeftControl/Q descends, and LeftShift boosts speed. The locator displays discovered player-like objects with distance and compass bearing.

## Build

Requires the .NET 6 SDK and a Big Walk installation with BepInEx IL2CPP interop assemblies generated.

```bash
dotnet build FlyAndLocate.csproj -c Release -p:GameDir="/Users/joelmclaughlin/Documents/Steambuild 32 64bit DXVK.app/Contents/SharedSupport/prefix/drive_c/Program Files (x86)/Steam/steamapps/common/Big Walk"
```

Copy `bin/Release/net6.0/FlyAndLocate.dll` to `Big Walk/BepInEx/plugins/FlyAndLocate/`. The game-specific player discovery is intentionally heuristic and logs/retries safely so updates do not crash startup; adjust discovery predicates after inspecting the current IL2CPP dump if needed.

Plugin metadata: `com.snowby.flyandlocate`, `Fly&Locate`, `1.0.0`.
