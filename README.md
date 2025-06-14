# Welcome!

This is a small project I'm working on to familiarize myself with the Godot engine.

## Style Guidelines

### Coding Practices

- Arrange properties and functions in alphabetical order.
- Add summaries for all classes and all properties even if they seem obvious. Make your intention known, otherwise no one will know.
- Use if statements to return as early as possible to reduce nesting complexity later on.
- Delete unused template functions like Ready() and Process().

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

#### Nodes (Non-visible to player)
DO THIS
> - PoisonSpellNode.tscn
> - PoisonSpellNodeSystem.cs

DON'T DO THIS
> - Poison.tscn
> - PoisonSystem.cs