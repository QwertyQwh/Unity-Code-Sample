using Battle.Core;
using Battle.Table;
using Rotorz.Games.Collections;
using Rotorz.Games.UnityEditorExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Battle.Level.Editor
{
    public class LevelModuleListAdaptor : GenericListAdaptor<ModuleId>, IReorderableListDropTarget
    {
        public bool CanDragToOtherList = true;
        public Action<Type> OnCreateModule;
        public Action<ModuleId> OnModuleDelete;
        public Action<ModuleId> OnModuleSelect;

        private static LevelModuleListAdaptor sSelectedList;
        private static ModuleId sSelectedItem;
        private static Vector2 sMouseDownPosition;
        private static ModuleId sRenamingItem;
        private static readonly string sModuleIdTypeName = typeof(ModuleId).FullName;

        private const float kMouseDragThresholdInPixels = 0.6f;
        private List<Type> mTypes = new List<Type>();
        private Func<ModuleId, string> GetNameFunc;
        private LevelConfig mLevelConfig;


        private class ModuleDraggedItem
        {
            public static readonly string TypeName = typeof(ModuleId).FullName;

            public int Index;
            public readonly ModuleId ModuleId;
            public readonly LevelModuleListAdaptor ListAdaptor;

            public ModuleDraggedItem(LevelModuleListAdaptor list, int index, ModuleId moduleId)
            {
                ListAdaptor = list;
                Index = index;
                ModuleId = moduleId;
            }
        }

        public LevelModuleListAdaptor(IList<ModuleId> list, Func<ModuleId, string> getNameFunc, LevelConfig levelConfig = null) : base(list, null, 16f)
        {
            GetNameFunc = getNameFunc;
            mLevelConfig = levelConfig;
        }

        bool IReorderableListDropTarget.CanDropInsert(int insertionIndex)
        {
            if (!ReorderableListControl.CurrentListPosition.Contains(Event.current.mousePosition))
                return false;

            var obj = DragAndDrop.GetGenericData(ModuleDraggedItem.TypeName);
            if (!(obj is ModuleDraggedItem))
                return false;

            return true;
        }

        void IReorderableListDropTarget.ProcessDropInsertion(int insertionIndex)
        {
            if (Event.current.type == EventType.DragPerform)
            {
                var draggedItem = DragAndDrop.GetGenericData(sModuleIdTypeName) as ModuleDraggedItem;

                // 如果是自己这个List，则调整位置
                if (draggedItem.ListAdaptor == this)
                {
                    Move(draggedItem.Index, insertionIndex);
                }
                else // 否则，移动到目标List
                {
                    List.Insert(insertionIndex, draggedItem.ModuleId);
                    sSelectedList = this;
                }
            }
        }

        public override void DrawItemBackground(Rect position, int index)
        {
            if (this == sSelectedList && List[index] == sSelectedItem)
            {
                Color restoreColor = GUI.color;
                GUI.color = ExtraEditorStyles.Skin.SelectedHighlightColor;
                GUI.DrawTexture(position, EditorGUIUtility.whiteTexture);
                GUI.color = restoreColor;
            }
        }

        public override void DrawItem(Rect position, int index)
        {
            var moduleId = List[index];

            int controlID = GUIUtility.GetControlID(FocusType.Passive);

            switch (Event.current.GetTypeForControl(controlID))
            {
                case EventType.KeyUp:
                    if (Event.current.keyCode == KeyCode.F2)
                    {
                        if (sSelectedItem != null)
                        {
                            sRenamingItem = sSelectedItem;

                            Event.current.Use();
                        }
                    }
                    break;
                case EventType.MouseDown:
                    Rect totalItemPosition = ReorderableListGUI.CurrentItemTotalPosition;
                    if (totalItemPosition.Contains(Event.current.mousePosition))
                    {
                        sSelectedList = this;
                        sSelectedItem = moduleId;

                        OnModuleSelect?.Invoke(moduleId);
                    }

                    // 计算Item的拖拽区域
                    Rect draggableRect = totalItemPosition;
                    draggableRect.x = position.x;
                    draggableRect.width = position.width;

                    if (Event.current.button == 0 && draggableRect.Contains(Event.current.mousePosition))
                    {
                        sSelectedList = this;
                        sSelectedItem = moduleId;

                        GUIUtility.hotControl = controlID;
                        sMouseDownPosition = Event.current.mousePosition;
                        Event.current.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (!CanDragToOtherList)
                        return;

                    if (GUIUtility.hotControl == controlID)
                    {
                        GUIUtility.hotControl = 0;

                        if (Vector2.Distance(sMouseDownPosition, Event.current.mousePosition) >= kMouseDragThresholdInPixels)
                        {
                            var item = new ModuleDraggedItem(this, index, moduleId);

                            DragAndDrop.PrepareStartDrag();
                            DragAndDrop.objectReferences = new UnityObject[0];
                            DragAndDrop.paths = new string[0];
                            DragAndDrop.SetGenericData(ModuleDraggedItem.TypeName, item);
                            DragAndDrop.StartDrag("test");
                        }

                        Event.current.Use();
                    }
                    break;

                case EventType.Repaint:
                    if (sRenamingItem == moduleId)
                    {
                        //sRenamingItem.Name = GUI.TextArea(position, s_RenamingItem.Name);
                    }
                    else
                        EditorStyles.label.Draw(position, GetNameFunc.Invoke(moduleId), false, false, false, false);

                    break;
            }
        }

        public void AddModuleType<T>()
        {
            var t = typeof(T);
            if (!mTypes.Contains(t))
                mTypes.Add(t);
        }

        public override void Add()
        {
            var menu = new GenericMenu();

            foreach (var t in mTypes)
            {
                if (mLevelConfig != null && mLevelConfig.Data != null)
                {
                    var modeData = LevelEditor.LevelModeDict;
                    foreach(var data in modeData)
                    {
                            menu.AddItem(new GUIContent(GetDescription(t)), false, () => { OnCreateModule(t); });
                    }

                }
                else
                    menu.AddItem(new GUIContent(GetDescription(t)), false, () => { OnCreateModule(t); });
            }
            menu.ShowAsContext();
        }

        public override void Remove(int index)
        {
            if (EditorUtility.DisplayDialog(SkillLanguage.Tips, SkillLanguage.DeleteConfirm, SkillLanguage.Ok, SkillLanguage.Cancel))
            {
                OnModuleDelete?.Invoke(List[index]);
                base.Remove(index);
            }
        }

        private static string GetDescription(Type t)
        {
            var descriptions = t.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (descriptions.Length == 0)
                return null;
            return (descriptions[0] as DescriptionAttribute).Description;
        }
    }
}

