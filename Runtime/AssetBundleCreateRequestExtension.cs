using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// 异步加载 AssetBundle 扩展
    /// <para>作者：强辰</para>
    /// </summary>
    public static class AssetBundleCreateRequestExtension
    {
        static public EnumeratorAwaiter<AssetBundle> GetAwaiter(this AssetBundleCreateRequest instruction)
        {
            EnumeratorAwaiter<AssetBundle> awaiter = new EnumeratorAwaiter<AssetBundle>();
            SyncContextUtil.RunOnUnityScheduler
            (() =>
            {
                CoroutineRunner.instance.StartCoroutine(AssetBundleCreateRequest(awaiter, instruction));
            });
            return awaiter;
        }

        static IEnumerator AssetBundleCreateRequest(EnumeratorAwaiter<AssetBundle> awaiter, AssetBundleCreateRequest instruction)
        {
            yield return instruction;
            awaiter.Complete(instruction.assetBundle, null);
        }
    }
}

