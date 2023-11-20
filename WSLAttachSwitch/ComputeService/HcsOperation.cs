using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WSLAttachSwitch.ComputeService;

internal class HcsOperation : SafeHandle
{
    private readonly TaskCompletionSource<string> tsc;


    public HcsOperation() : base(IntPtr.Zero, true)
    {
        handle = HcsCreateOperation(CompleteCallback, IntPtr.Zero);
        tsc = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public bool IsCompleted { get; private set; }

    public string Result => GetResult();


    public override bool IsInvalid => handle == IntPtr.Zero;

    [DllImport("computecore.dll", ExactSpelling = true)]
    private static extern IntPtr HcsCreateOperation(HCS_OPERATION_COMPLETION callback, IntPtr context);

    [DllImport("computecore.dll", ExactSpelling = true)]
    private static extern void HcsCloseOperation(IntPtr operation);

    [DllImport("computecore.dll", ExactSpelling = true, PreserveSig = false)]
    private static extern void HcsWaitForOperationResult(HcsOperation operation, uint timeoutMs,
        out IntPtr resultDocument);

    public string GetResult()
    {
        HcsWaitForOperationResult(this, 0xFFFFFFFF, out var docptr);
        var result = Marshal.PtrToStringUni(docptr);
        Marshal.FreeHGlobal(docptr);
        return result;
    }

    private void CompleteCallback(IntPtr handle, IntPtr context)
    {
        IsCompleted = true;
        try
        {
            var result = GetResult();
            tsc.SetResult(result);
        }
        catch (Exception e)
        {
            tsc.SetException(e);
        }
    }

    public Task<string> AsTask()
    {
        return tsc.Task;
    }

    public TaskAwaiter<string> GetAwaiter()
    {
        return tsc.Task.GetAwaiter();
    }

    protected override bool ReleaseHandle()
    {
        HcsCloseOperation(handle);
        tsc.TrySetCanceled();
        return true;
    }

    private delegate void HCS_OPERATION_COMPLETION(IntPtr handle, IntPtr context);
}