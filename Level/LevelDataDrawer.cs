using System.IO;
using Unity.Mathematics;
using static Unity.Mathematics.quaternion;
using UnityEditor;
using UnityEngine;
using ZhFramework.Editor.Layout;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Battle.Table;
using Battle.Core;
using Rotorz.Games.Collections;
using Rotorz.Games.UnityEditorExtensions;

namespace Battle.Level.Editor
{
    internal class LevelDataDrawer
    {
        public static readonly string kSceneDir = "Assets/_Scenes/Game/Battle/";
        public static readonly string kSceneSuffix = ".unity";

        private readonly GUILayoutOption kPropertyWidth = GUILayout.Width(160f);
        private SceneView mSceneView;
        private string[] mLevelModeNames;
        private static Vector2 mInspectorScrollPosition;
        private static Vector2 mGlobalPlotScrollPosition;
        private static Vector2 mVictoryScrollPosition;

        internal virtual void OnPropertyGUI(Rect position, LevelDataBase data)
        {
            using (var scroll = new GUIScrollView(mInspectorScrollPosition))
            {
                mInspectorScrollPosition = scroll.ScrollPosition;
                // 场景名称
                using (new GUIVertical(EditorStyles.helpBox))
                {
                    using (new GUIHorizontal())
                    {
                        GUILayout.Label(LevelLanguage.kSceneName);
                        SceneAsset scene = null;
                        if (!string.IsNullOrEmpty(data.SceneName))
                            scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(kSceneDir + data.SceneName + kSceneSuffix);
                        scene = (SceneAsset)EditorGUILayout.ObjectField(scene, typeof(SceneAsset), false, kPropertyWidth);
                        if (scene != null)
                        {
                            string scenePath = kSceneDir + scene.name + kSceneSuffix;
                            if (File.Exists(scenePath))
                            {
                                data.SceneName = scene.name;
                                var curScene = SceneManager.GetActiveScene();
                                if (curScene.name != scene.name)
                                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                            }
                        }
                    }
                }
                int index = 0;
                // 关卡模式
                using (new GUIVertical(EditorStyles.helpBox))
                {
                    if (mLevelModeNames == null)
                    {
                        var modeNames = new List<string>() { };
                        foreach (var levelData in LevelEditor.LevelModeDict)
                        {
                            modeNames.Add(levelData.Name);
                        }
                        mLevelModeNames = modeNames.ToArray();
                    }

                    
                    index = (int)data.LevelMode;
                    data.LevelMode = (ELevelType)(EditorGUILayout.Popup(index, mLevelModeNames));
                    if (index != (int)data.LevelMode)
                    {
                        var window = EditorWindow. GetWindow<LevelDataEditor>();
                        window.Data = data;
                    }
                    
                    //var modeData = TableLevelMode.Find(data.LevelMode);

 
                    var levelModeData = LevelEditor.LevelModeDict[(int)data.LevelMode];
                    data.MaxInputDuration = EditorGUILayout.IntField(LevelLanguage.kInputDuration, data.MaxInputDuration);
                    data.MaxRound = EditorGUILayout.IntField("最大回合", data.MaxRound);
                    //using (new GUIDisable())
                    //{
                        EditorGUILayout.IntField(LevelLanguage.kPrepareDuration, 60);
                        EditorGUILayout.IntField(LevelLanguage.kTeamNum, 1);

                    //}
                }

                // 战斗位置
                using (new GUIVertical(EditorStyles.helpBox))
                {
                    data.FightPos = Float3Field(LevelLanguage.kFightPos, data.FightPos);
                    var newLength = EditorGUILayout.IntField("镜头角度数量",data.FightRot.Count);
                    if (newLength <= 0)
                    {
                        EditorUtility.DisplayDialog(LevelLanguage.kWarning, "镜头数量必须大于0", LevelLanguage.kConfirm);
                        newLength = data.FightRot.Count;
                    }
                    int dif = newLength - data.FightRot.Count;
                    while (dif > 0)
                    {
                        data.FightRot.Add(0);
                        dif--;
                    }
                    while (dif < 0)
                    {
                        data.FightRot.RemoveAt(data.FightRot.Count - 1);
                        dif++;
                    }
                    for(int i = 0; i<data.FightRot.Count; i++)
                    {
                        data.FightRot[i] = EditorGUILayout.FloatField($"{LevelLanguage.kFightRot}{i+1}", data.FightRot[i]);
                    }

                    data.WinPos = Float3Field(LevelLanguage.kWinPos, data.WinPos);
                    data.WinRot = EditorGUILayout.FloatField(LevelLanguage.kWinRot, data.WinRot);
                }

                //战斗胜利条件
                using (new GUIVertical(EditorStyles.helpBox))
                {

                    if(data.VictoryCondition == null)
                    {
                        data.VictoryCondition = new LevelConditionData();
                    }
                    LevelUtility.DrawSubPlotPlayer(data.VictoryCondition, ref mVictoryScrollPosition, LevelLanguage.kVictoryCondition);

                }




                //全局剧情


                if (data.plot == null)
                {
                    data.plot = new List<GlobalPlotData>();

                }
                using (new GUIHorizontal())
                {
                    var originalLength = data.plot.Count;
                    var length = EditorGUILayout.IntField("全局剧情数量", originalLength);

                    if (length > originalLength)
                    {
                        for (int i = 0; i < length - originalLength; i++)
                        {

                            data.plot.Add(new GlobalPlotData());
                        }
                    }
                    else
                    {
                        for (int i = 0; i < originalLength - length; i++)
                        {
                            data.plot.RemoveAt(length);
                        }

                    }


                }
                    foreach (var globalPlot in data.plot)
                {
                    using (new GUIVertical(EditorStyles.helpBox))
                    {
                        if (globalPlot.ConditionData == null)
                        {
                            globalPlot.ConditionData = new LevelConditionData();
                        }
                        LevelUtility.DrawSubPlotPlayer(globalPlot.ConditionData, ref mGlobalPlotScrollPosition, LevelLanguage.kGlobalPlot);

                        if (globalPlot.ConditionData.Condition != 0)
                        {
                            using (new GUIVertical(EditorStyles.helpBox))
                            {

                                ReorderableListGUI.Title($"{LevelLanguage.kGlobalPlot}{LevelLanguage.kConfig}");
                                using (new GUIHorizontal())
                                {
                                    GUILayout.Label(LevelLanguage.kRes);
                                    globalPlot.PlotRes = EditorGUILayout.TextField(globalPlot.PlotRes);
                                }
                            }
                        }

                    }


                }

            }
        }

        internal virtual void OnSceneGUI(SceneView sceneView, LevelDataBase data)
        {
            mSceneView = sceneView;
            DrawLocationAngleList(LevelLanguage.kFightPos, ref data.FightPos, ref data.FightRot);
            //DrawLocation(LevelLanguage.kPreparePos, ref data.PreparePos, ref data.PrepareRot);
            DrawLocationAngle(LevelLanguage.kWinPos, ref data.WinPos, ref data.WinRot);
        }

        #region Private Functions
        private void DrawLocationAngleList(string label, ref float3 position, ref List<float> rotation)
        {
            Handles.Label(position - new float3(0f, 1f, 0f), label, EditorStyle.sceneViewLabel);
            var quaternion = Euler(math.radians(new float3(0, rotation[0], 0)));
            if (Handles.Button(position, quaternion, 1, 1, Handles.ConeHandleCap))
                mSceneView.LookAt(position);

            switch (Tools.current)
            {
                case Tool.Move:
                    position = Handles.PositionHandle(position, quaternion);
                    EditorWindow.GetWindow<LevelDataEditor>().Repaint();
                    break;
                case Tool.Rotate:
                    rotation[0] = Handles.RotationHandle(quaternion, position).eulerAngles.y;
                    EditorWindow.GetWindow<LevelDataEditor>().Repaint();
                    break;
            }
        }

        private void DrawLocationAngle(string label, ref float3 position, ref float rotation)
        {
            Handles.Label(position - new float3(0f, 1f, 0f), label, EditorStyle.sceneViewLabel);
            var quaternion = Euler(math.radians(new float3(0,rotation,0)));
            if (Handles.Button(position, quaternion, 1, 1, Handles.ConeHandleCap))
                mSceneView.LookAt(position);

            switch (Tools.current)
            {
                case Tool.Move:
                    position = Handles.PositionHandle(position, quaternion);
                    EditorWindow.GetWindow<LevelDataEditor>().Repaint();
                    break;
                case Tool.Rotate:
                    rotation = Handles.RotationHandle(quaternion, position).eulerAngles.y;
                    EditorWindow.GetWindow<LevelDataEditor>().Repaint();
                    break;
            }
        }

        private float3 Float3Field(string label, float3 vector)
        {
            float3 result;
            using (new GUIHorizontal())
            {
                GUILayout.Label(label);
                result = EditorGUILayout.Vector3Field("", vector);
            }
            return result;
        }
        #endregion Private Functions




   
    }

    internal class CDIncreaseLevelDataDrawer : LevelDataDrawer
    {
        internal override void OnPropertyGUI(Rect position, LevelDataBase data)
        {
            base.OnPropertyGUI(position, data);
            using (new GUIVertical(EditorStyles.helpBox))
            {
                ReorderableListGUI.Title($"{LevelLanguage.kMode}{LevelLanguage.kConfig}");
                data.IncreasedCD = EditorGUILayout.IntField($"CD{LevelLanguage.kIncrease}", data.IncreasedCD);
            }


            
        }
    }

    internal class BombLevelDataDrawer : LevelDataDrawer
    {
        internal override void OnPropertyGUI(Rect position, LevelDataBase data)
        {
            base.OnPropertyGUI(position, data);
            using (new GUIVertical(EditorStyles.helpBox))
            {
                ReorderableListGUI.Title($"{LevelLanguage.kMode}{LevelLanguage.kConfig}");
               data.BUffID = EditorGUILayout.IntField($"BuffId", data.BUffID);
            }



        }
    }

    internal class LimitedTimeBLevelDataDrawer : LevelDataDrawer
    {
        internal override void OnPropertyGUI(Rect position, LevelDataBase data)
        {
            base.OnPropertyGUI(position, data);
            using (new GUIVertical(EditorStyles.helpBox))
            {
                ReorderableListGUI.Title($"{LevelLanguage.kMode}{LevelLanguage.kConfig}");
                data.TimeLimitB = EditorGUILayout.IntField($"{LevelLanguage.kTimeLimit}", data.TimeLimitB);
            }



        }
    }

}

