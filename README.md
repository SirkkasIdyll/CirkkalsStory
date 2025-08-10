# Welcome!

This is a small project I'm working on to familiarize myself with the Godot engine.

## Style Guidelines

### Coding Practices

- Don't use LINQ.
- Arrange properties and functions in alphabetical order. This lessens organizational ambiguity and makes it easier to find specific code.
- Add summaries for all classes and all properties even if they seem obvious. Make your intention known, otherwise no one will know.
- Use if statements to return as early as possible to reduce nesting complexity later on.
- Delete unused template functions like _Ready() and _Process().
- Parents use signal from children, then call functions from their children to update them. Children do not ever need to be aware of their parents.
- Events subscribed from the SignalBus need to be manually disconnected in _ExitTree(), failure to do so will make the node unavailable to be freed and cause bugs.
- CODE IN _PROCESS OR _PHYSICSPROCESS PROBABLY NEEDS CONTINUE, NOT RETURN

### Naming Practices

Follow these naming practices, so it's obvious what type of file you're working with, and so that systems sort next to their respective scenes.

#### Components 
> ExampleComponent.cs

#### Scene (Visible to player)
DO THIS
> - BattleScene.tscn
> - BattleSceneSystem.cs

DON'T DO THIS
> - Main.tscn
> - MainSystem.cs

#### Scene UI fragment
DO THIS
> - VolumeHSliderSystem.cs

DON'T DO THIS
> - VolumeSystem.cs