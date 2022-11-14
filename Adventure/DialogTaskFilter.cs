using System;
using Table.Structure;
using UnityEngine.UI;

#pragma warning disable 649

namespace Game.Runtime
{


    [UISettings(UILayer.Floating, UIContext.Blur | UIContext.Popup | UIContext.Tips)]
    class DialogTaskFilter : CommonDialog_S
    {
        [Hotfix]
        private ScrollRect m_ScrollRect;



        private UnitFilterGroup m_QualityFilterGroup;


        private Action m_OnOk;

        public int QualityFilter => m_QualityFilterGroup.Value;

        public event Action OnOk
        {
            add => m_OnOk += value;
            remove => m_OnOk -= value;
        }

        protected override void OnPreload(params object[] args)
        {
                        var qualityFilter = (int) args[0];
                        var maxUnlockedLevel = (int)args[1];
                        //任务难度
                        m_QualityFilterGroup = CreateWidget<UnitFilterGroup>(m_ScrollRect.content, true,
                                                                                   "Adventure_quality_",
                                                                                   TableGlobalNum.MaxAdventureQualityCount,
                                                                                   TableSysPropertyText.AdventureQuality_Title,
                                                                                   qualityFilter,
                                                                                   false,maxUnlockedLevel);

                        m_ScrollRect.verticalNormalizedPosition = 1;


        }

        protected override void BindUIEvents()
        {
            m_ConfirmBtn.onClick.AddListener(OnOK);
            m_CloseBtn.onClick.AddListener(Close);
        }

        protected override void UnBindUIEvents()
        {
            m_ConfirmBtn.onClick.RemoveListener(OnOK);
            m_CloseBtn.onClick.RemoveListener(Close);
        }

        private void OnOK()
        {

            m_OnOk?.Invoke();
            Close();
        }


        protected override void OnDestroy()
        {


                m_QualityFilterGroup = null;
            m_OnOk = null;
        }

    }
}