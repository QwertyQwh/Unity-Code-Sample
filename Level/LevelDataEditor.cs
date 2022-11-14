using Battle.Core;
using Battle.Table;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Battle.Level.Editor
{
    internal class LevelDataEditor : EditorWindow
    {
        public static bool IsShow = false;
        private const float kMinWidth = 500f;
        private const float kMinHeight = 400f;
        private LevelDataDrawer mDrawer = new LevelDataDrawer();


        private LevelDataBase mData;
        public  LevelDataBase Data
        {
            set
            {
                mData = value;
                //if (mData.LevelRating == null)
                //{
                //    mData.LevelRating = new LevelRating[3];
                //    for (int i = 0; i < mData.LevelRating.Length; ++i)
                //    {
                //        mData.LevelRating[i] = new LevelRating();
                //        mData.LevelRating[i].Args = new int[2];
                //    }
                //}
                switch (mData.LevelMode)
                {
                    case ELevelType.Normal:
                        mDrawer=new LevelDataDrawer();
                        break;
                    case ELevelType.CDIncrease:
                        mDrawer=new CDIncreaseLevelDataDrawer();
                        break;
                    case ELevelType.Bomb:
                        mDrawer = new BombLevelDataDrawer();
                        break;
                    case ELevelType.LimitedTimeB:
                        mDrawer = new LimitedTimeBLevelDataDrawer();
                        break;
                    default:
                        mDrawer=new LevelDataDrawer();
                        break;
                }
                


            }
        }

        public static void Show(LevelDataBase data)
        {
            IsShow = true;
            var window = GetWindow<LevelDataEditor>();
            window.titleContent = new GUIContent(LevelLanguage.kLevelData, EditorGUIUtility.IconContent("BuildSettings.SelectedIcon").image);
            window.Data = data;
            window.ShowPopup();
        }

        private void OnEnable()
        {
            minSize = new Vector2(kMinWidth, kMinHeight);
            SceneView.duringSceneGui += OnSceneGUI;

        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;

        }

        private void OnDestroy()
        {
            IsShow = false;
            var window = GetWindow<LevelEditor>();
            window.SetLevelData(mData);
        }

        private void OnGUI()
        {

               mDrawer.OnPropertyGUI(position, mData);

        }

        private void OnSceneGUI(SceneView sceneView)
        {

                mDrawer.OnSceneGUI(sceneView, mData);

        }
    }
}