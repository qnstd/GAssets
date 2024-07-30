using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.U2D;

using cngraphi.gassets.common;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace cngraphi.gassets
{
    /// <summary>
    /// ��Դ������
    /// <para>������Դ������Դ�����������ù�ϵ����ع�������</para>
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class GAssetManager : MonoBehaviour
    {
        #region ��̬
        const string MANIFEST = "manifest"; //��Դ������Ϣ�ļ�
        public const string CONFIGURE_FILENAME = "GAssetSettings"; // ��Դ�����������ļ���

        static private string m_resRootPath = ""; //��Դ·����ֻ��Ŀ¼��
        static private string m_resDataPath = ""; //����·�����ɶ���дĿ¼��

        [RuntimeInitializeOnLoadMethod]
        static private void Initialize()
        {
            // ��ʼ������
            settings = Resources.Load<GAssetSettings>(CONFIGURE_FILENAME);
            if (settings == null)
            {
                Debug.LogError("��ʧ GAssets Settings �����ļ�.");
                return;
            }

            // �������·��
            m_resRootPath = Paths.StreamingPathAppend(settings.AssetRootPath);
            m_resDataPath = Paths.PersistentPathAppend(settings.AssetDataPath);

#if UNITY_EDITOR
            if (!Directory.Exists(m_resRootPath))
            {
                IO.RecursionDirCreate(m_resRootPath);
                AssetDatabase.Refresh();
            }
#endif

            if (!Directory.Exists(m_resDataPath))
            {//����Ŀ¼�����ڣ���ݹ鴴��
                IO.RecursionDirCreate(m_resDataPath);
            }

            // ��ʼ����Դ����������
            GameObject obj = new GameObject("GAssetManager");
            Ins = obj.AddComponent<GAssetManager>();
            DontDestroyOnLoad(obj);
        }


        static public GAssetSettings settings { get; set; } = null;


        static public GAssetManager Ins { get; private set; }
        #endregion


        /// <summary>
        /// ��ʼ�� Mainfest ����
        /// </summary>
        /// <param name="callback">��ʼ����ϵĻص����ڱ༭ģʽ�¿�����Ϊnull��ֻ���ڷ���֮����Ҫ���ûص���</param>
        public void LoadManifest(Action callback = null)
        {

#if UNITY_EDITOR
            ReadManifest();
            callback?.Invoke();
#else
            // ��Ҫ��ĸ���� manifest �ļ��������־û�Ŀ¼��
            string tarpath = Path.Combine(m_resDataPath, MANIFEST);
            if (File.Exists(tarpath))
            {// ���ش��ڣ���ֱ�Ӷ�ȡ
                ReadManifest();
                callback?.Invoke();
            }
            else
            {// �����ڣ�˵���ǵ�һ�����С��Ƚ�Ĭ�ϵ� manifest �ļ�����������Ŀ¼���ٶ�ȡ��Ϣ
                Debug.Log("��һ�����У��ȴӰ��彫 Manifest �ļ�����������Ŀ¼.");
                CoroutineRunner.instance.StartCoroutine(CopyManifest(callback));
            }
#endif
        }
        IEnumerator CopyManifest(Action callback)
        {// ����Manifest�����õ�����Ŀ¼�������ؽ�������
            UnityWebRequest req = UnityWebRequest.Get(Path.Combine(m_resRootPath, MANIFEST));
            yield return req.SendWebRequest();
            File.WriteAllBytes(Path.Combine(m_resDataPath, MANIFEST), Encoding.UTF8.GetBytes(req.downloadHandler.text));
            req.Dispose();
            Debug.Log("GAssets Manifest �������.");
            ReadManifest();
            callback?.Invoke();
        }
        private void ReadManifest()
        {
            if (!GAssetManifest.Load(GetResPath(MANIFEST)))
                Debug.LogError("GAssets Manifest ���ػ����ʧ��.");
            else
                Debug.Log("GAssets Mainfest ���ز��������.");
        }


        /// <summary>
        /// ����δʹ����Դ [����ʱ]
        /// </summary>
        /// <returns></returns>
        public AsyncOperation Clean()
        {
            return Resources.UnloadUnusedAssets();
        }


        /// <summary>
        /// ��ȡ��Դ·��
        /// </summary>
        /// <param name="res">��Դ����</param>
        /// <returns>��������Դ���򷵻���Դ��·�����������ڣ��򷵻ز���</returns>
        public string GetResPath(string res)
        {
#if UNITY_EDITOR //Editor�༭��������

            string rsp = Path.Combine(m_resRootPath, res);
            rsp = Paths.Replace(rsp);
            if (File.Exists(rsp))
            {
                return rsp;
            }
            return res;

#else //������

            string p = Path.Combine(m_resDataPath, res); //����Ŀ¼��Ҳ���ǿɶ���дĿ¼��
            p = Paths.Replace(p);
            if (File.Exists(p))
                return p;

            p = Path.Combine(m_resRootPath, res); //StreamingAssets���Ӽ�Ŀ¼��ֻ�ɶ�������д���ļ���Ҳ������ʹ�õ�Ŀ¼��
            p = Paths.Replace(p);

            if (GAssetManifest.ExistAB(res))
                return p;

            return res;
#endif
        }


        /// <summary>
        /// �жϲ��������AssetBundle�ļ��Ƿ�����ڱ���
        /// </summary>
        /// <param name="abname">AssetBundle�ļ�����</param>
        /// <returns>true�����ڣ�false��������</returns>
        public bool ExistAssetBundleOnLocal(string abname)
        {
            if (string.IsNullOrEmpty(abname)) { return false; }
            string p = GetResPath(abname);
            if (p.IndexOf("/") == -1) { return false; }
            return true;
        }


        /// <summary>
        /// ��æ״̬
        /// <para>true����æ��false������</para>
        /// </summary>
        public bool Busy { get { return Time.realtimeSinceStartup - m_time >= 0.01f; /* 10������ӳ� */ } }



        #region MonoBehaviour ��Ϊ
        private float m_time = 0;
        private void Start()
        {
            SpriteAtlasManager.atlasRequested += _SpriteAtlasRequestHandle;
        }
        private void Update()
        {
            m_time = Time.realtimeSinceStartup;
            GLoader.UpdateAll();
            GOperate.UpdateAll();
            GAsyncCom.UpdateAll();
        }
        private void OnDestroy()
        {
            SpriteAtlasManager.atlasRequested -= _SpriteAtlasRequestHandle;
            GLoader.ClearAll();
            GOperate.ClearAll();
            GAsyncCom.ClearAll();
        }
        /// <summary>
        /// SpriteAtlas ͼ���ӳٰ�
        /// </summary>
        /// <param name="tag">atlas���ƣ������ļ���׺��</param>
        /// <param name="action">�� atlas �ش��� unity ��ί��</param>
        void _SpriteAtlasRequestHandle(string tag, Action<SpriteAtlas> action)
        {
            /*
                �����ڹ��� AssetBundle ����ʱ���� SpriteAtlas ÿ��ͼ���ļ��������� AssetBundle ����
                �������ӳٰ�ʱ�ļ��ش������õ���ͬ�����ط�ʽ�����ܻ���ߡ���ͼ�������� icon ����
                ��һ�����壬��ô�ڼ���ʱ�����Ĵ�����ʱ�䣬���һ���ɲ�ͬ����Ŀ��٣����ݰ���ߴ磩��
             */

            string atlasname = tag + ".spriteatlas";
            string abname = GAssetManifest.GetABNameByResName(atlasname);
            if (string.IsNullOrEmpty(abname))
            {
                Debug.LogError($"spriteatlas �ӳٰ��쳣. ͼ���ļ� {atlasname} δ�ҵ���Ӧ�� AssetBundle �ļ�.");
                return;
            }
            Str.Split(abname, ".", out List<string> lst);

            AssetBundle ab = GLoader.FindABundleByName(lst[0]); // �ж�����ʱ�ڴ����Ƿ���ڶ�Ӧ�� AssetBundle �ļ�
            if (ab == null)
            { // �����ڣ������ab������ͼ��
              // ע�⣺�������� GAssetLoader ���غ�û�н����������ϲ��߼����л��档��������Ҫ�� SpriteAtlas ʱ��ֻ����� GAssetLoader.DisposeLoader("ͼ������") �����ͷš�
                action.Invoke((SpriteAtlas)GAssetLoader.Load(atlasname, typeof(SpriteAtlas)).Asset);
            }
            else
            { // ���ڣ�ֱ�Ӽ���ͼ����Դ
                action.Invoke(ab.LoadAsset<SpriteAtlas>(tag));
            }
        }
        #endregion

    }
}