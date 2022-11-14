using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Table.Structure;
using Battle.Unity;
using System.Threading.Tasks;
using AK.Wwise;
using System;
using ZhFramework.Engine.Msic;
using ZhFramework.Unity.UI;
using Event = AK.Wwise.Event;
using System.IO;
using System.Text;
using ZhFramework.Engine.Utilities;
using ZhFramework.Engine;


namespace Game.Runtime
{
    //public class AssetCooler
    //{
    //    //private HashSet<string> m_Inuse = new HashSet<string>();
    //    private Dictionary<uint, DateTime> m_WarmShelf = new Dictionary<uint, DateTime>();
    //    private Dictionary<uint, DateTime> m_ColdShelf = new Dictionary<uint, DateTime>();
    //    private static int k_WarmShelfLife = 2 * 60 * 1000;
    //    private static int k_ColdShelfLife = 2 * 60 * 1000;
    //    private Timer k_WarmTimer;
    //    private static Action<int> m_WarmRefresh;
    //    private Timer k_ColdTimer;
    //    private static Action<int> m_ColdRefresh;
    //    public void Init()
    //    {
    //        k_WarmTimer = TimerPool.Start(float.MaxValue, k_WarmShelfLife, m_WarmRefresh);
    //        k_ColdTimer = TimerPool.Start(float.MaxValue, k_ColdShelfLife, m_ColdRefresh);
    //        m_WarmRefresh += CleanWarmShelf;
    //        m_ColdRefresh += CleanColdShelf;
    //    }
    //    public void SetInUse(uint name)
    //    {
    //        CleanFromShelves(name);
    //    }

    //    public void CleanFromShelves(uint name)
    //    {
    //        //m_Inuse.Add(name);
    //        m_WarmShelf.Remove(name);
    //        m_ColdShelf.Remove(name);
    //    }

    //    public void CleanFromShelves(List<uint> ids)
    //    {
    //        foreach (var id in ids)
    //        {
    //            CleanFromShelves(id);
    //        }
    //    }

    //    public bool IsOnShelf(uint id)
    //    {
    //        if (m_WarmShelf.ContainsKey(id) || m_ColdShelf.ContainsKey(id))
    //        {
    //            return true;
    //        }
    //        return false;
    //    }
    //    public void ReleaseFromUse(uint name)
    //    {
    //        //m_Inuse.Remove(name);
    //        m_WarmShelf[name] = DateTime.Now;
    //    }
    //    public void CleanShelves()
    //    {
    //        CleanWarmShelf(0);
    //        CleanColdShelf(0);
    //    }
    //    private void CleanWarmShelf(int milliseconds)
    //    {
    //        var now = DateTime.Now;
    //        var tempShelf = new List<uint>();
    //        foreach (var warmItem in m_WarmShelf)
    //        {
    //            if ((now - warmItem.Value).Milliseconds > k_WarmShelfLife)
    //            {
    //                tempShelf.Add(warmItem.Key);
    //            }
    //        }
    //        foreach (var tempItem in tempShelf)
    //        {
    //            m_WarmShelf.Remove(tempItem);
    //            m_ColdShelf[tempItem] = now;
    //        }
    //    }
    //    private void CleanColdShelf(int milliseconds)
    //    {
    //        var now = DateTime.Now;
    //        var tempShelf = new List<uint>();
    //        foreach (var warmItem in m_ColdShelf)
    //        {
    //            if ((now - warmItem.Value).Milliseconds > k_WarmShelfLife)
    //            {
    //                tempShelf.Add(warmItem.Key);
    //            }
    //        }
    //        SfxManager.Instance.UnloadEvents(tempShelf);
    //        foreach (var tempItem in tempShelf)
    //        {
    //            m_ColdShelf.Remove(tempItem);
    //        }
    //    }
    //}


    public class SoundBankConfig
    {
        public SoundBankInfo SoundBanksInfo;
    }

    public class SoundBankInfo
    {
        public List<SoundBankData> SoundBanks;
    }

    public class SoundBankData
    {
        public List<EventData> IncludedEvents;
        public string ShortName;
    }

    public class EventData
    {
        public string Name;
    }

    //public class SfxManager : SingletonBehavior<SfxManager>
    //{
    //    //临时使用
    //    public static string k_SoundBankPath = "Assets/_Art/WwiseSoundbank/GeneratedSoundBanks/Android";
    //    private SoundBankConfig m_soundBankConfig;
    //    private List<string> m_ConsistentBanks;
    //    private AssetCooler m_Cooler;
    //    [Hotfix] private GameObject emitter;


    //    protected override void OnPreload(params object[] args)
    //    {
    //        base.OnPreload(args);

    //        var bytes = File.ReadAllBytes($"{k_SoundBankPath}/SoundbanksInfo.json");
    //        var configData = Encoding.UTF8.GetString(bytes);
    //        m_soundBankConfig = JsonUtils.ToObject<SoundBankConfig>(configData);
    //        m_Cooler = new AssetCooler();
    //        m_Cooler.Init();
    //        TempLoadBanks();
    //    }


    //    private void TempLoadBanks()
    //    {
    //        foreach (var bank in m_soundBankConfig.SoundBanksInfo.SoundBanks)
    //        {
    //            LoadBank(bank.ShortName);
    //        }
    //    }


    //    protected override void BindUIEvents()
    //    {
    //        base.BindUIEvents();
    //        XButton.onClickSfx += PlayButtonClickedEvent;

    //    }

    //    protected override void UnBindUIEvents()
    //    {
    //        base.UnBindUIEvents();
    //        XButton.onClickSfx -= PlayButtonClickedEvent;
    //    }

    //    protected override void OnDestroy()
    //    {
    //        base.OnDestroy();
    //        foreach (var bank in m_soundBankConfig.SoundBanksInfo.SoundBanks)
    //        {
    //            UnloadBank(bank.ShortName);
    //        }
    //    }

    //    public void LoadBank(string bankName)
    //    {
    //        AkSoundEngine.LoadBank(bankName, out var bankid);
    //        //AkSoundEngine.PrepareBank(AkPreparationType.Preparation_Load, bankName, AkBankContent.AkBankContent_StructureOnly);
    //    }

    //    public void UnloadBank(string bankName)
    //    {
    //        AkSoundEngine.UnloadBank(bankName, IntPtr.Zero);
    //        var banks = m_soundBankConfig.SoundBanksInfo.SoundBanks;
    //        foreach (var bank in banks)
    //        {
    //            if (bank.ShortName == bankName)
    //            {
    //                var temp = new List<uint>();
    //                foreach (var eve in bank.IncludedEvents)
    //                {
    //                    temp.Add(AkSoundEngine.GetIDFromString(eve.Name));
    //                }
    //                m_Cooler.CleanFromShelves(temp);
    //                UnloadEvents(temp);
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// 播放2D事件（不需要判定声源）
    //    /// </summary>
    //    /// <param name="eventName"></param>
    //    public void PlayEvent(string eventName)
    //    {
    //        PlayEvent(eventName, emitter);
    //    }


    //    /// <summary>
    //    /// 对目标gameobject播放事件 注意播放同名事件不会重新播放该事件 而是重复播放
    //    /// </summary>
    //    /// <param name="eventName"></param>
    //    /// <param name="go"></param>
    //    public void PlayEvent(string eventName, GameObject go)
    //    {
    //        if (!m_Cooler.IsOnShelf(AkSoundEngine.GetIDFromString(eventName)))
    //        {
    //            AkSoundEngine.PrepareEvent(AkPreparationType.Preparation_Load, new string[] { eventName }, 1);
    //        }
    //        AkSoundEngine.PostEvent(eventName, go, (uint)AkCallbackType.AK_EndOfEvent, DefaultEventCallBack, null);
    //        m_Cooler.SetInUse(AkSoundEngine.GetIDFromString(eventName));
    //    }




    //    /// <summary>
    //    /// 对目标gameobject终止开始的事件，如果该事件未开始或者已经结束则无效，注意停止事件会停止所有同名事件
    //    /// </summary>
    //    /// <param name="eventName"></param>
    //    /// <param name="gameobject"></param>
    //    public void UnloadEvents(List<uint> eventName)
    //    {
    //        AkSoundEngine.PrepareEvent(AkPreparationType.Preparation_Unload, eventName.ToArray(), (uint)eventName.Count);

    //    }
    //    /// <summary>
    //    /// 对目标gameobject设置RTPC
    //    /// </summary>
    //    /// <param name="parameterName"></param>
    //    /// <param name="value"></param>
    //    /// <param name="gameobject"></param>
    //    public void SetRTPCValue(string parameterName, float value, GameObject gameobject)
    //    {
    //        var result = AkSoundEngine.SetRTPCValue(parameterName, value, gameobject);
    //        if (result != AKRESULT.AK_Success)
    //        {
    //            CLogger.Error($"Wwise设置RTPC:{parameterName}为{value}失败");
    //        }
    //    }
    //    /// <summary>
    //    /// 设置全局RTPC
    //    /// </summary>
    //    /// <param name="parameterName"></param>
    //    /// <param name="value"></param>
    //    public void SetGlobalRTPCValue(string parameterName, float value)
    //    {
    //        var result = AkSoundEngine.SetRTPCValue(parameterName, value);
    //        if (result != AKRESULT.AK_Success)
    //        {
    //            CLogger.Error($"Wwise设置全局RTPC:{parameterName}为{value}失败");
    //        }
    //    }
    //    /// <summary>
    //    ///  设置全局state
    //    /// </summary>
    //    /// <param name="StateName"></param>
    //    /// <param name="stateValue"></param>
    //    public void SetStateValue(string StateName, string stateValue)
    //    {
    //        var result = AkSoundEngine.SetState(StateName, stateValue);
    //        if (result != AKRESULT.AK_Success)
    //        {
    //            CLogger.Error($"Wwise设置State:{StateName}为{stateValue}失败");
    //        }
    //    }

    //    /// <summary>
    //    /// 对目标gameobject设置switch
    //    /// </summary>
    //    /// <param name="SwitchName"></param>
    //    /// <param name="SwitchState"></param>
    //    /// <param name="gameobject"></param>
    //    public void SetSwitchValue(string SwitchName, string SwitchState, GameObject gameobject)
    //    {
    //        var result = AkSoundEngine.SetSwitch(SwitchName, SwitchState, gameobject);
    //        if (result != AKRESULT.AK_Success)
    //        {
    //            CLogger.Error($"Wwise设置Switch:{SwitchName}为{SwitchState}失败");
    //        }
    //    }

    //    /// <summary>
    //    /// 对目标gameobject播放事件，同时在对应的akcallbacktype时调用对应的callback
    //    /// </summary>
    //    /// <param name="eventName"></param>
    //    /// <param name="go"></param>
    //    /// <param name="callback"></param>
    //    /// <param name="index"></param>
    //    public void PlayEventWithCallback(string eventName, GameObject go, AkCallbackManager.EventCallback callback, int index = 0)
    //    {
    //        callback += DefaultEventCallBack;
    //        if (!m_Cooler.IsOnShelf(AkSoundEngine.GetIDFromString(eventName)))
    //        {
    //            AkSoundEngine.PrepareEvent(AkPreparationType.Preparation_Load, new string[] { eventName }, 1);
    //        }
    //        AkSoundEngine.PostEvent(eventName, go, (uint)AkCallbackType.AK_EndOfEvent, callback, null);
    //        m_Cooler.SetInUse(AkSoundEngine.GetIDFromString(eventName));
    //    }


    //    /// <summary>
    //    /// 默认回调函数
    //    /// </summary>
    //    /// <param name="cookie"></param>
    //    /// <param name="type"></param>
    //    /// <param name="info"></param>
    //    private void DefaultEventCallBack(object cookie, AkCallbackType type, AkCallbackInfo info)
    //    {
    //        if (type == AkCallbackType.AK_EndOfEvent)
    //        {
    //            var eventInfo = info as AkEventCallbackInfo;
    //            m_Cooler.ReleaseFromUse(eventInfo.eventID);
    //        }
    //    }

    //    /// <summary>
    //    /// 按钮声音统一接口
    //    /// </summary>
    //    /// <param name="button"></param>
    //    private void PlayButtonClickedEvent(XButton button)
    //    {
    //        var go = button.gameObject;
    //        var hf = go.GetComponentInParent<HotfixBehavior>(go);
    //        var className = hf.className;
    //        foreach (var field in hf.serializeData)
    //        {
    //            if (field.obj == button)
    //            {
    //                className += "=>";
    //                className += field.fieldName;
    //            }
    //        }
    //        var eventName = TableSoundEffect.Find(className, false);
    //        if (string.IsNullOrEmpty(eventName) || eventName == className)
    //        {
    //            PlayEvent(TableSoundEffectConfigs.Sfx_Default_Button[0]);
    //        }
    //        else
    //        {
    //            PlayEvent(eventName);
    //        }
    //    }


    //}


}
