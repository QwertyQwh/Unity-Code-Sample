using System;
using System.Threading.Tasks;
using UnityEngine;
using Table.Structure;
using ZhFramework.Unity.UI;
using System.Collections.Generic;

namespace Game.Runtime
{
    [UISettings(UILayer.Floating)]
    public class UnitAchievementReached : UIPanel
    {
        [Hotfix]
        protected XText m_TextTitle;

        [Hotfix] private XImage m_ImgIcon;

        private static WeakReference<UnitAchievementReached> s_Reference;
        private Queue<AchievementInfo> m_Queue = new Queue<AchievementInfo>();
        private bool m_IsPlaying = false;
        private static UnitAchievementReached Instance
        {
            get
            {
                if (null == s_Reference || !s_Reference.TryGetTarget(out var waiting))
                {
                    waiting = UIManager.Instance.CreatePanel<UnitAchievementReached>();
                    s_Reference = new WeakReference<UnitAchievementReached>(waiting);
                }

                return waiting;
            }
        }

        public static void Cleanup()
        {
            if (null != s_Reference && s_Reference.TryGetTarget(out var waiting))
            {
                waiting.Close();
                s_Reference = null;
            }
        }

        public static void ShowFloatingTip(AchievementInfo info)
        {
            if (Instance.m_IsPlaying)
            {
                Instance.m_Queue.Enqueue(info);
                return;
            }
            else
            {
                Instance.m_ImgIcon.SetSpriteSafe(UIHelper.GetAchievementCompleteIcon(TableExtensions.GetAchievementTabGroupFromAchievementId(info.Id).First));
                Instance.m_TextTitle.text = TableAchievementText.Find(info.Data.NameKey);
            }
            ShowInternal();
            Wait();
        }


        public static async void Wait()
        {
            Instance.m_IsPlaying = true;
            await Task.Delay(2000);
            Instance.m_IsPlaying = false;
            if (Instance.m_Queue.Count == 0)
            {
                Instance.SetActive(false);
            }
            else
            {
                ShowFloatingTip(Instance.m_Queue.Dequeue());
            }

        }
        private static void ShowInternal()
        {
            Instance.SetActive(true);
            Instance.SetAsLastSibling();
        }




        protected override void OnDestroy()
        {
            base.OnDestroy();
            s_Reference = null;
        }
    }
}