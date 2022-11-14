using System;
using System.Collections.Generic;
using UnityEngine;
using UnityURP;
using ZhFramework.Engine;
using ZhFramework.Engine.Msic;
using ZhFramework.Unity.Sdk;

namespace Game.Runtime
{
    [Serializable]
    public class BattleSimulator : MonoBehaviour
    {
        //public int LevelId = 1001;

        //public EProcedure Procedure = EProcedure.Match;
        public string ConfigId = "";

        public int ServerId = 1;

        [SerializeField]
        public BattleSimulatorRoleDataList P1Roles = new BattleSimulatorRoleDataList();

        public BattleSimulatorRoleDataList P2Roles = new BattleSimulatorRoleDataList();

        private void Awake()
        {
            SDK.Init();
            GameStorage.Init();
            NetworkManager.Init();
            GameStorage.Enter($"{SDK.GetUserId()}");
            PlayerManager.CreateInstance();
            UnityURPManager.CreateInstance();
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            var setting = gameObject.AddComponent<LauncherSettings>();
            setting.IsSimulator = true;
            setting.Simulator = this;
            GameManager.CreateInstance();
        }

        private void Update()
        {
            if (GameManager.IsExisted)
            {
                GameManager.Instance.Update();
            }
            if (ModuleManager.IsExisted)
            {
                ModuleManager.Instance.Update();
            }

            Behavior.UpdateBehaviors();

            TimerPool.Update();
            GameStorage.Update();
            NetworkManager.Update();
            
            //Tutorial.TutorialManager.Update();
        }

        public static void Quit()
        {
            SDK.Uninit();
#if UNITY_EDITOR
            Dispose();
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(0);
#endif
        }

        private static void Dispose()
        {
            //WebViewManager.DestroyInstance();
            //Tutorial.TutorialManager.DestroyInstance();

            GameProcedure.Dispose();
            GameManager.DestroyInstance();
            GameStorage.Uninit();
            NetworkManager.Uninit();

            TableManager.Uninit();
            NetworkManager.OnDisconnect -= GameProcedure.OnDisconnect;

            //todo qinwh 复制粘贴要看清楚
            //#if !UNITY_EDITOR && DEBUG_LOG
            //            CLogger.OnFirstErrorCallback -= OnFirstError;
            //#endif
        }

        private void LateUpdate()
        {
            try
            {
                Behavior.LateUpdateBehaviors();
                GameManager.Instance?.LateUpdate();
            }
            catch (Exception e)
            {
                CLogger.Exception(e);
            }
        }
    }

    public enum EAITYPE
    {
        None,
        Automatic
    }

    [Serializable]
    public class BattleSimulatorRoleData
    {
        [HideInInspector] public string name;
        public int id;

        public int level;

        public int star;

        [SerializeField]
        public List<BattleSimulatorSkillData> skills = new List<BattleSimulatorSkillData>();

        public BattleSimulatorRoleData(int level, int star)
        {
            this.level = level;
            this.star = star;
            this.skills = new List<BattleSimulatorSkillData>();
            this.id = 0;
            this.name = "角色";
        }
    }

    [Serializable]
    public class BattleSimulatorRoleDataList
    {
        //public EAITYPE AI = EAITYPE.None;
        public List<BattleSimulatorRoleData> roles;

        //public List<>

        public List<BattleSimulatorSuperviseSkillData> SuperviseSkills;

        public BattleSimulatorRoleDataList()
        {
            roles = new List<BattleSimulatorRoleData>();
        }

        public void AddRole(BattleSimulatorRoleData data)
        {
            roles.Add(data);
        }
    }

    [Serializable]
    public class BattleSimulatorSkillData
    {
        [HideInInspector]
        public string Name;

        [HideInInspector]
        public int id;

        public int level;
    }

    [Serializable]
    public class BattleSimulatorSuperviseSkillData
    {
        [HideInInspector]
        public string Name;

        public int Id;
    }
}