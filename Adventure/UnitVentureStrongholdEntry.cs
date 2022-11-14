using System;
using UnityEngine;
using ZhFramework.Unity.UI;

namespace Game.Runtime
{
    public class UnitVentureStrongholdEntryData
    {
        public int id;
        public int funcId;
        public string name;
        public bool isUnlock;
    }

    public class UnitVentureStrongholdEntry : UIWidget
    {
        [Hotfix] private XButton m_Btn;
        [Hotfix] private GameObject m_Mask;

        public event Action<int> onStrongholdEntryClicked;

        private UnitVentureStrongholdEntryData m_Data;
        private UnitRedDot m_UnitRedDot;

        protected override void OnPreload(params object[] args)
        {
            base.OnPreload(args);
            m_UnitRedDot = CreateWidget<UnitRedDot>(Transform, false);
        }

        public override void SetData(params object[] args)
        {
            m_Data = args[0] as UnitVentureStrongholdEntryData;
            m_Btn.SetText(m_Data.name);
            m_Mask.SetActive(!m_Data.isUnlock);
        }

        protected override void BindUIEvents()
        {
            base.BindUIEvents();
            m_Btn.onClick.AddListener(OnStrongholdClicked);
        }

        protected override void UnBindUIEvents()
        {
            base.UnBindUIEvents();
            m_Btn.onClick.RemoveAllListeners();
        }

        private void OnStrongholdClicked()
        {
            if (!ConditionHelper.CheckSysFuncConditionTips(m_Data.funcId))
            {
                return;
            }

            onStrongholdEntryClicked?.Invoke(m_Data.id);
        }

        public void ShowRedDot(int count)
        {
            m_UnitRedDot.SetActive(count > 0);
        }

        public int GetStrongholdId()
        {
            return m_Data.id;
        }
    }
}