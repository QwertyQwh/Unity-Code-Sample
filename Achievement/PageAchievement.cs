using System.Collections.Generic;
using System.Threading.Tasks;
using Table.Structure;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using ZhFramework.Engine.Msic;
using ZhFramework.Unity.Extensions;
using ZhFramework.Unity.Resource;
using ZhFramework.Unity.UI;

namespace Game.Runtime
{

    [UISettings(UILayer.Default, mode: UIMutexMode.HideOther, addBackStack: true)]
    public class PageAchievement : UIPanel
    {
        #region Feilds
        [Hotfix] private RectTransform m_RootTabs;
        [Hotfix] private RectTransform m_TabPoolRoot;
        [Hotfix] private RectTransform m_RootFourSubTab;
        [Hotfix] private RectTransform m_RootFourSubTab_1;
        [Hotfix] private RectTransform m_RootFourSubTab_2;
        [Hotfix] private RectTransform m_RootFourSubTab_3;
        [Hotfix] private RectTransform m_RootFourSubTab_4;
        [Hotfix] private RectTransform m_RootThreeSubTab;
        [Hotfix] private RectTransform m_RootThreeSubTab_1;
        [Hotfix] private RectTransform m_RootThreeSubTab_2;
        [Hotfix] private RectTransform m_RootThreeSubTab_3;
        [Hotfix] private RectTransform m_RootTwoSubTab;
        [Hotfix] private RectTransform m_RootTwoSubTab_1;
        [Hotfix] private RectTransform m_RootTwoSubTab_2;
        [Hotfix] private XText m_TxtTitle;
        [Hotfix] private XImage m_ImgBg;

        private UIPoolList<UnitAchievementTab, VoTabData> m_TabList;
        private VoTabData[] m_VoData;
        private Dictionary<int,List<int>> m_TabData = new Dictionary<int, List<int>>();
        private RectTransform[] m_FourRoots;
        private RectTransform[] m_ThreeRoots;
        private RectTransform[] m_TwoRoots;
        private List<UnitAchievementIcon> m_FourIcons = new List<UnitAchievementIcon>();
        private List<UnitAchievementIcon> m_ThreeIcons = new List<UnitAchievementIcon>();
        private List<UnitAchievementIcon> m_TwoIcons = new List<UnitAchievementIcon>();
        private RectTransform[] m_roots;
        #endregion Feilds



        #region Methods

        protected override void OnPreload(params object[] args)
        {
            base.OnPreload(args);
            InitTabs();
            m_ImgBg.SetSpriteSafe(TableGameAsset.Achievement_Bg);
            UIHelper.AttachUnitPageBack(this, TableSysPropertyText.Achievement_Title, "", GlobalBack).SetHelp(TableSysTipText.Tips_Achievement);
            InitIcons();

        }
        private void OnCloseClicked()
        {
            GameURL.GlobalBack();
        }
        private void InitIcons()
        {
            m_FourRoots = new RectTransform[] { m_RootFourSubTab_1, m_RootFourSubTab_2, m_RootFourSubTab_3, m_RootFourSubTab_4 };
            m_ThreeRoots = new RectTransform[] { m_RootThreeSubTab_1, m_RootThreeSubTab_2, m_RootThreeSubTab_3 };
            m_TwoRoots = new RectTransform[] { m_RootTwoSubTab_1, m_RootTwoSubTab_2 };
            for (int i =0; i< m_FourRoots.Length; i++)
            {
                m_FourIcons.Add(CreateWidget<UnitAchievementIcon>(m_FourRoots[i], true));
            }
            for (int i = 0; i < m_ThreeRoots.Length; i++)
            {
                m_ThreeIcons.Add(CreateWidget<UnitAchievementIcon>(m_ThreeRoots[i], true));
            }
            for (int i = 0; i < m_TwoRoots.Length; i++)
            {
                m_TwoIcons.Add(CreateWidget<UnitAchievementIcon>(m_TwoRoots[i], true));
            }
            m_roots = new RectTransform[] { m_RootTwoSubTab, m_RootThreeSubTab, m_RootFourSubTab };
            m_RootFourSubTab.SetActive(false);
            m_RootThreeSubTab.SetActive(false);
            m_RootTwoSubTab.SetActive(false);

        }

        private void SetTabRootsActive(int tab)
        {
            for(int i =0;i< m_roots.Length; i++)
            {
                if (i == tab - 2)
                    m_roots[i].SetActive(true);
                else
                    m_roots[i].SetActive(false);
            }
        }

        private void InitTabs()
        {
            m_TabList = new UIPoolList<UnitAchievementTab, VoTabData>(m_TabPoolRoot, m_RootTabs, true);
            var ptr = TableAchievementTab.GetEnumerator();
            while (ptr.MoveNext())
            {
                var data = TableAchievementTab.Find(ptr.Current.Key);
                if (!m_TabData.ContainsKey(data.Type))
                {
                    m_TabData[data.Type] = new List<int>();
                    m_TabData[data.Type].Add(data.SubType);
                }
                else
                    m_TabData[data.Type].Add(data.SubType);
            }
            ptr.Dispose();
            m_VoData = new VoTabData[m_TabData.Count];
            for (int i = 0; i < m_TabData.Count; i++)
            {
                int id = i+1;
                m_VoData[i] = new VoTabData()
                {
                    Name = UIHelper.GetAchievementTabTitle(i + 1),
                    ID = id,
                    RedDotId = ModuleManager.Instance.Get<AchievementModule>().FirstRedDotIds[id],
                };
            }
            m_TabList.SetData(m_VoData);
            for(int i =0; i<m_VoData.Length; i++)
            {
                m_TabList[i].SetRedDot();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ModuleManager.Instance.Get<AchievementModule>().PageAchievementLastSelectedIndex = m_TabList.selectedIndex;
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            AchievementModule.RequestAchievementInfo();
            var module = ModuleManager.Instance.Get<AchievementModule>();
            module.UpdateRedDots();
            m_TabList.selectedIndex = module.PageAchievementLastSelectedIndex;
        }
        protected override void BindUIEvents()
        {
            base.BindUIEvents();
            m_TabList.OnSelected.AddListener(OnTabListClicked);
        }
        protected override void UnBindUIEvents()
        {
            base.UnBindUIEvents();
            m_TabList.OnSelected.RemoveAllListeners();
        }
        private void OnTabListClicked(int index, VoTabData data)
        {
            m_TxtTitle.text = data.Name;
            var subtabs = m_TabData[data.ID];
            switch (subtabs.Count)
            {
                case 2:
                    for(int i =0; i< subtabs.Count; i++)
                    {
                        m_TwoIcons[i].SetData(subtabs[i],index+1);
                        m_TwoIcons[i].SetRedDot(ModuleManager.Instance.Get<AchievementModule>().SecondaryRedDotIds[subtabs[i]]);
                    }
                    SetTabRootsActive(2);
                    break;
                case 3:
                    for (int i = 0; i < subtabs.Count; i++)
                    {
                        m_ThreeIcons[i].SetData(subtabs[i], index + 1);
                        m_ThreeIcons[i].SetRedDot(ModuleManager.Instance.Get<AchievementModule>().SecondaryRedDotIds[subtabs[i]]);
                    }
                    SetTabRootsActive(3);

                    break;
                case 4:
                    for (int i = 0; i < subtabs.Count; i++)
                    {
                        m_FourIcons[i].SetData(subtabs[i], index + 1);
                        m_FourIcons[i].SetRedDot(ModuleManager.Instance.Get<AchievementModule>().SecondaryRedDotIds[subtabs[i]]);
                    }
                    SetTabRootsActive(4);

                    break;
            }
        }




        #endregion Methods
    }
}