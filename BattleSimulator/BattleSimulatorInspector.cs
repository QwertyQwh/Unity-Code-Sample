using UnityEngine;
using UnityEditor;
using Table.Structure;
using UnityEngine.UIElements;
using Game.Runtime;
using Battle.Unity;
using ZhFramework.Unity.Utilities;
using ZhFramework.Engine.Utilities;
using System.IO;

using BattleSimulatorRoleData = Game.Runtime.BattleSimulatorRoleData;
using System.Collections.Generic;

namespace Battle.Editor
{


    [ExecuteInEditMode]

    [UnityEditor.CustomEditor(typeof(BattleSimulator))]
    public class BattleSimulatorInspector : UnityEditor.Editor
    {
        //SerializedProperty m_P1;
        public string Errormsg;
        private bool error = false;
        private bool SetDataClicked = false;
        private bool FileExists = false;
        private bool FileExistsClicked = false;

        private bool LoadClicked = false;
        private bool LoadSucceed = true;
        public string loadFileName = "";
        public static string SaveLocation = "battleconfigs";
        private void OnEnable()
        {
            //m_P1 = serializedObject.FindProperty("m_P1");
        }


        private void CheckSuperviseSkills(List<BattleSimulatorSuperviseSkillData> skills, int p =1)
        {
            for(int i = 0; i< skills.Count; i++)
            {
                var skillData = TableExtensions.GetSkillData(skills[i].Id);
                if(skillData == null)
                {
                    skills[i].Name = "技能名字未找到";
                    LogError($" P{p}阵容 第{i+1}个督战技能未找到"); ;
                }
                else if(skillData.SuperviseSkillType == 0){
                    skills[i].Name = "技能不是督战技能";
                    LogError($" P{p}阵容 督战第{i+1}个技能不是督战技能"); ;
                }
                else
                {
                    skills[i].Name = TableExtensions.GetSkillName(skills[i].Id);
                }
            }
        }

        private void SetRoleData(BattleSimulatorRoleData role, bool isMax = false, int p = 1, int i = 0)
        {
            var dataAttr = TableExtensions.GetTableRoleAttributeData(role.id);
            if(dataAttr  == null)
            {
                role.name = "Id不正确,请检查";
                LogError($"P{p}阵容 第{i}个角色Id不正确");
                return;
            }
            role.name = TableExtensions.GetRoleName(role.id);
            if (string.IsNullOrEmpty(role.name))
            {
                role.name = "未找到";
                LogError($"P{p}阵容 第{i}个角色名字未找到");
                return;
            }
            if (isMax)
            {
                role.level = TableExtensions.GetRoleLevelMax();
                role.star = TableExtensions.GetRoleMaxStar();
            }
            else
            {
                role.level = 1;
                role.star = TableExtensions.GetRoleDefaultStar(role.id);
            }
            var skillCount = dataAttr.Skills.Length;
            role.skills.Clear();
            for(int j = 0; j< skillCount; j++)
            {
                role.skills.Add(new BattleSimulatorSkillData());
                role.skills[j].level = 1;
                role.skills[j].id = dataAttr.Skills[j];
                if (isMax)
                {
                    var skillData = TableExtensions.GetSkillData(role.skills[j].id);
                    var leveledSkill = TableExtensions.GetLeveledSkillFromGroupId(skillData.GroupId, 5);
                    role.skills[j].id = leveledSkill.Id;
                    role.skills[j].level = 5;
                }
                var skillName = TableExtensions.GetSkillName(dataAttr.Skills[j]);
                if (string.IsNullOrEmpty(skillName))
                {
                    role.skills[j].Name = "技能名字未找到";
                    LogError($" P{p}阵容 第{i}个角色 第{j+1}个技能未找到");;
                }
                else
                    role.skills[j].Name = skillName;

            }

        }


        private void CheckRoleData(BattleSimulatorRoleData role,int p = 1, int i = 0)
        {
            var data = TableExtensions.GetTableRoleData(role.id);
            if(data == null)
            {
                LogError($"P{p}阵容 第{i}个角色Id不正确");
                return;
            }
            if (role.name != TableExtensions.GetRoleName(role.id))
            {
                LogError($"P{p}阵容 第{i}个角色更改id后没有重新设定数据");
                return;
            }
            if (role.level > TableExtensions.GetRoleLevelMax() || role.level < 1)
            {
                LogError($"P{p}阵容 第{i}个角色等级不合法");
                return;
            }
            if(role.star > TableExtensions.GetRoleMaxStar() || role.star < 1)
            {
                LogError($"P{p}阵容 第{i}个角色星级不合法");
            }
            var skills = role.skills;
            for( int j = 0; j< skills.Count; j++)
            {
                if(skills[j].level <1 || skills[j].level > 5)
                {
                    skills[j].Name = "技能等级不正确";
                    LogError($"P{p}阵容 第{i}个角色第{j}个技能等级不正确");
                    continue;
                }
                var skillData = TableExtensions.GetSkillData(skills[j].id);
                if(skillData == null)
                {
                    skills[j].Name = "技能名字未找到";
                    LogError($"P{p}阵容 第{i}个角色第{j}个技能名字未找到");
                    continue;
                }
                var leveledSkill = TableExtensions.GetLeveledSkillFromGroupId(skillData.GroupId,skills[j].level);
                skills[j].Name = TableExtensions.GetSkillName(leveledSkill.Id);
                skills[j].id = leveledSkill.Id;
            }
        }


        private void LogError(string msg)
        {
            error = true;
            Errormsg += msg;
            Errormsg += '\n';
        }

        private void LoadConfig(string name, BattleSimulator target)
        {
            LoadClicked = true;
            LoadSucceed = true;
            var path = PathUtils.GetExternalPath(Path.Combine(SaveLocation, $"{name}.json"));
            if (!File.Exists(path))
            {
                LoadSucceed = false;
            }
            else
            {
                var simulator = JsonUtils.ToObject<BattleSimulator>(File.ReadAllText(path));
                SetDataFromConfig(target, simulator);
            }
        }

        private void SetDataFromConfig(BattleSimulator target, BattleSimulator config)
        {
            target.ConfigId = config.ConfigId;
            target.P1Roles = config.P1Roles;
            target.P2Roles = config.P2Roles;
        }
        private void CheckDataValidity(BattleSimulator setting)
        {
            ResetError();
            SetDataClicked = true;
            int len = setting.P1Roles.roles.Count;
            for (int i = 0; i < len; i++)
            {
                CheckRoleData(setting.P1Roles.roles[i], 1, i + 1);
            }
            CheckSuperviseSkills(setting.P1Roles.SuperviseSkills, 1);
            int len2 = setting.P2Roles.roles.Count;
            for (int i = 0; i < len2; i++)
            {
                CheckRoleData(setting.P2Roles.roles[i], 2, i + 1);
            }
            CheckSuperviseSkills(setting.P2Roles.SuperviseSkills, 2);
        }

        private void ShowSetDataMsg()
        {
            if (!error)
                EditorGUILayout.HelpBox("数据设置/检查成功！", MessageType.Info);
            else
                EditorGUILayout.HelpBox(Errormsg, MessageType.Error);
        }
        private void ShowSaveFileMsg()
        {
            if (!FileExists)
                EditorGUILayout.HelpBox("配置存储成功", MessageType.Info);
            else
                EditorGUILayout.HelpBox("已存在同名配置", MessageType.Error);
        }

        private void ShowLoadConfigMsg()
        {
            if (LoadSucceed)
                EditorGUILayout.HelpBox("配置读取成功", MessageType.Info);
            else
                EditorGUILayout.HelpBox("不存在该名字的配置文件", MessageType.Error);
        }
        private void SaveConfigFile(BattleSimulator simulator)
        {
            FileExists = false;
            FileExistsClicked = true;
            var json = JsonUtils.ToString(simulator);
            var path = PathUtils.GetExternalPath(Path.Combine(SaveLocation, $"{simulator.ConfigId}.json"));

            if (!File.Exists(path))
            {
                PathUtils.MakeDirectory(path);
                File.WriteAllText(path, json);
            }
            else
            {
                FileExists = true;
            }

        }

        private void SaveP1Config(BattleSimulator simulator)
        {
            var json = JsonUtils.ToString(simulator.P1Roles);
            var path = PathUtils.GetExternalPath(Path.Combine(SaveLocation, $"{simulator.ConfigId}_P1.json"));
            if (!File.Exists(path))
            {
                PathUtils.MakeDirectory(path);

            }
            File.WriteAllText(path, json);
        }

        private void ResetError()
        {
            error = false;
            Errormsg = "";
        }
        private void setDataFromId(BattleSimulator setting, bool max)
        {
            SetDataClicked = true;
            Errormsg = "";
            error = false;
            int len = setting.P1Roles.roles.Count;
            for (int i = 0; i < len; i++)
            {
                SetRoleData(setting.P1Roles.roles[i], max, 1, i + 1);
            }
            CheckSuperviseSkills(setting.P1Roles.SuperviseSkills, 1);
            int len2 = setting.P2Roles.roles.Count;
            for (int i = 0; i < len2; i++)
            {
                SetRoleData(setting.P2Roles.roles[i], max, 2, i + 1);
            }
            CheckSuperviseSkills(setting.P2Roles.SuperviseSkills, 2);

        }
        public override void OnInspectorGUI()
        {
            var setting = target as BattleSimulator;
            EditorGUILayout.HelpBox("要设定角色阵容，请手动输入角色数量，并依次输入角色id。然后点击“一键自动设定”按钮。", MessageType.Info);
            GUILayout.Label("配置文件名称");
            loadFileName = EditorGUILayout.DelayedTextField (loadFileName);
            if (GUILayout.Button("读取配置文件"))
            {
                LoadConfig(loadFileName,setting);
            }
            if (LoadClicked)
            {
                ShowLoadConfigMsg();
            }
            if (GUILayout.Button("存储当前战斗配置（请确认配置id输入无误）"))
            {
                SaveConfigFile(setting);
            }
            if (FileExistsClicked)
            {
                ShowSaveFileMsg();
            }
            base.OnInspectorGUI();
            //serializedObject.ApplyModifiedProperties();
            serializedObject.Update();


            //setting.m_roles = new System.Collections.Generic.List<Unity.RoleData>();
            if(GUILayout.Button( "一键自动设定(角色初始数据)"))
            {
                SetDataClicked = true;
                setDataFromId(setting,false);
            }
            if (GUILayout.Button("一键自动设定(角色满级数据)"))
            {
                SetDataClicked = true;
                setDataFromId(setting, true);
            }


            if (GUILayout.Button("设置并检查数据合法性"))
            {
                CheckDataValidity(setting);
            }
            if (SetDataClicked)
            {
                ShowSetDataMsg();

            }
            if (GUILayout.Button("数据检查无误，导出P1数据json"))
            {
                SaveP1Config(setting);

            }

            if (GUILayout.Button("准备完成，进入战斗吧！"))
            {
                EditorApplication.EnterPlaymode();

            }






            serializedObject.ApplyModifiedProperties();

        }
    }


}