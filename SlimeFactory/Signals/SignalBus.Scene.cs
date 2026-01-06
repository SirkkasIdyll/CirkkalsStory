using Godot;

namespace PC.SlimeFactory.Signals;

public partial class SignalBus
{
    public delegate void ChangeActiveSceneSignalHandler(Node node, ref ChangeActiveSceneSignal args);
    public event ChangeActiveSceneSignalHandler? ChangeActiveSceneSignal;
    public void EmitChangeActiveSceneSignal(Node node, ref ChangeActiveSceneSignal args)
    {
        ChangeActiveSceneSignal?.Invoke(node, ref args);
    }
}