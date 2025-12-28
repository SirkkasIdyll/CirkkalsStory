# Welcome!

This is a small project I'm working on to familiarize myself with the Godot engine.

## Style Guidelines

### Coding Practices

- Add summaries for all classes and all properties even if they seem obvious. **Make your intention known, otherwise no one will understand.**
- Events subscribed from the SignalBus need to be manually disconnected in _ExitTree(), **failure to do so will prevent Free()ing the node and cause bugs as it responds to signals.**
- Remember to use continues when doing foreach.
- Arrange properties and functions in alphabetical order. This lessens organizational ambiguity and makes it easier to find specific code.
- Use simple if statements to return early to reduce code nesting.
- Don't use LINQ.
- Delete unused template functions like _Ready() and _Process().

### File Layout Order

1. Exports
2. NodeManager/NodeSystemManager
3. Injected NodeSystems
4. Any other fields/variables
5. Overrided methods
6. OnEvent methods
7. Public members
8. Private members

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