using System.Collections.Generic;
using Table.Structure;
using UnityEngine;
using UnityEngine.UI;
using ZhFramework.Engine;
using ZhFramework.Unity.UI;
using System;

namespace Game.Runtime
{
    [UISettings(UILayer.Default, UIContext.Default | UIContext.DisableBlank, mode: UIMutexMode.HideOther, addBackStack: true)]
    public class PageAdventurePromotion : UIPanel
    {
        [Hotfix] private RectTransform m_RootClosed;
        [Hotfix] private RectTransform m_RootOpen;
        [Hotfix] private XButton m_BtnSign;
        [Hotfix] private XImage m_ImgSign;
        [Hotfix] private XText m_TxtLevel;
        [Hotfix] private XText m_TxtLevelTitle;
        [Hotfix] private XText m_TxtNameTitle;
        [Hotfix] private XText m_TxtRegistTitle;
        [Hotfix] private XText m_TxtTimeLapseTitle;
        [Hotfix] private XText m_TxtName;
        [Hotfix] private XText m_TxtRegist;
        [Hotfix] private XText m_TxtTimeLapse;
        [Hotfix] private XImage m_ImgMedal;
        [Hotfix] private XImage m_ImgLevel;
        [Hotfix] private XText m_TxtDesc;
        [Hotfix] private XText m_TxtSignTip;
        [Hotfix]private XButton m_BtnOpen;
        [Hotfix]private XImage m_ImgOpen;
        public Action OnSignClosed;

        protected override void OnPreload(params object[] args)
        {
            base.OnPreload(args);
            var data = TableAdventurerLevel.Find(AdventureManager.Instance.AdventurerData.adventurerLv+1);
            m_ImgMedal.SetSpriteSafe(data.GradeIcon);
            m_ImgLevel.SetSpriteSafe(data.GradeTextIcon);
            m_TxtLevel.text = data.GetAdventurerGradeDescribe();
            m_TxtName.text = PlayerManager.Instance.Role.RoleName;
            var createTimeStamp = PlayerManager.Instance.Role.CreateTime;
            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            var createTime = startTime.AddMilliseconds(createTimeStamp);
            m_TxtTimeLapse.text =  string.Format(TableSysPropertyText.AdventurePromotion_DaysLapsed,((ServerTime.CurrTime - createTime).Hours/24 +1).ToString());
            m_TxtRegist.text = $"{createTime.Year}/{createTime.Month}/{createTime.Day}";
            SetOpen(false);
            m_TxtDesc.text = TableSysPropertyText.AdventurePromotion_SignDesc;
            m_ImgOpen.SetSpriteSafe(TableGameAsset.AdventurePromotion_IconOpen);
            UIHelper.AttachUnitPageBack(this, TableSysPropertyText.AdventurePromotion_Title, "", OnCloseClicked).SetHelp(TableSysTipText.Tips_AdventurePromotion);
            m_ImgSign.color = new Color(0, 0, 0, 0);
                m_TxtSignTip.gameObject.SetActive(true);
            m_TxtLevelTitle.text = TableSysPropertyText.AdventurePromotion_LevelTitle;
            m_TxtNameTitle.text = TableSysPropertyText.AdventurePromotion_NameTitle;
            m_TxtRegistTitle.text = TableSysPropertyText.AdventurePromotion_RegistTitle;
            m_TxtTimeLapseTitle.text = TableSysPropertyText.AdventurePromotion_TimeLapseTitle;
            
        }

        private void OnCloseClicked()
        {
            if (m_BtnSign.enabled)
            {
                GlobalBack();
            }
            else
            {
                OnSignClosed?.Invoke();
                GlobalBack();
            }
        }

        protected override void BindUIEvents()
        {
            base.BindUIEvents();
            m_BtnOpen.onClick.AddListener(OnBtnOpenClicked);
            m_BtnSign.onClick.AddListener(OnSignClicked);
        }

        private void OnSignClicked()
        {
            UIManager.Instance.CreatePanel<PageAdventurePromotionSign>().OnConfirm += OnDisplayTexture;
            
        }

        private void OnDisplayTexture(Texture2D obj)
        {
            m_ImgSign.sprite = Sprite.Create(obj, new Rect(0, 0, obj.width, obj.height), new Vector2(0, 0));
            m_ImgSign.color = new Color(1, 1, 1, 1);
            m_BtnSign.enabled = false;
            m_TxtSignTip.gameObject.SetActive(false);
        }

        private void OnBtnOpenClicked()
        {
            SetOpen(true);
        }

        protected override void UnBindUIEvents()
        {
            base.UnBindUIEvents();
            m_BtnOpen.onClick.RemoveAllListeners();
            m_BtnSign.onClick.RemoveAllListeners();

        }

        private void SetOpen(bool stat)
        {
            m_RootClosed.gameObject.SetActive(!stat);
            m_RootOpen.gameObject.SetActive(stat);
        }

    }
}