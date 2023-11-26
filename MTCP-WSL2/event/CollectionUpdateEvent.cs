namespace MTCP_WSL2;

/// <summary>
///     Event that should be raised when an NIC is added or removed
/// </summary>
public class CollectionUpdateEvent : EventArgs
{
    public NetworkInformation NetworkInfo { get; set; }
    public EventType Type { get; set; }
}