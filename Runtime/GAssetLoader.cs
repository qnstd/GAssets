using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// ��Դ������
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class GAssetLoader : GLoader, IEnumerator
    {

        #region ��̬

        /// <summary>
        /// ����������
        /// </summary>
        static public readonly Dictionary<string, GAssetLoader> m_cache = new Dictionary<string, GAssetLoader>();

        /// <summary>
        /// ����ab������Դ���صļ���������
        /// </summary>
        static public Func<string, Type, GAssetLoader> Creator { get; set; } = GBundleAssetLoader.Create;

        /// <summary>
        /// ���� GAssetLoader ʵ������
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        static private GAssetLoader CreateInstance(string path, Type type)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException(nameof(path));
            return Creator(path, type);
        }


        static private GAssetLoader LoadInternal(string resname, Type type, Action<GAssetLoader> com = null)
        {
            //������Դ���ƣ��ж����ڵ�AssetBundle�ļ��Ƿ����
            string rname = resname.ToLower();
            string abname = GAssetManifest.GetABNameByResName(rname);
            if (abname == null)
            {
                Debug.LogError($"Ҫ������Դ���ڵ�AssetBundle�ļ����ڵ�ǰ�汾Manifest�в�����. resname = {rname}");
                return null;
            }
            if (!GAssetManager.Ins.ExistAssetBundleOnLocal(abname))
            {
                Debug.LogError($"Ҫ������Դ���ڵ�AssetBundle�ļ����ڱ�����Դ����Ŀ¼��δ�ҵ�. resname = {rname} / abname = {abname}");
                return null;
            }

            string path = GAssetManifest.GetResPathInAB(rname); //��Դ·������Assets��ͷ�����·��
            if (!m_cache.TryGetValue(path, out var loa))
            {
                loa = CreateInstance(path, type);
                m_cache.Add(path, loa);
            }

            if (com != null) loa.Completed += com;

            loa.Load();
            return loa;
        }



        /// <summary>
        /// �첽������Դ
        /// </summary>
        /// <param name="resname">��Դ����</param>
        /// <param name="type">��Դ����</param>
        /// <param name="com">�ص�ί��</param>
        /// <returns></returns>
        static public GAssetLoader LoadAsync(string resname, Type type, Action<GAssetLoader> com = null)
        {
            return LoadInternal(resname, type, com);
        }


        /// <summary>
        /// ������Դ
        /// </summary>
        /// <param name="resname">��Դ����</param>
        /// <param name="type">��Դ����</param>
        /// <returns></returns>
        static public GAssetLoader Load(string resname, Type type)
        {
            var assetloader = LoadInternal(resname, type);
            assetloader.Immediate();
            return assetloader;
        }

        /// <summary>
        /// ���غ��ж��Ƕ��Object�ĸ���Asset��Դ
        /// <para>
        /// ���磺����Ƕ���˶�����FBX�ļ������߼���Ƕ���˶�������ͼ�������Ҫ���ص�Object��������ͬһAsset��
        /// �������ֺͺܶ಻��ص�����Object�����ͬһ��AssetBundle�У��Ǿ�Ӧ��ʹ�ô˷�����
        /// </para>
        /// </summary>
        /// <param name="resname">��Դ����</param>
        /// <param name="type">��Դ����</param>
        /// <returns></returns>
        static public GAssetLoader LoadSubAssets(string resname, Type type)
        {
            var assetloader = LoadInternal(resname, type);
            if (assetloader == null)
                return null;
            assetloader.IsSubAssets = true;
            assetloader.Immediate();
            return assetloader;
        }


        /// <summary>
        /// ���غ��ж��Ƕ��Object�ĸ���Asset��Դ���첽��
        /// <para>
        /// ���磺����Ƕ���˶�����FBX�ļ������߼���Ƕ���˶�������ͼ�������Ҫ���ص�Object��������ͬһAsset��
        /// �������ֺͺܶ಻��ص�����Object�����ͬһ��AssetBundle�У��Ǿ�Ӧ��ʹ�ô˷�����
        /// </para>
        /// </summary>
        /// <param name="resname">��Դ����</param>
        /// <param name="type">��Դ����</param>
        /// <param name="com">ί�лص�</param>
        /// <returns></returns>
        static public GAssetLoader LoadSubAssetsAsync(string resname, Type type, Action<GAssetLoader> com = null)
        {
            var assetloader = LoadInternal(resname, type, com);
            if (assetloader == null)
                return null;
            assetloader.IsSubAssets = true;
            return assetloader;
        }


        /// <summary>
        /// ��ȡ������
        /// </summary>
        /// <param name="key">����������������������Դ�ļ��������ļ�����׺����</param>
        /// <returns></returns>
        static public GAssetLoader GetLoader(string key)
        {
            string respath = GAssetManifest.GetResPathInAB(key);
            GAssetLoader loa = null;
            foreach (string k in m_cache.Keys)
            {
                if (k == respath)
                {
                    loa = m_cache[k];
                    break;
                }
            }
            return loa;
        }


        /// <summary>
        /// �ͷż�����
        /// <para>�������������������ã�������ü���-1. ���������κ����ã����ü���Ϊ0��������ͷ�.</para>
        /// </summary>
        /// <param name="key">����������������������Դ�ļ��������ļ�����׺����</param>
        static public void DisposeLoader(string key)
        {
            GAssetLoader loa = GetLoader(key);
            if (loa == null) { return; }
            loa.Dispose();
        }

        #endregion


        #region ʵ�� IEnumerator �ӿ�

        public object Current => null;

        public bool MoveNext()
        {
            return !IsDone; //���������ʱ������false��������ز���δ��ɣ��򷵻�true��
        }

        public void Reset() { }

        #endregion


        /// <summary>
        /// ��Դ����
        /// </summary>
        public UnityEngine.Object Asset { get; protected set; }

        /// <summary>
        /// Ƕ�׸�������Դ��
        /// </summary>
        public UnityEngine.Object[] SubAssets { get; protected set; }

        /// <summary>
        /// ������ɵ�ί��
        /// </summary>
        public Action<GAssetLoader> Completed;

        /// <summary>
        /// ��ȡ��Դ����
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>() where T : UnityEngine.Object
        {
            return Asset as T;
        }

        /// <summary>
        /// ��Դ����
        /// </summary>
        protected Type Typ { get; set; }

        /// <summary>
        /// �Ƿ�������Ƕ�׸���Asset�ļ���
        /// </summary>
        protected bool IsSubAssets { get; set; }

        /// <summary>
        /// ������ɵĴ���
        /// </summary>
        /// <param name="target"></param>
        protected void OnLoaded(UnityEngine.Object target)
        {
            Asset = target;
            Finish(Asset == null ? "��Դ����ʧ�ܣ�Ϊ�գ�null��" : null);
        }

        /// <summary>
        /// �������ʱ�ĵ���
        /// </summary>
        protected override void OnComplete()
        {
            if (Completed == null) { return; }

            var c = Completed;
            Completed?.Invoke(this);
            Completed -= c;
        }


        /// <summary>
        /// δʹ�ú�������
        /// </summary>
        protected override void OnUnused()
        {
            Completed = null;
        }

        /// <summary>
        /// ж��ʱ�Ĳ���
        /// </summary>
        protected override void OnUnload()
        {
            m_cache.Remove(Path);
        }

    }
}