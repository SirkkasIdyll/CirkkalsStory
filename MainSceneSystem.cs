using CS.SlimeFactory;
using CS.SlimeFactory.Signals;
using Godot;

namespace CS;

public partial class MainSceneSystem : Node2D
{
    private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;
    private readonly NodeManager _nodeManager = NodeManager.Instance;

    private Node? _activeScene;

    [ExportCategory("Instantiated")]
    [Export] private PackedScene? _escapeMenuSceneSystem;
    
    [ExportCategory("Owned")]
    [Export] private CanvasLayer _canvasLayer = null!;

    public override void _Ready()
    {
        GetViewport().SetPhysicsObjectPicking(true);
        GetViewport().SetPhysicsObjectPickingSort(true);
        GetViewport().SetPhysicsObjectPickingFirstOnly(true);
        
        _nodeSystemManager.InitializeNodeSystems(this);
        _nodeSystemManager.InjectNodeSystemDependencies();
        _nodeManager.SignalBus.ChangeActiveSceneSignal += OnChangeActiveScene;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        
        _nodeManager.SignalBus.ChangeActiveSceneSignal -= OnChangeActiveScene;
        _nodeManager.PurgeDictionary();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey eventKey)
            return;
        
        if (eventKey.IsPressed() && eventKey.Keycode == Key.Escape)
        {
            var node = _escapeMenuSceneSystem?.Instantiate();
            _canvasLayer.AddChild(node);
            GetViewport().SetInputAsHandled();
        }
    }

    /// <summary>
    /// Change the active scene by deleting the currently active scene and replacing it<br />
    /// The active scene is the scene that is always returned to after all others are closed<br />
    /// </summary>
    private void OnChangeActiveScene(Node node, ref ChangeActiveSceneSignal args)
    {
        // Add new scene to scene tree
        var prevScene = _activeScene;
        var newScene = args.NewScene.Instantiate();
        _activeScene = newScene;
        AddChild(newScene); 
        /*_canvasLayer.AddChild(newScene);
        _activeScene.SetOwner(_canvasLayer);*/
        
        // Fade out previous scene before deleting it entirely
        if (prevScene != null)
        {
            var tweenQueueFreeTarget = prevScene;
            var tweenQueueFree = CreateTween();
            tweenQueueFree.TweenProperty(tweenQueueFreeTarget, "modulate", new Color(1, 1, 1, 0), 0.3);
            tweenQueueFree.TweenCallback(Callable.From(tweenQueueFreeTarget.QueueFree));
        }
        
        // Fade in new scene
        var tweenIntoSceneTarget = _activeScene;
        tweenIntoSceneTarget.Call(CanvasItem.MethodName.SetModulate, new Color(1, 1, 1, 0));    
        var tweenIntoScene = CreateTween();
        tweenIntoScene.TweenProperty(tweenIntoSceneTarget, "modulate", new Color(1, 1, 1, 1),  0.3);
    }
}

public partial class ChangeActiveSceneSignal : UserSignalArgs
{
    public readonly PackedScene NewScene;

    public ChangeActiveSceneSignal(PackedScene newScene)
    {
        NewScene = newScene;
    }
}