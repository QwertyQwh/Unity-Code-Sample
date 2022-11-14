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
using static Battle.Level.Editor.LevelDataDrawer;
using Object = UnityEngine.Object;
using UnityObject = UnityEngine.Object;

namespace Battle.Level.Editor
{
    public class LevelModuleDrawer
    {
        private static LevelEnemyListAdaptor sEnemyListAdpator;
        private static GUILayoutOption kPropertyWidth = GUILayout.Width(167);
        private static GUILayoutOption kHalfWidth = GUILayout.Width(83);
        private static GUILayoutOption kBoolWidth = GUILayout.Width(14);
        private static BornPositionConfig sBornConfig;
        private static List<GameObject> sObjectPool;
        private static List<GameObject> sPosMarkers;
        private const string kJsonPath = "BornPositionConfig.json";
        private static Vector2 mCreateEnemySubPlotConditionPosition;
        private static Vector2 mAddEnemyConditionPosition;
        private static Vector2 mAddEnemyPosition;
        private static Vector2 mAddEnemySubplotConditionPosition;
        private static Vector2 mAddBuffConditionPosition;
        private static string kMarkerRoot = "Assets/_Art/LevelDesign/Editor/";
        public static void Dispose()
        {
            sObjectPool?.ForEach(v => UnityObject.DestroyImmediate(v));
            sObjectPool?.Clear();
            sPosMarkers?.ForEach(v => UnityObject.DestroyImmediate(v));
            sPosMarkers?.Clear();
            sObjectPool = null;
            sPosMarkers = null;
            sBornConfig = null;
            sEnemyListAdpator?.Clear();
            sEnemyListAdpator = null;
        }

        public static void DrawModule(LevelModule module, LevelDataBase levelData)
        {
            switch (module)
            {
                default:
                    DrawTitle(module);
                    DrawModuleName(module);
                    DrawProperties(module);
                    break;
                case LevelCreateEnemyModule m:
                    DrawCreateEnemys(m, levelData);
                    break;
                case LevelAddEnemyModule m:
                    DrawAddEnemys(m);
                    break;
                case LevelAddBuffModule m:
                    DrawAddBuff(m);
                    break;
            }
        }


        public static void ClearCanvas()
        {
            using (new GUIHorizontal())
            {
                EditorGUILayout.HelpBox("先选择模块", MessageType.Info);
            }
        }
        private static void DrawCreateEnemys(LevelCreateEnemyModule module, LevelDataBase levelData)
        {
            if (module.Enemys == null)
                module.Enemys = new List<LevelEnemyInfo>();
            if (sEnemyListAdpator == null || sEnemyListAdpator.List != module.Enemys)
                sEnemyListAdpator = new LevelEnemyListAdaptor(module.Enemys);

            ReorderableListGUI.Title(LevelLanguage.kCreateEnemyModuleTitle);
            DrawModuleName(module);

            module.wave = EditorGUILayout.IntField($"波次", module.wave);
            using (new GUIHorizontal())
            {
                module.EnemyBattleConfigId = EditorGUILayout.IntField($"{LevelLanguage.kEnemy}{LevelLanguage.kBattlePositionConfig}Id", module.EnemyBattleConfigId);
                module.EnemyBattlePosCount = EditorGUILayout.IntField(LevelLanguage.kEnemyBattlePosCount, module.EnemyBattlePosCount);

            }
            using (new GUIHorizontal())
            {
                module.AllyBattleConfigId = EditorGUILayout.IntField($"{LevelLanguage.kAlly}{LevelLanguage.kBattlePositionConfig}Id", module.AllyBattleConfigId);
                module.AllyBattlePosCount = EditorGUILayout.IntField(LevelLanguage.kAllyBattlePosCount, module.AllyBattlePosCount);

            }

            module.BattleCameraId = EditorGUILayout.IntField(LevelLanguage.kBattleCameraId, module.BattleCameraId);
            using (new GUIHorizontal())
            {
                GUILayout.Label(LevelLanguage.kDisableSkillCamera);
                module.DisableSkillCamera = EditorGUILayout.Toggle(module.DisableSkillCamera, kBoolWidth);
            }

            if (sBornConfig == null)
            {
                var bornData = ResourceManager.ReadText(kJsonPath);
                sBornConfig = JsonUtils.ToObject<BornPositionConfig>(bornData);
            }

            if (sPosMarkers == null)
                sPosMarkers = new List<GameObject>();
            //if(sPosMarkers.Count != module.BattlePosCount)
            {
                sPosMarkers.ForEach(v => UnityObject.DestroyImmediate(v));
                sPosMarkers.Clear();

                //var mode = TableLevelMode.Find(levelData.LevelMode);

                //for (int i = 0; i < module.BattlePosCount; ++i)
                //{
                //    var marker = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets\\_Art\\LevelDesign\\Editor\\PosMarker_{i + 1}.prefab");
                //    var go = UnityObject.Instantiate(marker);
                //    go.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
                //    sPosMarkers.Add(go);

                //    var config = sBornConfig.teams[mode.EnemyTeamStand - 1][module.BattlePosCount - 1];
                //    var position = config.positions[i];

                //    float3 finalPos = position.position + levelData.FightPos + (float3)go.transform.position;
                //    float3 finalRot = position.rotation + levelData.FightRot + (float3)go.transform.rotation.eulerAngles;
                //    finalRot += new float3(0f, 180f, 0f);

                //    go.transform.position = finalPos;
                //    go.transform.rotation = Quaternion.Euler(finalRot);
                //}

                SceneView.RepaintAll();
            }
            EditorGUILayout.LabelField("敌人列表");

            ReorderableListGUI.ListField(sEnemyListAdpator);

            if (module.LocalPlot == null)
            {
                module.LocalPlot = new List<LocalPlotData>();

            }


            using (new GUIHorizontal())
            {
                var originalLength = module.LocalPlot.Count;
                var length = EditorGUILayout.IntField("波次剧情数量", originalLength);

                if (length > originalLength)
                {
                    for (int i = 0; i < length - originalLength; i++)
                    {

                        module.LocalPlot.Add(new LocalPlotData());
                    }
                }
                else
                {
                    for (int i = 0; i < originalLength - length; i++)
                    {
                        module.LocalPlot.RemoveAt(length);
                    }

                }


            }
            foreach(var localPlot in module.LocalPlot)
            {
                if (localPlot.ConditionData == null)
                {
                    localPlot.ConditionData = new LevelConditionData();
                }
                LevelUtility.DrawSubPlotPlayer(localPlot.ConditionData, ref mCreateEnemySubPlotConditionPosition, LevelLanguage.kLocalPlot);
                if (localPlot.ConditionData.Condition != 0)
                {
                    using (new GUIVertical(EditorStyles.helpBox))
                    {

                        ReorderableListGUI.Title($"{LevelLanguage.kLocalPlot}{LevelLanguage.kConfig}");
                        using (new GUIHorizontal())
                        {
                            GUILayout.Label(LevelLanguage.kRes);
                            localPlot.PlotRes = EditorGUILayout.TextField(localPlot.PlotRes);
                        }


                        using (new GUIHorizontal())
                        {
                            GUILayout.Label(LevelLanguage.kTeam);
                            localPlot.Team = (ETeam)EditorGUILayout.Popup((int)localPlot.Team, new string[] { "我方", "敌方" });
                        }
                        using (new GUIHorizontal())
                        {
                            GUILayout.Label(LevelLanguage.kPositionId);
                            localPlot.PositionId = EditorGUILayout.IntField(localPlot.PositionId);
                        }
                    }
                }
            }




            if (GUILayout.Button(LevelLanguage.kPreview))
            {
                if (module.EnemyBattlePosCount < module.Enemys.Count)
                    EditorUtility.DisplayDialog(LevelLanguage.kWarning, LevelLanguage.kBornPosLessThanEnemyCount, LevelLanguage.kConfirm);

                if (module.Enemys.Count(v => v.BornPos < 1 || v.BornPos > module.EnemyBattlePosCount) > 0)
                {
                    EditorUtility.DisplayDialog(LevelLanguage.kWarning, LevelLanguage.kInvalidBornPos, LevelLanguage.kConfirm);
                    return;
                }

                for (int i = 0; i < module.Enemys.Count; ++i)
                {
                    if (module.Enemys.Count(v => v.BornPos == i + 1 && v.IsAlly == module.Enemys[i].IsAlly) > 1)
                    {
                        EditorUtility.DisplayDialog(LevelLanguage.kWarning, LevelLanguage.kRepeatedBornPos, LevelLanguage.kConfirm);
                        return;
                    }
                }







                int enemyCount = module.Enemys.Count;

                if (sObjectPool == null)
                    sObjectPool = new List<GameObject>();
                sObjectPool.ForEach(v => UnityObject.DestroyImmediate(v));
                sObjectPool.Clear();

                for (int i = 0; i < module.Enemys.Count; ++i)
                {
                    var enemyInfo = module.Enemys[i];
                    if (enemyInfo.IsAlly)
                    {
                        continue;
                    }
                    var enemyId = enemyInfo.RoleId;
                    var cardId = TableMonster.Find(enemyId).Card_ID;
                    var roleData = TableRole.Find(cardId);
                    var model = TableModel.Find(roleData.ViewResId);

                    var config = sBornConfig.teams[module.EnemyBattleConfigId-1][module.EnemyBattlePosCount - 1];
                    var position = config.positions[enemyInfo.BornPos - 1];

                    var obj = ResourceManager.LoadAsset<GameObject>($"Battle_{model.Res}");
                    var go = UnityObject.Instantiate(obj);
                    go.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;

                    go.transform.position = new float3(-position.position.x, position.position.y, -position.position.z) + levelData.FightPos;
                    go.transform.rotation = Quaternion.Euler(new float3(position.rotation.x, position.rotation.y+180, position.rotation.z));
                    go.transform.RotateAround(levelData.FightPos, Vector3.up, levelData.FightRot[0]);


                    sObjectPool.Add(go);


                }

                for (int i = 0; i < module.AllyBattlePosCount; i++)
                {
                    var config = sBornConfig.teams[module.AllyBattleConfigId-1][module.AllyBattlePosCount - 1];
                    var position = config.positions[i];
                    var allyobj = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath($"{kMarkerRoot}PosMarker_{i + 1}.prefab", typeof(GameObject));
                    var offset = 0;
                    if (module.Enemys.Count > i)
                    {
                        foreach(var enemy in module.Enemys)
                        {
                            if (enemy.IsAlly&& enemy.BornPos == i+1)
                            {
                                var enemyId = enemy.RoleId;
                                var cardId = TableMonster.Find(enemyId).Card_ID;
                                var roleData = TableRole.Find(cardId);
                                var model = TableModel.Find(roleData.ViewResId);
                                allyobj = ResourceManager.LoadAsset<GameObject>($"Battle_{model.Res}");
                                offset = 90;
                            }
                        }

                    }


                    var go = Object.Instantiate<GameObject>(allyobj);
                    go.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;

                    go.transform.position = position.position + levelData.FightPos+new float3(0,0.2f,0);
                    go.transform.rotation = Quaternion.Euler(new float3(position.rotation.x+90-offset, position.rotation.y, position.rotation.z));
                    go.transform.RotateAround(levelData.FightPos, Vector3.up, levelData.FightRot[0]);
                    sObjectPool.Add(go);
                }

            }
        }

        private static void DrawAddEnemys(LevelAddEnemyModule module)
        {
            using (var scroll = new GUIScrollView(mAddEnemyPosition))
            {
                ReorderableListGUI.Title(LevelLanguage.kAddEnemys);
                DrawModuleName(module);
                module.wave = EditorGUILayout.IntField($"波次", module.wave);
                if (module.ConditionData == null)
                {
                    module.ConditionData = new LevelConditionData();
                }
                using (new GUIVertical(EditorStyles.helpBox))
                {
                    LevelUtility.DrawSubPlotPlayer(module.ConditionData, ref mAddEnemyConditionPosition, LevelLanguage.kAddEnemys);
                    EditorGUILayout.Space();

                    string roleName = "";


                    if (module.MonsterId > 0)
                        roleName = TableMonster.Find(module.MonsterId).Name;

                    EditorGUILayout.Space();
                    module.MonsterId = EditorGUILayout.DelayedIntField($"{LevelLanguage.kMonsterId} {roleName}", module.MonsterId);

                    if (module.MonsterId > 0)
                    {
                        var monster = TableMonster.Find(module.MonsterId);
                        if (module.Restraint == ERestraint.None)
                            module.Restraint = (ERestraint)TableRoleAttribute.Find(monster.Card_ID).Restraint;

                        Color color = Color.red;
                        switch (module.Restraint)
                        {
                            case ERestraint.Order:
                                color = Color.green;
                                break;
                            case ERestraint.Freedom:
                                color = Color.blue;
                                break;
                        }
                        using (new GUIColor(color, true))
                        {
                            module.Restraint = (ERestraint)EnumGUILayout.EnumPopup("", module.Restraint, kHalfWidth);
                        }
                    }

                    module.BornPosId = EditorGUILayout.DelayedIntField(LevelLanguage.kBornPosId, module.BornPosId);
                    module.BornPosId = math.clamp(module.BornPosId, 1, 6);

                    using (new GUIHorizontal())
                    {
                        GUILayout.Label(LevelLanguage.kShowStageOn);
                        module.ShowStageOn = EditorGUILayout.Toggle(module.ShowStageOn, kBoolWidth);
                    }
                    using (new GUIHorizontal())
                    {
                        GUILayout.Label("我方");
                        module.isAlly = EditorGUILayout.Toggle(module.isAlly, kBoolWidth);
                    }
                    EditorGUILayout.Space();
                    module.ExecuteTimes = EditorGUILayout.IntField(LevelLanguage.kExecuteTimes, module.ExecuteTimes);
                    module.ExecuteTimes = math.clamp(module.ExecuteTimes, 1, int.MaxValue);

                }
                if (module.LocalPlot == null)
                {
                    module.LocalPlot = new List<LocalPlotData>();

                }

                using (new GUIHorizontal())
                {
                    var originalLength = module.LocalPlot.Count;
                    var length = EditorGUILayout.IntField("波次剧情数量", originalLength);

                    if (length > originalLength)
                    {
                        for (int i = 0; i < length - originalLength; i++)
                        {

                            module.LocalPlot.Add(new LocalPlotData());
                        }
                    }
                    else
                    {
                        for (int i = 0; i < originalLength - length; i++)
                        {
                            module.LocalPlot.RemoveAt(length);
                        }

                    }


                }
                foreach (var plot in module.LocalPlot)
                {
                    if (plot.ConditionData == null)
                    {
                        plot.ConditionData = new LevelConditionData();
                    }
                    LevelUtility.DrawSubPlotPlayer(plot.ConditionData, ref mAddEnemySubplotConditionPosition, LevelLanguage.kLocalPlot);
                    if (plot.ConditionData.Condition != 0)
                    {
                        using (new GUIVertical(EditorStyles.helpBox))
                        {

                            ReorderableListGUI.Title($"{LevelLanguage.kLocalPlot}{LevelLanguage.kConfig}");
                            using (new GUIHorizontal())
                            {
                                GUILayout.Label(LevelLanguage.kRes);
                                plot.PlotRes = EditorGUILayout.TextField(plot.PlotRes);
                            }


                            using (new GUIHorizontal())
                            {
                                GUILayout.Label(LevelLanguage.kTeam);
                                plot.Team = (ETeam)EditorGUILayout.Popup((int)plot.Team, new string[] { "我方", "敌方" });
                            }
                            using (new GUIHorizontal())
                            {
                                GUILayout.Label(LevelLanguage.kPositionId);
                                plot.PositionId = EditorGUILayout.IntField(plot.PositionId);
                            }
                        }
                    }
                }
            }
  
        }


        private static void DrawAddBuff(LevelAddBuffModule module)
        {
            ReorderableListGUI.Title(LevelLanguage.kAddBuffs);
            DrawModuleName(module);
            module.wave = EditorGUILayout.IntField($"波次", module.wave);
            using (new GUIVertical(EditorStyles.helpBox))
            {
                using (new GUIHorizontal())
                {

                    if(module.ConditionData == null)
                    {
                        module.ConditionData = new LevelConditionData();
                    }
                    LevelUtility.DrawSubPlotPlayer(module.ConditionData, ref mAddBuffConditionPosition, LevelLanguage.kAddBuffs);
                }
                module.BuffId = EditorGUILayout.IntField(LevelLanguage.kBuffId, module.BuffId);
                module.BuffId = math.clamp(module.BuffId, 1, int.MaxValue);
                module.BuffDescr = EditorGUILayout.TextField(LevelLanguage.kBuffDescr, module.BuffDescr);

                if (!string.IsNullOrWhiteSpace(module.BuffDescr))
                {
                    var descr = TableBuffText.Find(module.BuffDescr, false);
                    if (descr != null)
                    {
                        using (new GUIHorizontal())
                        {
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.LabelField(descr, GUILayout.Width(50));
                        }
                    }
                }

                using (new GUIHorizontal())
                {
                    GUILayout.Label(LevelLanguage.kExecuteTimes);
                    module.TriggerTimes = (ELevelTriggerTimes)EnumGUILayout.EnumPopup("", module.TriggerTimes, kHalfWidth);
                    bool isInfinity = module.TriggerTimes == ELevelTriggerTimes.Infinity;
                    if (isInfinity)
                        module.ExecuteTimes = 0;

                    using (new GUIDisable(isInfinity))
                    {
                        module.ExecuteTimes = EditorGUILayout.IntField(module.ExecuteTimes, kHalfWidth);
                    }
                }


            }

        }



        private static void DrawModuleName(LevelModule module)
        {
            using (new GUIVertical(EditorStyles.helpBox))
            {
                module.ConfigName = EditorGUILayout.TextField(LevelLanguage.kModuleName, module.ConfigName);
            }
        }

        private static void DrawTitle(LevelModule module)
        {
            var type = module.GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            Dictionary<string, FieldInfo> modules = new Dictionary<string, FieldInfo>();

            if (HasAttribute<DescriptionAttribute>(type))
            {
                ReorderableListGUI.Title($"{GetDescription(type)}");
            }
        }

        private static void DrawProperties(object action)
        {
            EditorGUI.BeginChangeCheck();

            var type = action.GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            Dictionary<string, FieldInfo> modules = new Dictionary<string, FieldInfo>();

            using (new GUIVertical(EditorStyles.helpBox))
            {
                foreach (var field in fields)
                {
                    var descr = GetDescription(field);
                    if (descr == null)
                        continue;

                    if (field.FieldType == typeof(string))
                    {
                        string value = (string)field.GetValue(action);
                        var assetType = GetAssetType(field);
                        Type unityAssetType = GetUnityAssetType(assetType);

                        UnityObject obj = null;

                        if (!string.IsNullOrEmpty(value))
                        {
                            switch (assetType)
                            {
                                default:
                                    obj = ResourceManager.LoadAsset<GameObject>(value);
                                    break;
                                case EAssetType.Audio:
                                    obj = ResourceManager.LoadAsset<AudioClip>(value);
                                    break;
                                case EAssetType.Text:
                                    obj = ResourceManager.LoadAsset<TextAsset>(value);
                                    break;
                            }
                        }

                        using (new GUIHorizontal())
                        {
                            GUILayout.Label(descr);
                            obj = EditorGUILayout.ObjectField(obj, unityAssetType, false, kPropertyWidth);
                            if (obj && AssetDatabase.GetAssetPath(obj).Contains(ResourceManager.kResRoot))
                            {
                                var fullPath = AssetDatabase.GetAssetPath(obj);
                                field.SetValue(action, Path.GetFileName(fullPath));
                            }
                        }
                    }
                    else if (field.FieldType.IsClass)
                    {
                        var value = field.GetValue(action);
                        if (value == null && HasAttribute<RequiredAttribute>(field))
                        {
                            value = Activator.CreateInstance(field.FieldType);
                            field.SetValue(action, value);
                        }
                        if (value != null)
                            modules.Add(descr, field);
                        continue;
                    }
                    else if (field.FieldType == typeof(float3))
                    {
                        float3 value = Float3Field(descr, (float3)field.GetValue(action));
                        field.SetValue(action, value);
                    }
                    else if (field.FieldType.IsEnum)
                    {
                        using (new GUIHorizontal())
                        {
                            GUILayout.Label(descr);
                            Enum value;

                            if (HasAttribute<FlagsAttribute>(field.FieldType))
                                value = EnumGUILayout.EnumFlagsField("", (Enum)field.GetValue(action), kPropertyWidth);
                            else
                                value = EnumGUILayout.EnumPopup("", (Enum)field.GetValue(action), kPropertyWidth);
                            field.SetValue(action, value);
                        }
                    }
                    else if (field.FieldType == typeof(bool))
                    {
                        bool value = (bool)field.GetValue(action);

                        using (new GUIHorizontal())
                        {
                            GUILayout.Label(descr);
                            value = EditorGUILayout.Toggle(value, kBoolWidth);
                            field.SetValue(action, value);
                        }
                    }
                    else if (field.FieldType == typeof(int))
                    {
                        int value = (int)field.GetValue(action);

                        using (new GUIHorizontal())
                        {
                            value = EditorGUILayout.IntField(descr, value);

                            var range = GetIntRange(field);
                            if (range != null)
                                value = math.clamp(value, range.min, range.max);

                            field.SetValue(action, value);
                        }
                    }
                    else if (field.FieldType == typeof(float))
                    {
                        float value = (float)field.GetValue(action);

                        using (new GUIHorizontal())
                        {
                            value = EditorGUILayout.FloatField(descr, value);

                            var range = GetFloatRange(field);
                            if (range != null)
                                value = math.clamp(value, range.min, range.max);

                            field.SetValue(action, value);
                        }
                    }
                    else if (field.FieldType == typeof(float3x3))
                    {
                        float3x3 values = (float3x3)field.GetValue(action);

                        EditorGUI.BeginChangeCheck();
                        for (int i = 0; i < 3; ++i)
                        {
                            values[i] = Float3Field("", values[i]);
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            field.SetValue(action, values);
                            SceneView.RepaintAll();
                        }
                    }
                }
            }

            foreach (var module in modules)
            {
                using (new GUIVertical(EditorStyles.helpBox))
                {
                    var field = module.Value;
                    var value = field.GetValue(action);

                    using (new GUIHorizontal())
                    {
                        bool mfoldout = true;
                        mfoldout = GUILayout.Toggle(mfoldout, module.Key, EditorStyle.foldout);
                        GUILayout.FlexibleSpace();
                    }
                    DrawProperties(value);
                }
            }

            if (EditorGUI.EndChangeCheck())
                SceneView.RepaintAll();
        }


        protected static float3 Float3Field(string label, float3 vector)
        {
            float3 result;
            using (new GUIHorizontal())
            {
                GUILayout.Label(label);
                result = EditorGUILayout.Vector3Field("", vector, GUILayout.Width(150f));
            }
            return result;
        }

        private static string GetDescription(FieldInfo field)
        {
            var descriptions = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (descriptions.Length == 0)
                return null;
            return (descriptions[0] as DescriptionAttribute).Description;
        }

        private static string GetDescription(Type t)
        {
            var descriptions = t.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (descriptions.Length == 0)
                return null;
            return (descriptions[0] as DescriptionAttribute).Description;
        }

        private static bool HasAttribute<T>(FieldInfo field) where T : Attribute
        {
            var attributes = field.GetCustomAttributes(typeof(T), false);
            return attributes.Length > 0;
        }

        private static bool HasAttribute<T>(Type t) where T : Attribute
        {
            return t.GetCustomAttributes(typeof(T), false).Length > 0;
        }

        private static IntRangeAttribute GetIntRange(FieldInfo field)
        {
            var attributes = field.GetCustomAttributes(typeof(IntRangeAttribute), false);
            if (attributes.Length == 0)
                return null;
            return attributes[0] as IntRangeAttribute;
        }

        private static FloatRangeAttribute GetFloatRange(FieldInfo field)
        {
            var attributes = field.GetCustomAttributes(typeof(FloatRangeAttribute), false);
            if (attributes.Length == 0)
                return null;
            return attributes[0] as FloatRangeAttribute;
        }

        private static EAssetType GetAssetType(FieldInfo field)
        {
            var attributes = field.GetCustomAttributes(typeof(AssetAttribute), false);
            if (attributes.Length == 0)
                return EAssetType.Prefab;
            return (attributes[0] as AssetAttribute).type;
        }

        private static Type GetUnityAssetType(EAssetType assetType)
        {
            switch (assetType)
            {
                default:
                    return typeof(GameObject);
                case EAssetType.Audio:
                    return typeof(AudioClip);
                case EAssetType.Text:
                    return typeof(TextAsset);
            }
        }
    }
}
