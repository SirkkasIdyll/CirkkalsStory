using System.Collections.Generic;
using Godot;

namespace CS.Components.Skills;

public partial class SkillListComponent : Node2D
{
    [Export] public Godot.Collections.Array<string> SkillList = [];
}