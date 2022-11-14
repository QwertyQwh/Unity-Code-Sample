using Battle.Core;
using Battle.Table;
using Rotorz.Games.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Table.Structure;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using ZhFramework.Editor.Layout;
using ZhFramework.Engine.Utilities;
using ZhFramework.Unity.Resource;
using UnityObject = UnityEngine.Object;

namespace Battle.Level.Editor
{
    public static class LevelUtility
    {
        private static GUIStyle m_titleStyle;





        private static void DrawSingleParamInput(ELevelTriggerCondition condition, LevelConditionData data)
        {
            foreach (var lookup in LevelConfigHelper.ConditionLookups)
            {
                if (lookup.condition == condition)
                {
                    using (new GUIVertical(EditorStyles.helpBox))
                    {
                        using (new GUIHorizontal())
                        {
                            GUILayout.Label(lookup.title);
                        }

                        for (int i = 0; i < lookup.Args.Length; i++)
                        {
                            using (new GUIHorizontal())
                            {
                                GUILayout.Label(lookup.ArgsTitle[i]);
                                data.ConditionArgs[lookup.Args[i]] = EditorGUILayout.IntField(data.ConditionArgs[lookup.Args[i]]);
                            }
                        }


                    }


                }

            }
        }
        public static void DrawSubPlotPlayer(LevelConditionData data, ref Vector2 pos,  string title = "")
        {
            if (m_titleStyle == null)
            {
                m_titleStyle = new GUIStyle()
                {
                    //fontStyle = FontStyle.Normal,

                };
            }

            if (LevelConfigHelper.ConditionLookups == null)
            {

            }
            //GUIVertical(EditorStyles.helpBox)
            //var window = EditorWindow.GetWindow<LevelDataEditor>();
            //var rect = new Rect(new Vector2(0, 300), new Vector2(window.position.size.x-10, 400));


            using (new GUIVertical(EditorStyles.helpBox))
            {
                using (var scroll = new GUIScrollView(pos))
                {
                    pos = scroll.ScrollPosition;

                    ReorderableListGUI.Title($"{title}{LevelLanguage.kTriggerTiming}");
                    using (new GUIVertical())
                    {
                        using (new GUIHorizontal())
                        {
                            GUILayout.Label(LevelLanguage.kTriggerTiming);
                            data.Condition = (ELevelTriggerCondition)EnumGUILayout.EnumFlagsField("", (Enum)data.Condition, GUILayout.Width(160f));
                        }

                        if (data.ConditionArgs == null)
                            data.ConditionArgs = new int[24];
                        foreach (var lookup in LevelConfigHelper.ConditionLookups)
                        {
                            if (data.Condition.HasFlag(lookup.condition))
                            {
                                DrawSingleParamInput(lookup.condition, data);
                            }
                        }
                    }
                }
            }

        }
    }
}