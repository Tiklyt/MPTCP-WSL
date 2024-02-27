namespace MPTCP_WSL;

/// <summary>
///     Boolean type that is thread safe
/// </summary>
public class SafeBool
{
    private int _booleanValue;

    public bool Value
    {
        get => Interlocked.CompareExchange(ref _booleanValue, 0, 0) == 1;
        set
        {
            var newValue = value ? 1 : 0;
            Interlocked.Exchange(ref _booleanValue, newValue);
        }
    }
}