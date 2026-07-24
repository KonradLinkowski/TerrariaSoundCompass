# SoundCompass

*Directional audio helper overlay for tModLoader.*

## Compass Mode
SoundCompass shows where active sound emitters are around you so you can react faster to threats and activity off-screen.

- **Center Ring mode**: markers are placed on a circle around screen center, pointing toward sound sources.
- **Screen Border mode**: markers are projected to the screen edges in the correct direction.
- **Outside Focus Area Only**: optionally show only emitters outside your central focus area.
- Distance-based fade: farther emitters are dimmer, close emitters are brighter.

## What It Tracks
- NPC-based emitters
- Active hostile/friendly moving projectiles
- Your currently used held item while animating

## Markers
When available, markers use real Terraria sprites (NPC, projectile, item icons). If a sprite cannot be resolved, SoundCompass falls back to a clean directional marker.

## Default Controls
- **P** - Toggle Overlay
- **O** - Switch Compass Type (Circle / Screen Border)

## Client Config
You can customize overlay visibility, compass layout, circle radius, border inset, and focus-area filtering in the Mod Config menu.

## Repository
Source code and issue tracking:
- https://github.com/KonradLinkowski/TerrariaSoundCompass

## License
Licensed under the GNU Affero General Public License v3.0 (AGPL-3.0).
See LICENSE for the full text.
