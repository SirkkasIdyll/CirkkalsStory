using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using PC.SlimeFactory.Signals;
using static GdUnit4.Assertions;

namespace PC.SlimeFactory;

public partial class NodeManager
{
    /// <summary>
    /// Declare that there can only ever be one <see cref="NodeManager"/> being used
    /// </summary>
    public static NodeManager Instance { get; } = new();
    public readonly SignalBus SignalBus = SignalBus.Instance;
    public readonly Dictionary<string, Node> NodeDictionary = [];
    public readonly Dictionary<string, Component> CompDictionary = [];
    public readonly HashSet<Dictionary<Node, Component>> ActiveNodeComps = [];
    private const string PathToNodePrototypes = "res://Nodes/Prototypes/";
    private const string PathToComponents = "res://Components/";
    
    /// <summary>
    /// Making the constructor private prevents the creation of a new <see cref="NodeManager"/>
    /// </summary>
    private NodeManager()
    {
        GetAllNodePrototypes(PathToNodePrototypes);
        GetAllComponents(PathToComponents);
    }
    
    /// <summary>
    /// Fetches all nodes and their components for future lookup
    /// Previously I was just doing GetChildren() on a Node and seeing if the node was the component I was looking for,
    /// this way we only have to do it once
    /// </summary>
    private void GetAllNodePrototypes(string path)
    {
        var nodeFiles = GetFilesByExtension(path, ".tscn");

        foreach (var file in nodeFiles)
        {
            var node = ResourceLoader.Load<PackedScene>(file).Instantiate();
            if (!node.Name.ToString().StartsWith("Base"))
                NodeDictionary.TryAdd(node.Name, node);
            else
                node.QueueFree();
        }
    }

    private void GetAllComponents(string path)
    {
        var nodeFiles = GetFilesByExtension(path, ".cs"); 

        foreach (var file in nodeFiles)
        {
            if (file.EndsWith(".Events.cs"))
                continue;
            
            var node = ResourceLoader.Load<CSharpScript>(file).New().Obj;
            if (node is Component component)
            {
                component.SetName(component.GetType().Name);
                CompDictionary.TryAdd(component.GetType().Name, component);
            }
            else if (node is Node nodeType)
                nodeType.QueueFree();
        }
    }

    /// <summary>
    /// Recursively retrieve all files that match the scene file extension in a given path
    /// </summary>
    /// <returns>A list of scenes</returns>
    public List<string> GetFilesByExtension(string path, string extension)
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
                    files.AddRange(GetFilesByExtension(path + fileName + "/", extension));
                }
                else
                {
                    if (Regex.IsMatch(fileName, "^.*\\" + extension + "$"))
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
            node.QueueFree();
        
        foreach (var node in CompDictionary.Values)
            node.QueueFree();
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
    /// Checks if GetNodeOrNull returns the type of T
    /// </summary>
    public bool HasComponent<T>(Node node) where T : Component, IComponent
    {
        var comp = node.GetNodeOrNull<T>($"{typeof(T).Name}");
        return comp != null;
    }
    
    public bool TryAddComponent<T>(Node node) where T : Component, IComponent
    {
        CompDictionary.TryGetValue(typeof(T).Name, out var component);

        if (component == null)
            return false;
        
        var dupe = component.Duplicate();
        node.AddChild(dupe);
        dupe.SetOwner(node);
        return true;
    }

    /// <summary>
    /// Checks if GetNodeOrNull returns the type of T and returns the Node
    /// </summary>
    public bool TryGetComponent<T>(Node node, [NotNullWhen(true)] out T? component) where T : Component, IComponent
    {
        component = node.GetNodeOrNull<T>($"{typeof(T).Name}");
        return component != null;
    }

    /// <summary>
    /// Removes the component if it exists and is a child of the node
    /// QueueFrees the component as well
    /// </summary>
    public void RemoveComponent<T>(Node node) where T : Component, IComponent 
    {
        var component = node.GetNodeOrNull<T>($"{typeof(T).Name}");
        if (component == null)
            return;
        
        node.RemoveChild(component);
        component.QueueFree();
    }

    /// <summary>
    /// Returns a dict of all the currently active nodes in the <see cref="SceneTree"/>
    /// with the corresponding component
    /// </summary>
    /// <param name="result"></param>
    /// <typeparam name="T"></typeparam>
    public void NodeQuery<T>(out Dictionary<Node, T> result) where T : Component
    {
        result = new Dictionary<Node, T>();
        foreach (var dict in ActiveNodeComps)
            if (dict.First().Value is T component)
                result.Add(dict.First().Key, component);
    }
}

public readonly struct Node<TComp> where TComp : IComponent?
{
    public readonly Node Owner;
    public readonly TComp Comp;

    private Node(Node owner, TComp comp)
    {
        Debug.Assert(comp?.Owner == owner);
        AssertBool(comp?.Owner == owner).IsTrue();
        
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