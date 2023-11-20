namespace MTCP_WSL2;

/// <summary>
/// Event that should be raised when an NIC is added or removed
/// </summary>
public class CollectionUpdateEvent : EventArgs
{
    public CollectionUpdateEvent(EventType type, string interfaceName)
    {
        Type = type;
        InterfaceName = interfaceName;
    }

    public string InterfaceName { get; }
    public EventType Type { get; }
}