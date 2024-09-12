using System.Collections;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MiraAPI.Utilities.Assets.Addressable;
public class WaitForAsyncOperationHandleFinish : IEnumerator
{
    internal AsyncOperationHandle handle;
    public WaitForAsyncOperationHandleFinish(AsyncOperationHandle handle)
    {
        this.handle = handle;
    }

    public object Current => null;
    public bool MoveNext()
    {
        return handle.PercentComplete != 1.0f && !handle.IsDone;
    }
    public void Reset()
    {
    }
}
