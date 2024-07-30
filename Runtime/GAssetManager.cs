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
    /// 资源管理器
    /// <para>负责资源的数据源、依赖及引用关系的相关管理工作。</para>
    /// <para>作者：强辰</para>
    /// </summary>
    public class GAssetManager : MonoBehaviour
    {
        #region 静态
        const string MANIFEST = "manifest"; //资源配置信息文件
        public const string CONFIGURE_FILENAME = "GAssetSettings"; // 资源管理器配置文件名

        static private string m_resRootPath = ""; //资源路径（只读目录）
        static private string m_resDataPath = ""; //数据路径（可读可写目录）

        [RuntimeInitializeOnLoadMethod]
        static private void Initialize()
        {
            // 初始化配置
            settings = Resources.Load<GAssetSettings>(CONFIGURE_FILENAME);
            if (settings == null)
            {
                Debug.LogError("丢失 GAssets Settings 配置文件.");
                return;
            }

            // 设置相关路径
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
            {//下载目录不存在，则递归创建
                IO.RecursionDirCreate(m_resDataPath);
            }

            // 初始化资源管理器对象
            GameObject obj = new GameObject("GAssetManager");
            Ins = obj.AddComponent<GAssetManager>();
            DontDestroyOnLoad(obj);
        }


        static public GAssetSettings settings { get; set; } = null;


        static public GAssetManager Ins { get; private set; }
        #endregion


        /// <summary>
        /// 初始化 Mainfest 配置
        /// </summary>
        /// <param name="callback">初始化完毕的回调（在编辑模式下可设置为null，只有在发布之后需要设置回调）</param>
        public void LoadManifest(Action callback = null)
        {

#if UNITY_EDITOR
            ReadManifest();
            callback?.Invoke();
#else
            // 需要将母包的 manifest 文件拷贝到持久化目录内
            string tarpath = Path.Combine(m_resDataPath, MANIFEST);
            if (File.Exists(tarpath))
            {// 本地存在，则直接读取
                ReadManifest();
                callback?.Invoke();
            }
            else
            {// 不存在，说明是第一次运行。先将默认的 manifest 文件拷贝到数据目录，再读取信息
                Debug.Log("第一次运行，先从包体将 Manifest 文件拷贝至数据目录.");
                CoroutineRunner.instance.StartCoroutine(CopyManifest(callback));
            }
#endif
        }
        IEnumerator CopyManifest(Action callback)
        {// 拷贝Manifest主配置到数据目录，并加载解析配置
            UnityWebRequest req = UnityWebRequest.Get(Path.Combine(m_resRootPath, MANIFEST));
            yield return req.SendWebRequest();
            File.WriteAllBytes(Path.Combine(m_resDataPath, MANIFEST), Encoding.UTF8.GetBytes(req.downloadHandler.text));
            req.Dispose();
            Debug.Log("GAssets Manifest 拷贝完毕.");
            ReadManifest();
            callback?.Invoke();
        }
        private void ReadManifest()
        {
            if (!GAssetManifest.Load(GetResPath(MANIFEST)))
                Debug.LogError("GAssets Manifest 加载或解析失败.");
            else
                Debug.Log("GAssets Mainfest 加载并解析完毕.");
        }


        /// <summary>
        /// 清理未使用资源 [运行时]
        /// </summary>
        /// <returns></returns>
        public AsyncOperation Clean()
        {
            return Resources.UnloadUnusedAssets();
        }


        /// <summary>
        /// 获取资源路径
        /// </summary>
        /// <param name="res">资源名称</param>
        /// <returns>若存在资源，则返回资源的路径；若不存在，则返回参数</returns>
        public string GetResPath(string res)
        {
#if UNITY_EDITOR //Editor编辑器环境内

            string rsp = Path.Combine(m_resRootPath, res);
            rsp = Paths.Replace(rsp);
            if (File.Exists(rsp))
            {
                return rsp;
            }
            return res;

#else //发布后

            string p = Path.Combine(m_resDataPath, res); //数据目录（也就是可读可写目录）
            p = Paths.Replace(p);
            if (File.Exists(p))
                return p;

            p = Path.Combine(m_resRootPath, res); //StreamingAssets或子集目录（只可读，不可写、文件流也不允许使用的目录）
            p = Paths.Replace(p);

            if (GAssetManifest.ExistAB(res))
                return p;

            return res;
#endif
        }


        /// <summary>
        /// 判断参数代表的AssetBundle文件是否存在于本地
        /// </summary>
        /// <param name="abname">AssetBundle文件名称</param>
        /// <returns>true：存在；false：不存在</returns>
        public bool ExistAssetBundleOnLocal(string abname)
        {
            if (string.IsNullOrEmpty(abname)) { return false; }
            string p = GetResPath(abname);
            if (p.IndexOf("/") == -1) { return false; }
            return true;
        }


        /// <summary>
        /// 繁忙状态
        /// <para>true：繁忙；false：正常</para>
        /// </summary>
        public bool Busy { get { return Time.realtimeSinceStartup - m_time >= 0.01f; /* 10毫秒的延迟 */ } }



        #region MonoBehaviour 行为
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
        /// SpriteAtlas 图集延迟绑定
        /// </summary>
        /// <param name="tag">atlas名称（不带文件后缀）</param>
        /// <param name="action">将 atlas 回传给 unity 的委托</param>
        void _SpriteAtlasRequestHandle(string tag, Action<SpriteAtlas> action)
        {
            /*
                建议在构建 AssetBundle 包体时，将 SpriteAtlas 每个图集文件单独构建 AssetBundle 包，
                这样在延迟绑定时的加载处理（启用的是同步加载方式）性能会更高。若图集与其他 icon 构建
                到一个包体，那么在加载时会消耗大量的时间，并且会造成不同情况的卡顿（依据包体尺寸）。
             */

            string atlasname = tag + ".spriteatlas";
            string abname = GAssetManifest.GetABNameByResName(atlasname);
            if (string.IsNullOrEmpty(abname))
            {
                Debug.LogError($"spriteatlas 延迟绑定异常. 图集文件 {atlasname} 未找到对应的 AssetBundle 文件.");
                return;
            }
            Str.Split(abname, ".", out List<string> lst);

            AssetBundle ab = GLoader.FindABundleByName(lst[0]); // 判断运行时内存中是否存在对应的 AssetBundle 文件
            if (ab == null)
            { // 不存在，则加载ab并加载图集
              // 注意：这里启用 GAssetLoader 加载后并没有将加载器在上层逻辑进行缓存。若不再需要此 SpriteAtlas 时，只需调用 GAssetLoader.DisposeLoader("图集名称") 即可释放。
                action.Invoke((SpriteAtlas)GAssetLoader.Load(atlasname, typeof(SpriteAtlas)).Asset);
            }
            else
            { // 存在，直接加载图集资源
                action.Invoke(ab.LoadAsset<SpriteAtlas>(tag));
            }
        }
        #endregion

    }
}