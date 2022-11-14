using System;
using System.Threading.Tasks;
using Table.Structure;
using UnityEngine;
using ZhFramework.Engine.Msic;
using ZhFramework.Engine.Utilities;
using ZhFramework.Unity.Resource;
using ZhFramework.Unity.Sdk;
using ZhFramework.Unity.UI;

namespace Game.Runtime
{
    internal class GameState_Version : State<GameManager>, ISdkInitAdapter, ISdkUpdateAdapter
    {
        private PageVersionLoading m_Loading;
        private FileList m_FileList;
        private LastCdnInfo m_LastCdnInfo;

        public override void HandleEvent(Enum eventType, object userData)
        {
        }

        public override Task OnPreload(object[] args)
        {
            m_Loading = UIManager.Instance.CreatePanel<PageVersionLoading>();
            _ = m_Loading.Loading();
            return Task.CompletedTask;
        }

        public override async Task OnEnter()
        {
            await Task.Delay(500);
            SDK.InitUpdate(this);

#if UNITY_EDITOR
            InitAndCheck();
#else
            await InitAndCheck();
#endif
        }

        public override void Update()
        {
        }


#if UNITY_EDITOR
        private void InitAndCheck()
        {
            SDK.CheckResUpdate();
#else
        private async Task InitAndCheck()
        {
            string lastCdnInfoUrl = SDK.GetLastCdnInfoAddress();

            CLogger.Log($"Version: lastCdnInfoUrl={lastCdnInfoUrl}");
            
            var cdnInfoRes = await HttpHelper.Get(lastCdnInfoUrl);
            if (cdnInfoRes.code != 0)
            {
                OnInitAndCheckFailed(cdnInfoRes.msg);
                return;
            }
            m_LastCdnInfo = JsonUtils.ToObject<LastCdnInfo>(cdnInfoRes.text);

            string resUrl = string.Empty;
#if DEBUG_LOG
            resUrl = $"{SDK.GetCdnAddress()}{m_LastCdnInfo.cdnDevUrl}";
#else
            resUrl = $"{SDK.GetCdnAddress()}{m_LastCdnInfo.cdnUrl}";
#endif
            CLogger.Log($"Version: resUrl={resUrl}");

            var fileListRes = await HttpHelper.Get($"{resUrl}/filelist.json");
            if (fileListRes.code != 0)
            {
                OnInitAndCheckFailed(fileListRes.msg);
                return;
            }

            m_FileList = JsonUtils.ToObject<FileList>(fileListRes.text);
            ResourceManager.InitVersioning(
                //"",
                //"",
                int.Parse(m_FileList.Version),
                resUrl
                /*"",
                ""*/);

            //暂时没有接入APP更新，直接检查更新资源
            if(SDK.IsInited)
            {
                SDK.CheckResUpdate();
            }
            else
            {
                SDK.Init(string.Empty, this);
            }
#endif
        }

        private async void OnInitAndCheckFailed(string msg)
        {
            var dialogerror = UIManager.Instance.CreatePanel<DialogVersionTips>();
            var confirmFail = await dialogerror.Show(TableSysPropertyText.Version_Error_Confirm, TableSysPropertyText.Version_Error_Cancel, TableSysPropertyText.Version_Error_Title, msg);
            if (confirmFail)
            {
#if !UNITY_EDITOR
                await InitAndCheck();
#endif
            }
            else
            {
                GameActivity.Quit();
            }
            return;
        }

        public override Task OnLeave()
        {
            m_Loading.Close();
            SDK.UninitUpdate();
            return Task.CompletedTask;
        }

        public void OnInitSucc()
        {
            SDK.CheckResUpdate();
        }

        #region SDK APP
        void ISdkUpdateAdapter.CheckAppUpdate()
        {
            if (ResourceManager.CheckAppUpdate())
            {
                SDK.OnNeedUpdateApp();
            }
            else
            {
                SDK.CheckAppUpdateCompleted();
            }
        }

        void ISdkUpdateAdapter.OnStartUpdateApp(string appUrl)
        {
            Application.OpenURL(appUrl);
        }

        async void ISdkUpdateAdapter.OnShowNeedUpdateAppDialog()
        {
            CLogger.Log($"OnShowNeedUpdateAppDialog {Application.internetReachability}");
            switch (Application.internetReachability)
            {
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    ResourceManager.StartUpdateApp();
                    break;
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    {
                        var dialog = UIManager.Instance.CreatePanel<DialogVersionTips>();
                        var ok = await dialog.Show(
                            TableSysPropertyText.SYS_BUTTON_OK,
                            TableSysPropertyText.SYS_BUTTON_EXIT,
                            TableSysPropertyText.SYS_TITLE_UPDATE_APP,
                            string.Format(TableSysPropertyText.SYS_FORMAT_APP_MESSAGE, ResourceManager.CdnResVer,
                                string.Empty));
                        if (ok)
                        {
                            ResourceManager.StartUpdateApp();
                        }
                        else
                        {
                            GameActivity.Quit();
                        }

                        break;
                    }
                default:
                    {
                        var dialog = UIManager.Instance.CreatePanel<DialogVersionTips>();
                        var ok = await dialog.Show(
                            TableSysPropertyText.SYS_BUTTON_RETRY,
                            TableSysPropertyText.SYS_BUTTON_EXIT,
                            TableSysPropertyText.SYS_TITLE_PROMPT,
                            TableSysPropertyText.SYS_UPDATE_APP_FAILED);
                        if (ok)
                        {
                            //OnShowNeedUpdateAppDialog();
                        }
                        else
                        {
                            GameActivity.Quit();
                        }

                        break;
                    }
            }
        }

        async void ISdkUpdateAdapter.OnShowUpdateAppFailedDialog()
        {
            CLogger.Error($"UpdateAppFailed");
            var dialog = UIManager.Instance.CreatePanel<DialogVersionTips>();
            var ok = await dialog.Show(
                TableSysPropertyText.SYS_BUTTON_RETRY,
                TableSysPropertyText.SYS_BUTTON_EXIT,
                TableSysPropertyText.SYS_TITLE_PROMPT,
                TableSysPropertyText.SYS_UPDATE_APP_FAILED);
            if (ok)
            {
                ResourceManager.StartUpdateApp();
            }
            else
            {
                GameActivity.Quit();
            }
        }

        void ISdkUpdateAdapter.OnChangedProgressUpdateApp(ulong downloadedBytes, ulong totalBytes)
        {
            CLogger.Log($"OnChangedProgressUpdateApp: {downloadedBytes}/{totalBytes}");
            m_Loading.SetDownloadedAndTotalSize(downloadedBytes, totalBytes);
            //m_Loading.SetTips($"{TablePersistentText.SYS_LOADING_STATE_RES_UPDATING} {GetSize(downloadedBytes)}/{GetSize(totalBytes)}");
        }

        async void ISdkUpdateAdapter.OnUpdateAppCompleted(string url)
        {
            SDK.InstallApk(url);
            var dialog = UIManager.Instance.CreatePanel<DialogVersionTips>();
            var ok = await dialog.Show(
                TableSysPropertyText.SYS_BUTTON_RETRY,
                TableSysPropertyText.SYS_BUTTON_EXIT,
                TableSysPropertyText.SYS_TITLE_PROMPT,
                TableSysPropertyText.SYS_INSTALL_APP_FAILED);
            if (ok)
            {
                //OnUpdateAppCompleted(url);
            }
            else
            {
                GameActivity.Quit();
            }
        }
        #endregion

        #region SDK RES
//#if UNITY_EDITOR
//todo liwh CDN 主线 分支的设计有问题暂时把主线的资源检测关掉
        void ISdkUpdateAdapter.CheckResUpdate()
        {
            SDK.CheckCompleted();
            CLogger.Log($"Version: CheckCompleted");
            //#else
            //        async void ISdkUpdateAdapter.CheckResUpdate()
            //        {
            //            bool update = ResourceManager.CheckResUpdate(m_FileList);
            //            if (update)
            //            {
            //                m_Loading.SetDownloadedAndTotalSize(ResourceManager.DownloadedSize, ResourceManager.TotalSize);
            //                SDK.OnNeedUpdateRes();
            //                //SDK.OnNeedUpdateRes(PersistentText.SYS_TITLE_UPDATE_RES, string.Format(PersistentText.SYS_FORMAT_RES_MESSAGE, AssetManager.RemoteResVersion, string.Empty));
            //            }
            //            else
            //            {
            //                Debug.Log("download Complete!!!!");
            //                SDK.CheckCompleted();
            //            }
            //#endif
        }

        async Task ISdkUpdateAdapter.StartUpdateRes()
        {
            //m_Loading.SetTips(TablePersistentText.SYS_LOADING_STATE_RES_UPDATING);
            await ResourceManager.StartUpdateResource(
                SDK.UpdateResProgress,
                SDK.UpdateResFailure,
                SDK.CheckCompleted);
        }

        private string GetSize(ulong size)
        {
            if (size > 1024 * 1024)
            {
                return $"{size / 1024f / 1024f:F2}M";
            }

            if (size > 1024)
            {
                return $"{size / 1024f:F2}K";
            }

            return $"{size}B";
        }

        async void ISdkUpdateAdapter.OnShowNeedUpdateResDialog()
        {
            if (Application.isEditor)
            {
                await SDK.StartUpdateRes();
                return;
            }

            switch (Application.internetReachability)
            {
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    await SDK.StartUpdateRes();
                    break;
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    {
                        var dialog = UIManager.Instance.CreatePanel<DialogVersionTips>();
                        var ok = await dialog.Show(
                            TableSysPropertyText.SYS_BUTTON_OK,
                            TableSysPropertyText.SYS_BUTTON_EXIT,
                            TableSysPropertyText.SYS_TITLE_UPDATE_RES,
                            string.Format(TableSysPropertyText.SYS_FORMAT_RES_MESSAGE, ResourceManager.CdnResVer,
                                GetSize(ResourceManager.TotalSize)));
                        if (ok)
                        {
                            await SDK.StartUpdateRes();
                        }
                        else
                        {
                            GameActivity.Quit();
                        }

                        break;
                    }
                default:
                    {
                        var dialog = UIManager.Instance.CreatePanel<DialogVersionTips>();
                        var ok = await dialog.Show(
                            TableSysPropertyText.SYS_BUTTON_RETRY,
                            TableSysPropertyText.SYS_BUTTON_EXIT,
                            TableSysPropertyText.SYS_TITLE_PROMPT,
                            TableSysPropertyText.SYS_UPDATE_RES_FAILED);
                        if (ok)
                        {
                            //OnShowNeedUpdateResDialog();
                        }
                        else
                        {
                            GameActivity.Quit();
                        }

                        break;
                    }
            }
        }

        async void ISdkUpdateAdapter.OnShowUpdateResFailedDialog(string message, long error)
        {
            CLogger.Error($"UpdateResFailed: {message}, ERROR:{error}");
            var dialog = UIManager.Instance.CreatePanel<DialogVersionTips>();
            var ok = await dialog.Show(
                TableSysPropertyText.SYS_BUTTON_RETRY,
                TableSysPropertyText.SYS_BUTTON_EXIT,
                TableSysPropertyText.SYS_TITLE_PROMPT,
                TableSysPropertyText.SYS_UPDATE_RES_FAILED);
            if (ok)
            {
                await SDK.StartUpdateRes();
            }
            else
            {
                GameActivity.Quit();
            }
        }

        void ISdkUpdateAdapter.OnChangedProgressUpdateRes(ulong downloadedBytes, ulong totalBytes)
        {
            m_Loading.SetDownloadedAndTotalSize(downloadedBytes, totalBytes);
            //m_Loading.SetTips($"{TablePersistentText.SYS_LOADING_STATE_RES_UPDATING} {GetSize(downloadedBytes)}/{GetSize(totalBytes)}");
        }

        void ISdkUpdateAdapter.OnCheckCompleted()
        {
            TableManager.Uninit();
            XStyleSheet.DestroyInstance(); // 重新加载，可能发生更新
            Translate<GameState_Login>();
        }
        #endregion
    }
}
