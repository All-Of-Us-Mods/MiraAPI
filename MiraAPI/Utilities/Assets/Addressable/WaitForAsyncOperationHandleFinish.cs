using System.Collections;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MiraAPI.Utilities.Assets.Addressable;

/// <summary>
/// An IEnumerator for awaiting an AsyncOperationHandle
/// </summary>
/// <param name="handle">The AsyncOperationHandle being awaited</param>
public class WaitForAsyncOperationHandleFinish(AsyncOperationHandle handle) : IEnumerator
{
    public object Current => null;
    public bool MoveNext()
    {
        return handle.PercentComplete != 1.0f && !handle.IsDone;
    }
    public void Reset()
    {
    }
}
