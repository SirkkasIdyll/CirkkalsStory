using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using CS.SlimeFactory.Signals;
using Godot;

namespace CS.SlimeFactory;

public partial class NodeManager
{
    /// <summary>
    /// Declare that there can only ever be one <see cref="NodeManager"/> being used
    /// </summary>
    public static NodeManager Instance { get; } = new();
    public readonly SignalBus SignalBus = SignalBus.Instance;
    public readonly Dictionary<string, Node> NodeDictionary = [];
    public readonly Dictionary<string, Component> CompDictionary = [];
    // public readonly Dictionary<string, Component> ComponentDictionary = [];
    
    /// <summary>
    /// Making the constructor private prevents the creation of a new <see cref="NodeManager"/>
    /// TODO: Fix magic pointer to Prototypes folder
    /// </summary>
    private NodeManager()
    {
        GetAllNodes("res://Nodes/Prototypes/");
        // GetAllComponents();
    }

    /*/// <summary>
    /// Makes a list of all components so that we can AddComponent() later
    /// </summary>
    private void GetAllComponents()
    {
        foreach (var dictionary in NodeCompHashset)
        {
            foreach (var value in dictionary.Values)
            {
                ComponentDictionary.TryAdd(value.Name, value);
            }
        }
    }*/
    
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
            NodeDictionary.TryAdd(node.Name, node);
            
            var children = node.GetChildren();
            foreach (var child in children)
            {
                if (child == null)
                    return;
                
                if (child is Component component)
                {
                    CompDictionary.TryAdd(component.Name, component);
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

        return files;
    }

    /// <summary>
    /// Prevent memory leaks by purging resources on close
    /// </summary>
    public void PurgeDictionary()
    {
        foreach (var node in NodeDictionary.Values)
        {
            node.QueueFree();
        }
    }

    /// <summary>
    /// Grabs the node from the dictionary of EVERY KNOWN NODE and duplicates it into existence if found
    /// </summary>
    public bool TrySpawnNode(string nodeName, [NotNullWhen(true)] out Node? spawnedNode)
    {
        spawnedNode = null;
        NodeDictionary.TryGetValue(nodeName, out var node);
        
        if (node == null)
            return false;
        
        spawnedNode = node.Duplicate();
        return true;
    }
    
    /// <summary>
    /// Goes through each child of the node and checks if the child is of the same type as T.
    /// </summary>
    public bool HasComponent<T>(Node node) where T : class, IComponent
    {
        var comp = node.GetNodeOrNull<T>($"{typeof(T).Name}");
        return comp != null;
    }
    
    public bool TryAddComponent<T>(Node node) where T : class, IComponent
    {
        CompDictionary.TryGetValue(typeof(T).Name, out var component);

        if (component == null)
            return false;
        
        node.AddChild(component.Duplicate());
        component.SetOwner(node);
        return true;
    }

    /// <summary>
    /// Goes through each child of the node and returns the child with the type of T
    /// </summary>
    public bool TryGetComponent<T>(Node node, [NotNullWhen(true)] out T? component) where T : class, IComponent
    {
        component = node.GetNodeOrNull<T>($"{typeof(T).Name}");
        return component != null;
    }
}

public readonly struct Node<TComp> where TComp : IComponent?
{
    public readonly Node Owner;
    public readonly TComp Comp;

    private Node(Node owner, TComp comp)
    {
        Debug.Assert(comp?.Owner == owner);
        
        Owner = owner;
        Comp = comp;
    } 
        
    public static implicit operator Node<TComp>((Node ParentNode, TComp Component) tuple)
    {
        return new Node<TComp>(tuple.ParentNode, tuple.Component);
    }

    public static implicit operator Node<TComp?>(Node owner)
    {
        return new Node<TComp?>(owner, default);
    }

    public static implicit operator Node(Node<TComp> ent)
    {
        return ent.Owner;
    }

    public static implicit operator TComp(Node<TComp> ent)
    {
        return ent.Comp;
    }

    public readonly void Deconstruct(out Node parentNode, out TComp comp)
    {
        parentNode = Owner;
        comp = Comp;
    }
}