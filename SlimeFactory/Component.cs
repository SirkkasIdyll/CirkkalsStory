using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Godot;

namespace CS.SlimeFactory;

public abstract partial class Component : Node2D, IComponent
{
    private const string SerializeAttributeName = "Serialize";
    
    /// <summary>
    /// Set node name to the type that it is for easier retrieval in <see cref="NodeManager"/>
    /// </summary>
    public override void _Ready()
    {
        base._Ready();
        
        SetName(GetType().Name);
        SetOwner(GetParent());
    }

    public string Serialize()
    {
        JsonSerializerOptions options = new JsonSerializerOptions();
        options.WriteIndented = true;
        // options.IndentCharacter = '\t';
        
        string jsonString = "";
        
        var fields = GetType().GetFields().Where(t => t.GetCustomAttribute<Serialize>(false) != null);
        // FieldInfo[] fieldstoSerialize = [];
        var objects = new List<Dictionary<string, object?>>();
        foreach (var field in fields)
        {
            var d = new Dictionary<string, object?>();
            d.Add(field.Name, field.GetValue(this));
            objects.Add(d);
        }
        var ab = new Dictionary<string, List<Dictionary<string, object?>>>();
        ab.Add(Name.ToString(), objects);

        jsonString += JsonSerializer.Serialize(ab, options);

        return jsonString;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class Serialize : Attribute
{
    
}