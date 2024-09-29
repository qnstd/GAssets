using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// 异步从 AssetBundle 包中加载资源扩展
    /// <para>作者：强辰</para>
    /// </summary>
    public static class AssetBundleRequestExtension
    {
        static public EnumeratorAwaiter<UnityEngine.Object> GetAwaiter(this AssetBundleRequest instruction)
        {
            EnumeratorAwaiter<UnityEngine.Object> awaiter = new EnumeratorAwaiter<Object>();
            SyncContextUtil.RunOnUnityScheduler
            (
                () =>
                {
                    CoroutineRunner.instance.StartCoroutine(AssetBundleRequest(awaiter, instruction));
                }
            );
            return awaiter;
        }


        static IEnumerator AssetBundleRequest(EnumeratorAwaiter<UnityEngine.Object> awaiter, AssetBundleRequest instruction)
        {
            yield return instruction;
            awaiter.Complete(instruction.asset, null);
        }
    }

}
