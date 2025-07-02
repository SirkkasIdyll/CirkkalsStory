using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using CS.Components.Skills;
using Godot;

namespace CS.SlimeFactory;

public partial class NodeManager : Node
{
    /// <summary>
    /// Making the constructor private prevents the creation of a new <see cref="NodeManager"/>
    /// </summary>
    private NodeManager()
    {
        GetAllNodes("res://Nodes/Game/");
        // Fetch("res://Nodes/Game/Mobs");
    }

    /// <summary>
    /// Declare that there can only ever be one <see cref="NodeManager"/> being used
    /// </summary>
    public static NodeManager Instance { get; } = new();
    public readonly SignalBus SignalBus = SignalBus.Instance;
    public HashSet<Dictionary<Node, Component>> NodeCompHashset = new();
    // private Dictionary<string, Component> _componentDictionary = [];

    /// <summary>
    /// Fetches all nodes and their components for future lookup
    /// Previously I was just doing GetChildren() on a Node and seeing if the node was the component I was looking for,
    /// this way we only have to do it once
    /// </summary>
    private void GetAllNodes(string path)
    {
        var nodeFiles = GetAllSceneFiles(path);

        foreach (var file in nodeFiles)
        {
            var node = ResourceLoader.Load<PackedScene>(file).Instantiate();
            // GD.Print(node.Name);

            var children = node.GetChildren();
            
            foreach (var child in children)
            {
                if (child == null)
                    return;
                
                if (child is Component component)
                {
                    var dic = new Dictionary<Node, Component> { { node, component } };
                    NodeCompHashset.Add(dic);
                }
            }
        }
    }

    /// <summary>
    /// Recursively retrieve all files that match the scene file extension in a given path
    /// </summary>
    /// <param name="path"></param>
    /// <returns>A list of scenes</returns>
    private List<string> GetAllSceneFiles(string path)
    {
        List<string> files = [];
        
        using var dir = DirAccess.Open(path);
        if (dir != null)
        {
            dir.ListDirBegin();
            // files.AddRange(dir.GetFiles());
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                if (dir.CurrentIsDir())
                {
                    files.AddRange(GetAllSceneFiles(path + fileName + "/"));
                }
                else
                {
                    if (Regex.IsMatch(fileName, "^.*\\.tscn$"))
                        files.Add(path + fileName);
                }
                fileName = dir.GetNext();
            }
        }
        else
        {
            GD.Print("An error occurred when trying to access the path.");
        }

        return files;
    }

    /// <summary>
    /// Makes a list of all components so that we can AddComponent() later
    /// </summary>
    private void GetAllComponents()
    {
        
    }
    
    /// <summary>
    /// Goes through each child of the node and checks if the child is of the same type as T.
    /// </summary>
    /// <param name="node">Mob, skill, whatever</param>
    /// <returns>True if component was found, false if otherwise</returns>
    public bool HasComponent<T>(Node node)
    {
        var children = node.GetChildren();

        foreach (var child in children)
        {
            if (child is T)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Goes through each child of the node and returns the child with the type of T
    /// </summary>
    /// <param name="node">Mob, skill, whatever</param>
    /// <param name="component">The component if found, null otherwise</param>
    /// <returns>True if component was found, false if otherwise</returns>
    public bool TryGetComponent<T>(Node node, [NotNullWhen(true)] out T? component)
    {
        var children = node.GetChildren();
        
        foreach (var child in children)
        {
            if (child is not T generic)
                continue;
            
            component = generic;
            return true;
        }

        component = default;
        return false;
    }
    
    
}