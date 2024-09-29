using System.Collections;
using Unity.VisualScripting;
using UnityEngine.Networking;

namespace cngraphi.gassets
{
    /// <summary>
    /// UnityWebRequest 异步加载扩展
    /// <para>作者：强辰</para>
    /// </summary>
    public static class UnityWebRequestAsyncOperationExtension
    {
        static public EnumeratorAwaiter<UnityWebRequestAsyncOperation> GetAwaiter(this UnityWebRequestAsyncOperation instruction)
        {
            return GetEnumeratorAwaiter(instruction);
        }

        static private EnumeratorAwaiter<T> GetEnumeratorAwaiter<T>(T instruction)
        {
            EnumeratorAwaiter<T> awaiter = new EnumeratorAwaiter<T>();
            SyncContextUtil.RunOnUnityScheduler(() =>
            {
                CoroutineRunner.instance.StartCoroutine(GetEnumeratorAwaiterCoro(awaiter, instruction));
            });
            return awaiter;
        }

        static private IEnumerator GetEnumeratorAwaiterCoro<T>(EnumeratorAwaiter<T> awaiter, T instruction)
        {
            yield return instruction;
            awaiter.Complete(instruction, null);
        }
    }
}


