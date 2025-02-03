# Simple Timeline for Unity

Simple Timeline is an editor tool that helps create and manage sequences of actions in Unity. It provides a straightforward way to animate objects and trigger events over time.

## Features

- Visual timeline editor window
- Support for multiple agents (objects)
- Various command types:
  - Move To: Move object to target position
  - Move Add: Add offset to current position
  - Rotate To: Rotate object to target rotation
  - Rotate Add: Add rotation offset
  - Set Color: Change object color
  - Send Message: Trigger instant events

## Getting Started

1. Open your Unity project
2. Go to `Window -> Timeline Editor` to open the Timeline Editor window
3. Select any GameObject with a TimelineAgent component
4. Add commands using the inspector or Timeline Editor window

## Sample Scene

To see the tool in action:
1. Open the included SampleScene
2. Open Timeline Editor window
3. Select objects with TimelineAgent component
4. Try adding/editing commands
5. Use Play/Pause buttons to preview animations
6. Adjust timing using the timeline ruler

## Controls

- Left click and drag on time ruler: Scrub through timeline
- Mouse wheel on ruler: Zoom timeline
- Click '-' button on command: Remove command
- Click 'Set' button (where available): Set target values from current object state

## Notes

- Commands can overlap and will blend together
- Each command type has its own specific settings
- Timeline state is saved with the scene

## Requirements

- Unity 2022.3.47f1 or higher 