namespace CS.SlimeFactory;

/// <summary>
/// This exists so we can type-hint it inside <see cref="NodeSystemManager"/>
/// </summary>
public interface INodeSystem
{
    /// <summary>
    /// Called once by the <see cref="NodeSystemManager"/> upon its construction
    /// The custom System equivalent of the Ready function normally used for Nodes
    /// </summary>
    public void _SystemReady() { }
}