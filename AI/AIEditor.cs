using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ZhFramework.Editor.Layout;
using ZhFramework.Engine.Utilities;
using ZhFramework.Unity.Utilities;

namespace Game.Editor.AI
{
    public sealed partial class AIEditor : EditorWindow
    {
        [MenuItem("BattleEditor/AI编辑器", false)]
        private static void OpenWindow()
        {
            var window = GetWindow<AIEditor>();
            window.titleContent = new GUIContent("AIEditor");
            window.minSize = new Vector2(800, 650);
        }

        private const float kPropertyMinWidth = 300;
        private const float kPropertyMaxWidth = 400;
        private const string kPrefsNodeLoadFile = "NodeLoadFile";
        private ZoomAreaLayout m_ZoomArea = new ZoomAreaLayout();

        private float m_PropertyWidth;
        private string m_FilePath;
        private Rect m_CanvasRect;
        private string m_EditorRootPath;

        private Vector2 m_Translation;
        private Vector2 m_Scale = Vector2.one;

        private void OnEnable()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnSaveAndDestroy;
            m_ZoomArea.Set(Vector2.zero, Vector2.one);
            if (string.IsNullOrEmpty(m_EditorRootPath))
            {
                var script = MonoScript.FromScriptableObject(this);
                var path = AssetDatabase.GetAssetPath(script);
                m_EditorRootPath = Path.GetDirectoryName(path);
            }

            Load();
        }

        private void OnDisable()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnSaveAndDestroy;
        }

        private void OnSaveAndDestroy()
        {
            OnDestroy();
        }

        private void OnDestroy()
        {
            Save(m_FilePath, true);
        }

        private void OnGUI()
        {
            var offset = Vector2.up * 21;
            var viewRect = new Rect(offset,
                                    position.size - offset + Vector2.left * m_PropertyWidth);

            using (new EditorGUI.DisabledScope(0 == m_Nodes.Count ))
            {
                m_ZoomArea.Rect = viewRect;
                m_ZoomArea.Set(m_Translation, m_Scale);
                m_ZoomArea.BeginViewGUI(rootVisualElement.layout.yMin);
                m_CanvasRect = m_ZoomArea.ShownArea;
                OnNodeCanvasGUI();
                m_ZoomArea.EndViewGUI();
                m_Translation = m_ZoomArea.Translation;
                m_Scale = m_ZoomArea.Scale;
            }

            if (0 == m_Nodes.Count)
            {
                var content = EditorGUIUtility.TrTempContent(NodeLanguages.pleaseNewFile);
                var size = NodeStyles.notificationBackground.CalcSize(content);
                var rt = new Rect(Vector2.zero, size) {center = viewRect.center};
                GUI.Label(rt, content, NodeStyles.notificationBackground);
            }


            OnTopToolbarGUI();
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                using (new GUILayout.HorizontalScope(GUILayout.Width(m_PropertyWidth)))
                {
                    GUILayout.Space(4);
                    using (new GUILayout.VerticalScope())
                    {
                        //EditorGUIUtility.labelWidth = 
                        OnNodePropertyGUI();
                        GUILayout.FlexibleSpace();
                    }
                }
            }

            var canvasWidth = viewRect.width;
            SplitterUtility.Resize(viewRect, ref canvasWidth,
                                   position.width - kPropertyMaxWidth,
                                   position.width - kPropertyMinWidth,
                                   true, false);
            m_PropertyWidth = position.width - canvasWidth;

            HandleKeyEvents();
        }

        private void HandleKeyEvents()
        {
            var e = Event.current;
            switch (e.type)
            {
                case EventType.KeyUp:
                    switch (e.keyCode)
                    {
                        case KeyCode.N:
                            if (e.control)
                            {
                                NewFile();
                                e.Use();
                            }
                            break;
                        case KeyCode.L:
                            if (e.control)
                            {
                                LoadFile();
                                e.Use();
                            }
                            break;
                        case KeyCode.S:
                            if (e.control & e.alt)
                            {
                                SaveAsFile();
                                e.Use();
                            }
                            else if (e.control)
                            {
                                SaveFile();
                                e.Use();
                            }
                            break;
                    }
                    break;
            }
        }

        private void OnTopToolbarGUI()
        {
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Space(4);
                var rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.toolbarDropDown,
                                                    GUILayout.Width(50));

                if (GUI.Button(rect, NodeLanguages.kFile, EditorStyles.toolbarDropDown))
                {
                    FileMenu(rect);
                }

                GUILayout.FlexibleSpace();
                GUILayout.Label(m_FilePath);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(NodeLanguages.clear, EditorStyles.toolbarButton))
                {
                    EditorPrefs.DeleteKey(kPrefsNodeLoadFile);
                }
            }
        }

        private void FileMenu(Rect rt)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent(NodeLanguages.newFile), false, NewFile);
            menu.AddItem(new GUIContent(NodeLanguages.loadFile), false, LoadFile);
            menu.AddItem(new GUIContent(NodeLanguages.saveFile), false, SaveFile);
            menu.AddItem(new GUIContent(NodeLanguages.saveAs), false, SaveAsFile);
            menu.DropDown(
                new Rect(
                    rt.min + Vector2.up * rt.height,
                    Vector2.zero));
        }

        private void Load()
        {
            var loadFile = EditorPrefs.GetString(kPrefsNodeLoadFile);
            if (File.Exists(loadFile))
            {
                Load(loadFile);
            }
        }

        private void Load(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            Debug.Log(path);
            m_FilePath = path;
            var text = File.ReadAllText(path);
            var data = JsonUtils.ToObject<AIData>(text);
            data.Attach();
            m_Nodes = data.nodes;
            m_Connections = data.Connections;
            m_Translation = data.translation;
            m_Scale = data.scale;
        }

        private AIData GetData()
        {
            var data = new AIData
                       {
                           scale = m_ZoomArea.Scale,
                           translation = m_ZoomArea.Translation
                       };
            data.Init(m_Nodes, m_Connections);
            return data;
        }

        private void Save(string path, bool confirm = false)
        {
            if (string.IsNullOrEmpty(path)) return;
            m_FilePath = path;
        

            var bytes = JsonUtils.ToString(GetData());

            if (confirm && File.Exists(path))
            {
                var text = File.ReadAllText(path);
                if (0 != string.CompareOrdinal(text, bytes))
                {
                    var result =
                        EditorUtility.DisplayDialog(NodeLanguages.tips, NodeLanguages.message, NodeLanguages.ok,
                                                    NodeLanguages.cancel);
                    if (!result) return;
                }
            }

            File.WriteAllText(path, bytes);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }


        private void LoadFile()
        {
            Load(GetLoadFilePath());
        }

        private string GetLoadFilePath()
        {
            var loadFile = EditorPrefs.GetString(kPrefsNodeLoadFile);
            var rootPath = string.IsNullOrEmpty(loadFile) ? string.Empty : PathUtils.GetParentFolderPath(loadFile);

            var path = EditorUtility.OpenFilePanelWithFilters("Load", rootPath, new[] {"ai files", "json"});

            if (string.IsNullOrEmpty(path)) return string.Empty;

            var dataRootPath = PathUtils.GetParentFolderPath(Application.dataPath);
            path = PathUtils.MakeRelativePath(dataRootPath, path);
            EditorPrefs.SetString(kPrefsNodeLoadFile, path);

            return path;
        }

        private string GetSaveFilePath()
        {
            var loadFile = EditorPrefs.GetString(kPrefsNodeLoadFile);
            var rootPath = string.IsNullOrEmpty(loadFile) ? string.Empty : PathUtils.GetParentFolderPath(loadFile);

            var path = EditorUtility.SaveFilePanel(
                "NewFile",
                rootPath,
                "NewFileName.json",
                "json");

            if (string.IsNullOrEmpty(path)) return string.Empty;

            EditorPrefs.SetString(kPrefsNodeLoadFile, path);
            return path;
        }


        private void SaveAsFile()
        {
            var path = GetSaveFilePath();
            if(string.IsNullOrEmpty(path)) return;
            Save(path);
        }

        private void SaveFile()
        {
            Save(string.IsNullOrEmpty(m_FilePath) ? GetSaveFilePath() : m_FilePath);
        }

        private void NewFile()
        {
            var path = GetSaveFilePath();
            if (string.IsNullOrEmpty(path)) return;
            m_FilePath = path;

            if (null == m_Nodes)
            {
                m_Nodes = new List<Node>();
            }
            else
            {
                m_Nodes.Clear();
            }

            if (null == m_Connections)
            {
                m_Connections = new List<Connection>();
            }
            else
            {
                m_Connections.Clear();
            }
            m_NodeSelection.Clear();
            m_ConnectionSelection.Clear();
            m_Translation = Vector2.zero;
            m_Scale = Vector2.one;
            var rootNode = new RootNode
                           {
                               position = Snap(new Vector2((position.width- m_PropertyWidth - kNodeWidth) /2, 50))
                           };
            AddNode(rootNode);

            SaveFile();
        }
    }
}