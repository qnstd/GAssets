using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// �첽���� AssetBundle ��չ
    /// <para>���ߣ�ǿ��</para>
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

