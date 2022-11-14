using Rotorz.Games.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ZhFramework.Engine.Utilities;
using UnityEditor.SceneManagement;
using Battle.Core;
using ZhFramework.Unity.Resource;
using Unity.Mathematics;
using System.Text;

namespace Battle.Level.Editor
{
    public partial class LevelEditor : EditorWindow
    {
        private string kTitle = "LevelEditor";
        private const float kMinHeight = 500f;
        private const float kMinWidth = 1200f;
        private const float kTopToolBarHeight = 18f;
        private const int mLevelCameraCount = 5;
        private readonly string kRootPath = $"{ResourceManager.kResRoot}/Configs/Levels";
        private LevelConfig mLevelConfig;
        private Vector2 mScrollPosition;
        private Vector2 mInspectorScrollPosition;
        private LevelEditorConfig mEditorConfig;

        private LevelModuleListAdaptor mModulesAdaptor ;
        //private LevelModuleListAdaptor mStorysAdpator;
        private LevelModuleListAdaptor mBattleAdaptor ;
        //private List<ModuleId> mStoryModules;
        private List<ModuleId> mBattleModules;
        private LevelModule mSelectedModule;
        private GameObject mGoCameraGroups;

        private Transform mTfLookAt;
        private Transform mTfFollow;
        private Transform mGoStageCamera;
        public static List<LevelModeDefaultData> LevelModeDict = new List<LevelModeDefaultData>();
        [MenuItem(LevelLanguage.kLevelEditorMenu, false)]
        private static void ShowLevelEditor()
        {
            if (LevelModeDict.Count == 0)
            {
                LevelModeDict.Add(new LevelModeDefaultData()
                {
                    Name = "普通",
                    MaxInputDuration = 60,
                });
                LevelModeDict.Add(new LevelModeDefaultData()
                {
                    Name = "解救",
                    MaxInputDuration = 60,
                });
                LevelModeDict.Add(new LevelModeDefaultData()
                {
                    Name = "CD增加",
                    MaxInputDuration = 60,
                });
                LevelModeDict.Add(new LevelModeDefaultData()
                {
                    Name = "限时B",
                    MaxInputDuration = 60,
                });
                LevelModeDict.Add(new LevelModeDefaultData()
                {
                    Name = "传递炸弹",
                    MaxInputDuration = 60,
                });
                LevelModeDict.Add(new LevelModeDefaultData()
                {
                    Name = "躲避弹幕",
                    MaxInputDuration = 60,
                });
            }
            var window = GetWindow<LevelEditor>();
            window.titleContent = new GUIContent(window.kTitle);
            window.Show();
        }

        private void OnEnable()
        {
            TableManager.Uninit();

            minSize = new Vector2(kMinWidth, kMinHeight);

            mEditorConfig = LevelEditorConfig.Load();
            
            //if (mEditorConfig.kWorkingPath != null)
            //    LoadLevelConfig(mEditorConfig.kWorkingPath);

        }

        private void OnGUI()
        {
            using (new GUIDisable(LevelDataEditor.IsShow))
            {
                HandleShorcuts();
                DrawTopToolBar();

                if (mLevelConfig == null)
                    return;

                DrawMultList();
            }
        }

        private void OnDestroy()
        {
            DestroyImmediate(mGoCameraGroups);
            LevelModuleDrawer.Dispose();
        }

        private void DrawTopToolBar()
        {
            using (new GUIHorizontal(EditorStyles.toolbar))
            {
                if (GUILayout.Button(LevelLanguage.kCreate, EditorStyles.toolbarButton))
                    OnFileMenuNewClick();
                if (GUILayout.Button(LevelLanguage.kLoad, EditorStyles.toolbarButton))
                    OnFileMenuLoadClick();
                if (GUILayout.Button(LevelLanguage.kSave, EditorStyles.toolbarButton))
                    SaveLevelConfig();
                if (GUILayout.Button(LevelLanguage.kSaveForNewFile, EditorStyles.toolbarButton))
                    OnSaveLevelConfigClick();
                if (GUILayout.Button(LevelLanguage.kCommit, EditorStyles.toolbarButton))
                    OnCommitClick();
                if(mLevelConfig != null)
                {
                    if(mLevelConfig.Data != null)
                    {
                        GUILayout.Label($"当前选中关卡模式： {LevelModeDict[(int)(mLevelConfig.Data.LevelMode)].Name}");
                    }

                }

                GUILayout.FlexibleSpace();


                if (GUILayout.Button(LevelLanguage.kServer, EditorStyles.toolbarButton))
                    OnServerClick();

                if (GUILayout.Button($"{LevelLanguage.kLevelCamera}{mLevelCameraIndex + 1}", EditorStyles.toolbarButton))
                    OnLevelCameraClick();

                if (GUILayout.Button(LevelLanguage.kLevelData, EditorStyles.toolbarButton))
                    OnLevelDataClick();
            }
        }




        private void DrawMultList()
        {
            using (new GUIHorizontal())
            {
                var columnWidth = GUILayout.Width(250);

                using (new GUIVertical(columnWidth))
                {
                    ReorderableListGUI.Title(LevelLanguage.kProcess);
                    ReorderableListGUI.ListField(mModulesAdaptor, ReorderableListFlags.HideAddButton | ReorderableListFlags.DisableContextMenu);
                }

                using (var scroll = new GUIScrollView(mScrollPosition))
                {
                    mScrollPosition = scroll.ScrollPosition;

                    using (new GUIVertical(columnWidth))
                    {
                        //ReorderableListGUI.Title(LevelLanguage.kStory);
                        //ReorderableListGUI.ListField(mStorysAdpator);

                        ReorderableListGUI.Title(LevelLanguage.kBattle);
                        ReorderableListGUI.ListField(mBattleAdaptor);
                    }
                }

                using (var scroll = new GUIScrollView(mInspectorScrollPosition))
                {
                    mInspectorScrollPosition = scroll.ScrollPosition;
                }

                columnWidth = GUILayout.Width(position.width - 250 * 2 - 30);
                using (new GUIVertical(columnWidth))
                {
                    if (mSelectedModule != null)
                        LevelModuleDrawer.DrawModule(mSelectedModule, mLevelConfig.Data);
                    else
                    {
                        LevelModuleDrawer.ClearCanvas();
                    }
                }
            }
        }

        private void OnCommitClick()
        {
            string path = kRootPath;
            string args = $"/command:commit /path:{path} /logmsg:{LevelLanguage.kCommitLog} /closeonend:1";
            Exec("TortoiseProc", args);
        }

        private void OnLevelDataClick()
        {
            if(mLevelConfig == null)
            {
                EditorUtility.DisplayDialog("还没加载关卡呢", "请先加载关卡配置文件", "知道了");
                return;
            }
            //if (mLevelConfig.Data == null)
            //    mLevelConfig.Data = new LevelData();
            LevelDataEditor.Show(mLevelConfig.Data);
        }

        public void SetLevelData(LevelDataBase data)
        {
            mLevelConfig = new LevelConfig()
            {
                Data = data,
                Configs = mLevelConfig.Configs,
                Process = mLevelConfig.Process
            };
        }

        private int mLevelCameraIndex = 0;
        private void OnLevelCameraClick()
        {
            ++mLevelCameraIndex;
            if (mLevelCameraIndex >= mLevelCameraCount)
                mLevelCameraIndex = 0;

            InitLevelCamera();
        }

        private void OnFileMenuNewClick()
        {
            var path = EditorUtility.SaveFilePanel(LevelLanguage.CreateConfig, kRootPath, "", "json");
            if (string.IsNullOrEmpty(path))
                return;

            SaveLevelConfig(path);
            LoadLevelConfig(path);
        }

        private void OnSaveLevelConfigClick()
        {
            var path = EditorUtility.SaveFilePanel(LevelLanguage.CreateConfig, kRootPath, "", "json");
            if (string.IsNullOrEmpty(path))
                return;

            SaveLevelConfig(path, true);
            LoadLevelConfig(path);
        }

        private void OnFileMenuLoadClick()
        {
            var path = EditorUtility.OpenFilePanel(LevelLanguage.LoadConfig, kRootPath, "json");
            if (string.IsNullOrEmpty(path))
                return;

            LoadLevelConfig(path);
        }

        private void LoadLevelConfig(string path)
        {
            if (path == null)
                return;
            mSelectedModule = null;
            mEditorConfig.kWorkingPath = path;
            var fileName = Path.GetFileName(path);
            kTitle = fileName; 
            titleContent = new GUIContent(kTitle);

            var bytes = File.ReadAllBytes(path);
            var configData = Encoding.UTF8.GetString(bytes);
            mLevelConfig = JsonUtils.ToObject<LevelConfig>(configData);

            if (mLevelConfig.Configs == null)
                mLevelConfig.Configs = new LevelModuleConfigs();

            if (mLevelConfig.Process == null)
            {
                mLevelConfig.Process = new LevelProcess();
                mLevelConfig.Process.Modules = new List<ModuleId>();
            }

            if (mLevelConfig.Data == null)
                mLevelConfig.Data = new LevelDataBase();

            if (!string.IsNullOrEmpty(mLevelConfig.Data .SceneName))
            {
                var scenePath = LevelDataDrawer.kSceneDir + mLevelConfig.Data.SceneName + LevelDataDrawer.kSceneSuffix;
                if (File.Exists(scenePath))
                {
                    if (mGoCameraGroups != null)
                        DestroyImmediate(mGoCameraGroups);
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                }
            }

            InitModuleList();
            InitListAdapters();

            var cameraGroups = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Art/LevelDesign/LevelCamera/LevelCameraGroups.prefab");
            mGoCameraGroups = GameObject.Instantiate(cameraGroups);
            mGoCameraGroups.hideFlags = HideFlags.DontSaveInEditor | HideFlags.NotEditable;
            mGoCameraGroups.transform.position = mLevelConfig.Data.FightPos;
            mGoCameraGroups.transform.rotation = Quaternion.Euler(new float3( mLevelConfig.Data.FightRot[0]));

            mTfLookAt = mGoCameraGroups.transform.Find("lookat");
            mTfFollow = mGoCameraGroups.transform.Find("follow");
            mGoStageCamera = mGoCameraGroups.transform.Find("OnstageCam");
        }

        private void SaveLevelConfig()
        {
            SaveLevelConfig(mEditorConfig.kWorkingPath);
        }

        private void SaveLevelConfig(string path, bool saveOther = false)
        {
            if (path == null)
                return;

            if (!saveOther)
            {
                if (!File.Exists(path))
                {
                    mLevelConfig = new LevelConfig();
                    mLevelConfig.Configs = new LevelModuleConfigs();
                    mLevelConfig.Process = new LevelProcess();
                    mLevelConfig.Process.Modules = new List<ModuleId>();
                }
                else
                {
                    mLevelConfig.Configs.CreateEnemyList.RemoveAll(e => !mBattleModules.Exists(v => v.Uid == e.Uid));
                    mLevelConfig.Configs.AddEnemyList.RemoveAll(e => !mBattleModules.Exists(v => v.Uid == e.Uid));
                    mLevelConfig.Configs.AddBuffList.RemoveAll(e => !mBattleModules.Exists(v => v.Uid == e.Uid));

                }
            }

            JsonFileHelper.Save(mLevelConfig, path);
        }

        private void SaveAll()
        {
            SaveLevelConfig();
            mEditorConfig.Save();
        }

        private void InitModuleList()
        {
            var config = mLevelConfig.Configs;
            //mStoryModules = new List<ModuleId>();
            mBattleModules = new List<ModuleId>();

            //if (config.StoryList == null)
            //    config.StoryList = new List<LevelStoryModule>();

            //foreach (var module in mLevelConfig.Configs.StoryList)
            //{
            //    var moduleId = new ModuleId(ELevelModuleType.Story, ELevelModuleId.Story, module.Uid, module.ConfigName);
            //    mStoryModules.Add(moduleId);
            //}

            if (config.CreateEnemyList == null)
                config.CreateEnemyList = new List<LevelCreateEnemyModule>();

            foreach (var module in mLevelConfig.Configs.CreateEnemyList)
            {
                var moduleId = new ModuleId(ELevelModuleType.Battle, ELevelModuleId.CreateEnemy, module.Uid, module.ConfigName);
                mBattleModules.Add(moduleId);
            }

            if (config.AddBuffList == null)
                config.AddBuffList = new List<LevelAddBuffModule>();

            foreach (var module in mLevelConfig.Configs.AddBuffList)
            {
                var moduleId = new ModuleId(ELevelModuleType.Battle, ELevelModuleId.AddBuff, module.Uid, module.ConfigName);
                mBattleModules.Add(moduleId);
            }

            if (config.AddEnemyList == null)
                config.AddEnemyList = new List<LevelAddEnemyModule>();

            foreach (var module in mLevelConfig.Configs.AddEnemyList)
            {
                var moduleId = new ModuleId(ELevelModuleType.Battle, ELevelModuleId.AddEnemy, module.Uid, module.ConfigName);
                mBattleModules.Add(moduleId);
            }


        }

        private void InitListAdapters()
        {
            mModulesAdaptor = new LevelModuleListAdaptor(mLevelConfig.Process.Modules, GetModuleName);
            mModulesAdaptor.CanDragToOtherList = false;
            mModulesAdaptor.OnModuleSelect = OnModuleSelect;

            //mStorysAdpator = new LevelModuleListAdaptor(mStoryModules, GetModuleName, mLevelConfig);
            //mStorysAdpator.AddModuleType<LevelStoryModule>();
            //mStorysAdpator.OnCreateModule = OnCreateModule;
            //mStorysAdpator.OnModuleSelect = OnModuleSelect;
            //mStorysAdpator.OnModuleDelete = OnModuleDelete;

            mBattleAdaptor = new LevelModuleListAdaptor(mBattleModules, GetModuleName, mLevelConfig);
            mBattleAdaptor.AddModuleType<LevelCreateEnemyModule>();
            mBattleAdaptor.AddModuleType<LevelAddEnemyModule>();
            mBattleAdaptor.AddModuleType<LevelAddBuffModule>();

            mBattleAdaptor.OnCreateModule = OnCreateModule;
            mBattleAdaptor.OnModuleSelect = OnModuleSelect;
            mBattleAdaptor.OnModuleDelete = OnModuleDelete;
        }

        private string GetModuleName(ModuleId moduleId)
        {
            var config = mLevelConfig.Configs;

            switch (moduleId.Id)
            {
                //case ELevelModuleId.Story:
                //    return config.StoryList.FirstOrDefault(v => v.Uid == moduleId.Uid).ConfigName;
                case ELevelModuleId.CreateEnemy:
                    return config.CreateEnemyList.FirstOrDefault(v => v.Uid == moduleId.Uid).ConfigName;
                case ELevelModuleId.AddEnemy:
                    return config.AddEnemyList.FirstOrDefault(v => v.Uid == moduleId.Uid).ConfigName;
                case ELevelModuleId.AddBuff:
                    return config.AddBuffList.FirstOrDefault(v => v.Uid == moduleId.Uid).ConfigName;

            }

            return "None";
        }

        private void OnCreateModule(Type t)
        {
            var config = mLevelConfig.Configs;
            var module = Activator.CreateInstance(t);
            //if (module is LevelStoryModule story)
            //{
            //    config.StoryList.Add(story);
            //    var moduleId = new ModuleId(ELevelModuleType.Story, ELevelModuleId.Story, story.Uid, story.ConfigName);
            //    mStoryModules.Add(moduleId);
            //}

            if (module is LevelCreateEnemyModule createEnemy)
            {
                config.CreateEnemyList.Add(createEnemy);
                var moduleId = new ModuleId(ELevelModuleType.Battle, ELevelModuleId.CreateEnemy, createEnemy.Uid, createEnemy.ConfigName);
                mBattleModules.Add(moduleId);
            }

            if (module is LevelAddEnemyModule addEnemy)
            {
                config.AddEnemyList.Add(addEnemy);
                var moduleId = new ModuleId(ELevelModuleType.Battle, ELevelModuleId.AddEnemy, addEnemy.Uid, addEnemy.ConfigName);
                mBattleModules.Add(moduleId);
            }

            if (module is LevelAddBuffModule addBuff)
            {
                config.AddBuffList.Add(addBuff);
                var moduleId = new ModuleId(ELevelModuleType.Battle, ELevelModuleId.AddBuff, addBuff.Uid, addBuff.ConfigName);
                mBattleModules.Add(moduleId);
            }

            //if (module is LevelBattlePrepareModule prepare)
            //{
            //    config.BattlePrepareList.Add(prepare);
            //    var moduleId = new ModuleId(ELevelModuleType.Battle, ELevelModuleId.BattlePrepare, prepare.Uid, prepare.ConfigName);
            //    mBattleModules.Add(moduleId);
            //}

        }

        private void OnModuleSelect(ModuleId moduleId)
        {
            switch (moduleId.Id)
            {
                default:
                    throw new InvalidCastException();
                //case ELevelModuleId.Story:
                //    mSelectedModule = mLevelConfig.Configs.StoryList.First(v => v.Uid == moduleId.Uid);
                //    break;
                case ELevelModuleId.CreateEnemy:
                    mSelectedModule = mLevelConfig.Configs.CreateEnemyList.First(v => v.Uid == moduleId.Uid);
                    break;
                case ELevelModuleId.AddEnemy:
                    mSelectedModule = mLevelConfig.Configs.AddEnemyList.First(v => v.Uid == moduleId.Uid);
                    break;
                case ELevelModuleId.AddBuff:
                    mSelectedModule = mLevelConfig.Configs.AddBuffList.First(v => v.Uid == moduleId.Uid);
                    break;

            }
        }

        private void OnModuleDelete(ModuleId moduleId)
        {
            //mLevelConfig.Configs.StoryList.RemoveAll(v => v.Uid == moduleId.Uid);
            mLevelConfig.Process.Modules.RemoveAll(v => v.Uid == moduleId.Uid);
        }

        private void InitLevelCamera()
        {
            bool isOnStage = mLevelCameraIndex == mLevelCameraCount - 1;
            mGoStageCamera.gameObject.SetActive(isOnStage);
            if (!isOnStage)
            {
                List<KeyValuePair<int, int>> looks = new List<KeyValuePair<int, int>>();
                looks.Add(new KeyValuePair<int, int>(1, 1));
                looks.Add(new KeyValuePair<int, int>(1, 6));
                looks.Add(new KeyValuePair<int, int>(6, 1));
                looks.Add(new KeyValuePair<int, int>(6, 6));

                var lookData = looks[mLevelCameraIndex];

                var bornData = ResourceManager.ReadText("BornPositionConfig.json");
                var bornConfig = JsonUtils.ToObject<BornPositionConfig>(bornData);
                var teamConfig = bornConfig.teams[0][bornConfig.teams.Length - 1];
                var selfPos = teamConfig.positions[lookData.Key - 1].position;
                var enemyPos = teamConfig.positions[lookData.Value - 1].position * new float3(1f, 1f, -1f);

                var selfOffset = math.mul(quaternion.Euler(math.radians(mLevelConfig.Data.FightRot[0])), selfPos);
                var followPos = mLevelConfig.Data.FightPos + selfOffset;
                var enemyOffset = math.mul(quaternion.Euler(math.radians(mLevelConfig.Data.FightRot[0])), enemyPos);
                var lookPos = mLevelConfig.Data.FightPos + enemyOffset;

                mTfFollow.transform.position = followPos + new float3(0f, 1f, 0f);
                mTfLookAt.transform.position = lookPos + new float3(0f, 1f, 0f);
            }
        }

        private void HandleShorcuts()
        {
            var evt = Event.current;
            if (evt.control && evt.keyCode == KeyCode.S)
            {
                evt.Use();
                SaveAll();
            }
        }

        private static bool Exec(string filename, string args = "", string workingDir = "")
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = filename;
            process.StartInfo.Arguments = args;
            if (!string.IsNullOrEmpty(workingDir))
                process.StartInfo.WorkingDirectory = workingDir;

            try
            {
                process.Start();
                if (process.StartInfo.RedirectStandardOutput && process.StartInfo.RedirectStandardError)
                {
                    process.BeginOutputReadLine();
                    Debug.LogError(process.StandardError.ReadToEnd());
                }
                else if (process.StartInfo.RedirectStandardOutput)
                {
                    string data = process.StandardOutput.ReadToEnd();
                    Debug.Log(data);
                }
                else if (process.StartInfo.RedirectStandardError)
                {
                    string data = process.StandardError.ReadToEnd();
                    Debug.LogError(data);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }

            process.WaitForExit();
            int exitCode = process.ExitCode;
            process.Close();
            return exitCode == 0;
        }
    }

}