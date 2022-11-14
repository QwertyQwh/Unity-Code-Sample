using System.Collections.Generic;
using Table.Structure;
using UnityEngine;
using UnityEngine.UI;
using ZhFramework.Engine.Msic;
using ZhFramework.Unity.UI;
using Battle.Unity;
using System;

namespace Game.Runtime
{
    [UISettings(UILayer.Default,  addBackStack: false)]
    public class PageAdventurePromotionSign : UIPanel
    {

        [Hotfix] private XText m_TxtYes;
        [Hotfix] private XText m_TxtClear;
        [Hotfix] private XButton m_BtnYes;
        [Hotfix] private XButton m_BtnClear;
        [Hotfix] private XImage m_ImgSign;
        [Hotfix] private RectTransform m_RootSign;
        [Hotfix] private XImage m_ImgSignTip;

        public Action<Texture2D> OnConfirm;
        private Texture2D m_canvas;
        private Vector2 m_1StepBack = new Vector2(-1,-1);
        private Vector2 m_2StepsBack = new Vector2(-1,-1);
        private float m_1StepSpeed = 10;
        private float m_2StepSpeed = 10;
        private Color m_brushColor = new Color(152f/256f, 129f/256f, 86F/256f,1f);
        private const float m_maxBrushSize = 24;
        private const float m_minBrushSize = 8;
        private const float m_maxBrushAlpha = 1;
        private const float m_minBrushAlpha = 0.2f;
        private const float m_speedFactor = 20;//速度调节器 越大的话粗细对速度越敏感

        private bool m_started = false;
        protected override void OnPreload(params object[] args)
        {
            base.OnPreload(args);
            UIHelper.AttachUnitPageBack(this, TableSysPropertyText.AdventurePromotion_Title, "", OnCloseClicked).SetHelp(TableSysTipText.Tips_AdventurePromotion);
            m_TxtClear.text = TableSysPropertyText.AdventurePromotion_SignClear;
            m_TxtYes.text = TableSysPropertyText.AdventurePromotion_SignConfirm;
            if (m_canvas == null)
            {
                var size = m_RootSign.rect.size;
                m_canvas = new Texture2D((int)size.x, (int)size.y);
                ResetCanvas();
                m_ImgSign.sprite = Sprite.Create(m_canvas, new Rect(0, 0, m_canvas.width, m_canvas.height), new Vector2(0, 0));
            }

        }


        protected override void BindUIEvents()
        {
            base.BindUIEvents();
            m_BtnClear.onClick.AddListener(OnClearClicked);
            m_BtnYes.onClick.AddListener(OnYesClicked);
        }

        private void OnYesClicked()
        {
            if (!m_ImgSignTip.IsActive())
            {
                OnConfirm.Invoke(m_canvas);
                OnCloseClicked();
            }
            else
            {
                PageFloatingTips.ShowFloatingTip(TableSysPropertyText.AdventurePromotion_SignTips);
            }


        }

        private void OnClearClicked()
        {
            ResetCanvas();
        }

        protected override void UnBindUIEvents()
        {
            base.UnBindUIEvents();
            m_BtnClear.onClick.RemoveAllListeners();
            m_BtnYes.onClick.RemoveAllListeners();

        }
        private void OnCloseClicked()
        {
            GameURL.GlobalBack(this);
        }
        private void ResetCanvas()
        {
            for (int i = 0; i <= m_canvas.width; i++)
            {
                for (int j = 0; j <= m_canvas.height; j++)
                {
                        m_canvas.SetPixel(i,j, new Color(0,0,0,0));
                }
            }
            m_canvas.Apply();
            m_ImgSignTip.gameObject.SetActive(true);
        }

        protected override void Update()
        {
            base.Update();

            if (Input.GetMouseButton(0))
            {

                var curPos = ScreenToSignSpace(Input.mousePosition);

                if (m_1StepBack.x < 0)
                {
                    m_1StepBack = curPos;
                    return;
                }
                if (m_2StepsBack.x < 0)
                {
                    m_2StepsBack = m_1StepBack;
                    m_2StepSpeed = m_1StepSpeed / 2;
                    m_1StepBack = curPos;
                    m_1StepSpeed = (m_1StepBack - m_2StepsBack).magnitude;

                    return;
                }
                m_1StepBack = (m_2StepsBack + curPos+m_1StepBack) / 3.0f;
                var dist = curPos - m_1StepBack;
                var curSpeed = new Vector2(dist.x * m_canvas.width, dist.y * m_canvas.height).magnitude;
                m_1StepSpeed = (m_2StepSpeed + m_1StepSpeed+ curSpeed) / 3.0f;
                if (curPos.x<0 || curPos.x> 1 || curPos.y < 0 || curPos.y > 1)
                {
                    m_2StepsBack = new Vector2(-1, -1);
                    m_2StepSpeed = 20;
                    m_1StepBack = new Vector2(-1, -1);
                    m_1StepSpeed = 20;
                    return;
                }
                if (m_ImgSignTip.IsActive())
                    m_ImgSignTip.gameObject.SetActive(false);
                DrawLine(m_2StepsBack, m_1StepBack, m_2StepSpeed, m_1StepSpeed);
                m_2StepsBack = m_1StepBack;
                m_2StepSpeed = m_1StepSpeed;
                m_1StepBack = curPos;
                m_1StepSpeed = curSpeed;
            }
            else
            {
                m_2StepsBack = new Vector2(-1, -1);
                m_2StepSpeed = 20;
                m_1StepBack = new Vector2(-1, -1);
                m_1StepSpeed = 20;
            }
            m_canvas.Apply();
        }


        private int GetInterpolationSteps(float speed) // use avg speed
        {
            return (int)(speed / GetBrushSize(speed) * 3)+2;
        }
        private Vector2 ScreenToSignSpace(Vector3 pos)
        {
            var center = m_ImgSign.transform.position;
            var localCoord = pos - center;
            var size = UIManager.Instance.GetPixelSize(m_RootSign.rect.size);
            localCoord.x /= size.x;
            localCoord.y /= size.y;
            return (Vector2)localCoord + new Vector2(0.5f, 0.5f);
        }
        private void DrawLine(Vector2 start, Vector2 end, float startSpeed, float endSpeed)
        {
            var steps = GetInterpolationSteps((startSpeed + endSpeed) / 2);
            for (int i = 0; i < steps; i++)
            {
                var t = ((float)i) / steps;
                var lerped = Vector2.Lerp(start, end, t);
                DrawPoint(lerped, GetBrushSize(Mathf.Lerp(startSpeed, endSpeed, t)), GetBrushAlpha(Mathf.Lerp(startSpeed, endSpeed, t)));
            }
        }

        private void DrawPoint(Vector2 pos, float size, float alphaAdjust)
        {
            var newColor = m_brushColor;
            var bound = (int)size;
            for(int i = -bound; i<= bound; i++)
            {
                for(int j = -bound; j<=bound; j++)
                {
                    var portion = (i * i + j * j) / (size * size);
                    if (portion < 1)
                    {
                        var x = (int)(pos.x * m_canvas.width) + i;
                        var y = (int)(pos.y * m_canvas.height) + j;
                        if (x < 0 || x > m_canvas.width || y < 0 || y > m_canvas.height)
                        {
                            return;
                        }
                        var alpha = (1 - portion)*alphaAdjust;
                        var before = m_canvas.GetPixel(x,y);
                        if (before.a < alpha)
                        {
                            newColor.a = alpha;
                            m_canvas.SetPixel(x, y, newColor);
                        }
                    }
                }
            }

        }

        private float GetBrushSize(float speed)
        {
            return Mathf.Lerp(m_minBrushSize,m_maxBrushSize, 1 / ((speed / m_speedFactor)+1));
        }

        private float GetBrushAlpha(float speed)
        {
            return Mathf.Lerp(m_minBrushAlpha, m_maxBrushAlpha, 1 / ((speed / m_speedFactor) + 1));
        }
    }
}