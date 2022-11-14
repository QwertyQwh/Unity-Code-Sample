using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ZhFramework.Engine.Utilities;

namespace Game.Editor.AI
{
    public sealed partial class AIEditor:ISerializationCallbackReceiver
    {
        public const float kSnap = 5;
        public const float kNodeWidth = 200;
        public const float kNodeHeight = 45;

        private static Dictionary<Type, INodeDrawer> s_NodeDrawers = new Dictionary<Type, INodeDrawer>();

        private List<Node> m_Nodes = new List<Node>();
        private List<Node> m_SortNodes = new List<Node>();
        private List<Node> m_BranchNodes = new List<Node>();
        private List<Connection> m_Connections = new List<Connection>();
        private List<Node> m_NodeSelection = new List<Node>();
        private List<Connection> m_ConnectionSelection = new List<Connection>();

        [NonSerialized]
        private bool m_IsCreateConnect;

        private Node m_FromNode;
        private Node m_ToNode;
        private bool m_IsValidConnect;
        private bool m_IsDirty;
        private bool m_IsRepaint;
        private Node m_ContextNode;

        private bool m_IsDraggingRegion;
        private Vector2 m_DraggingStart;
        private Rect m_DraggingRect;

        [SerializeField]
        private string m_Data;

        static AIEditor()
        {
            Load(s_NodeDrawers);
        }

        private static void Load<T>(Dictionary<Type, T> dict)
        {
            var collection = TypeCache.GetTypesDerivedFrom(typeof(T));
            foreach (var element in collection)
            {
                if (null == element.BaseType || element.IsAbstract) continue;

                dict.Add(
                    element.BaseType.GenericTypeArguments[0],
                    (T) Activator.CreateInstance(element)
                );
            }
        }

        private void OnNodeCanvasGUI()
        {
            OnConnectionGUI();
            OnNodeGUI();

            OnDraggingGUI();

            if (m_IsDirty)
            {
                m_IsDirty = false;
                UpdateNodeIds();
            }

            if (m_IsRepaint)
            {
                m_IsRepaint = false;
                var e = Event.current;

                if (e.type != EventType.Repaint && e.type != EventType.Layout)
                {
                    e.Use();
                }

                Repaint();
            }
        }

        private void StartDraggingRegion()
        {
            m_IsDraggingRegion = true;
            m_DraggingStart = Event.current.mousePosition;
            m_NodeSelection.Clear();
            m_ConnectionSelection.Clear();
        }

        private void StopDraggingRegion()
        {
            m_IsDraggingRegion = false;
        }

        private void UpdateDraggingRegion<T>(Rect rt, List<T> list, T target)
        {
            var overlap = m_DraggingRect.Overlaps(rt);
            if (list.Contains(target) == overlap) return;
            if (overlap)
            {
                list.Add(target);
            }
            else
            {
                list.Remove(target);
            }
        }

        private void OnDraggingGUI()
        {
            if (!m_IsDraggingRegion) return;


            var e = Event.current;
            var start = m_DraggingStart;
            var end = e.mousePosition;
            m_DraggingRect = Rect.MinMaxRect(
                Math.Min(start.x, end.x),
                Math.Min(start.y, end.y),
                Math.Max(start.x, end.x),
                Math.Max(start.y, end.y));

            GUI.color = Color.yellow;
            GUI.Box(m_DraggingRect, GUIContent.none, GUI.skin.box);
            GUI.color = Color.white;
            Handles.color = new Color(1f, 0.92f, 0.016f, 0.5f);
            Handles.DrawLine(m_DraggingRect.min, m_DraggingRect.min + Vector2.right * m_DraggingRect.width);
            Handles.DrawLine(m_DraggingRect.max, m_DraggingRect.max - Vector2.right * m_DraggingRect.width);
            Handles.DrawLine(m_DraggingRect.min, m_DraggingRect.min + Vector2.up * m_DraggingRect.height);
            Handles.DrawLine(m_DraggingRect.max, m_DraggingRect.max - Vector2.up * m_DraggingRect.height);
            Handles.color = Color.white;

            m_IsRepaint = true;
        }

        private void OnNodePropertyGUI()
        {
            if (0 == m_Nodes.Count ) return;

            var node = 0 == m_NodeSelection.Count ? m_Nodes[0] : m_NodeSelection[0];
            if (!s_NodeDrawers.TryGetValue(node.GetType(), out var drawer))
            {
                return;
            }
            EditorGUILayout.LabelField($"{node.Id}-{drawer.GetName()}");
            using (new GUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MinHeight(20)))
            {
                drawer.SetTarget(node);
                drawer.OnPropertyGUI();

                drawer.SetTarget(null);
                GUILayout.Space(1);
            }
        }


        private Vector2 Snap(Vector2 v)
        {
            v.x = Mathf.CeilToInt(v.x) / kSnap * kSnap;
            v.y = Mathf.CeilToInt(v.y) / kSnap * kSnap;

            return v;
        }

        private void OnNodeGUI()
        {
            if (null == m_Nodes) return;


            BeginWindows();

            var offset = Vector2.zero;
            for (var i = 0; i < m_Nodes.Count; i++)
            {
                var node = m_Nodes[i];
                if (!s_NodeDrawers.TryGetValue(node.GetType(), out var drawer))
                {
                    return;
                }

                if (node.rect == Rect.zero)
                {
                    node.rect = new Rect(node.position, new Vector2(kNodeWidth, kNodeHeight));
                }

                node.rect = GUILayout.Window(
                    i,
                    node.rect,
                    OnWindowGUI,
                    string.Empty,
                    drawer.GetStyle(m_NodeSelection.Contains(node)));

                if (Vector2.Distance(node.position, node.rect.position) > kSnap)
                {
                    offset = Snap(node.rect.position - node.position);
                }
            }

            if (Vector2.zero != offset)
            {
                foreach (var node in m_NodeSelection)
                {
                    var temp = node.position + offset;

                    node.position = temp;
                    node.rect.position = temp;
                }

                m_IsDirty = true;
            }

            EndWindows();
            HandleCanvasKeyEvents();
        }

        private void OnConnectionGUI()
        {
            if (null == m_Connections) return;

            if (m_IsCreateConnect)
            {
                var mousePosition = Event.current.mousePosition;
                var startPosition = m_FromNode.rect.center;
                var endPosition = null == m_ToNode ? mousePosition : m_ToNode.rect.center;
                var offset = endPosition + startPosition;
                var startTangent = offset - startPosition;
                var endTangent = offset - endPosition;
                Handles.DrawBezier(startPosition, endPosition, startTangent, endTangent,
                                   m_IsValidConnect ? Color.grey : Color.red, null, 4);
                m_IsRepaint = true;
            }

            for (var i = 0; i < m_Connections.Count; )
            {
                var connection = m_Connections[i];

                if (connection.inNode == connection.outNode)
                {
                    m_Connections.RemoveAt(i);
                    continue;
                }

                connection.Update();
                var outNode = connection.outNode;
                var inNode = connection.inNode;
                var startPosition = outNode.rect.center;
                var endPosition = inNode.rect.center;
                var offset = endPosition + startPosition;
                var startTangent = offset - startPosition;
                var endTangent = offset - endPosition;

                var center = offset / 2;
                var area = 20;
                var rt = new Rect(center - Vector2.one * (area * 0.5f), Vector2.one * area);
                EditorGUIUtility.AddCursorRect(rt, MouseCursor.Link, 0);

                var selected = m_ConnectionSelection.Contains(connection);

                var e = Event.current;
                if (rt.Contains(e.mousePosition) && e.type == EventType.MouseDown)
                {
                    ControlLogic(e.control, m_ConnectionSelection, connection);
                }

                Handles.DrawBezier(startPosition, endPosition,
                                   startTangent, endTangent,
                                   selected ? Color.yellow : Color.grey, null,
                                   4);

                if (m_IsDraggingRegion)
                {
                    UpdateDraggingRegion(rt, m_ConnectionSelection, connection);
                }

                ++i;

            }
        }

        private void ControlLogic<T>(bool control, List<T> list, T value)
        {
            if (control)
            {
                if (list.Contains(value))
                {
                    list.Remove(value);
                }
                else
                {
                    list.Add(value);
                }
            }
            else if (!list.Contains(value))
            {
                list.Clear();
                list.Add(value);
            }
        }

        private void OnWindowGUI(int index)
        {
            var node = m_Nodes[index];
            if (!s_NodeDrawers.TryGetValue(node.GetType(), out var drawer))
            {
                return;
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                drawer.SetTarget(node);
                GUILayout.Label($"{node.Id}-{drawer.GetName()}", EditorStyles.whiteLargeLabel);
                GUILayout.FlexibleSpace();
                drawer.SetTarget(null);
            }

            var e = Event.current;
            var controlId = GUIUtility.GetControlID("Node".GetHashCode(), FocusType.Keyboard, node.rect);

            var mousePosition = e.mousePosition;

            switch (e.type)
            {
                case EventType.ContextClick:
                    {
                        m_ContextNode = node;
                    }
                    break;
                case EventType.MouseDown:
                    {
                        if (0 == e.button &&
                            GUIUtility.keyboardControl != controlId)
                        {
                            GUIUtility.keyboardControl = controlId;
                            ControlLogic(e.control, m_NodeSelection, node);
                        }

                        if (m_IsCreateConnect)
                        {
                            if (m_IsValidConnect && null != m_ToNode)
                            {
                                AddConnection(m_FromNode, m_ToNode);
                            }
                            m_IsCreateConnect = false;
                            m_FromNode = null;
                            m_ToNode = null;
                        }

                        //e.Use();  don't add, affect drag node
                    }
                    break;
                case EventType.MouseUp:
                    {
                        if (0 == e.button && GUIUtility.keyboardControl == controlId)
                        {
                            GUIUtility.keyboardControl = 0;
                        }
                    }
                    break;
            }

            if (mousePosition.x > 0 && mousePosition.y > 0 &&
                mousePosition.x < node.rect.width && mousePosition.y < node.rect.height)
            {
                if (m_ToNode != node && node != m_FromNode)
                {
                    m_ToNode = node;
                    m_IsValidConnect = IsValidConnect(m_FromNode, node);
                }
            }
            else if (m_ToNode == node)
            {
                m_ToNode = null;
                m_IsValidConnect = true;
            }

            if (m_IsDraggingRegion)
            {
                UpdateDraggingRegion(node.rect, m_NodeSelection, node);
            }

            if (GUIUtility.keyboardControl == controlId)
            {
                GUI.DragWindow();
            }
        }

        private void AddConnection(Node outNode, Node inNode)
        {
            Undo.RecordObject(this, "AddConnection");

            var connection = new Connection
                             {
                                 outNode = outNode,
                                 inNode = inNode
                             };
            m_Connections.Add(connection);
            m_IsDirty = true;
        }

        private bool IsValidConnect(Node outNode, Node inNode)
        {
            if (outNode == inNode || 0 != inNode.branch) return false;

            if (outNode is RootNode)
            {
                if (inNode is ConditionNode)
                {
                    return true;
                }

                return false;
            }

            if (outNode is ConditionNode)
            {
                if (inNode is ConditionNode)
                {
                    return true;
                }
                if (inNode is FilterNode)
                {
                    return true;
                }
                return false;
            }

            if (outNode is FilterNode)
            {
                if (inNode is FilterNode)
                {
                    return true;
                }
                if (inNode is ExecutorNode)
                {
                    return true;
                }
                return false;
            }

            return false;
        }

        private void CreateConnection(object userdata)
        {
            m_IsCreateConnect = true;
            m_FromNode = (Node) userdata;
            m_IsValidConnect = true;
        }
        
        private void HandleCanvasKeyEvents()
        {
            var e = Event.current;
            if (!m_CanvasRect.Contains(e.mousePosition) || 0 != GUIUtility.hotControl) return;

            switch (e.type)
            {
                case EventType.ContextClick:
                    if (null == m_ContextNode)
                    {
                        RightMenu();
                    }
                    else
                    {
                        NodeMenu();
                    }
                    e.Use();
                    break;
                case EventType.MouseUp:
                    if (m_IsDraggingRegion)
                    {
                        StopDraggingRegion();
                    }
                    break;
                case EventType.MouseDrag:
                    if (!m_IsDraggingRegion)
                    {
                        StartDraggingRegion();
                    }
                    break;
                case EventType.MouseDown:
                    if (0 == e.button)
                    {
                        if (m_IsCreateConnect)
                        {
                            m_FromNode = null;
                            m_ToNode = null;
                            m_IsCreateConnect = false;
                        }

                        m_NodeSelection.Clear();
                        m_ConnectionSelection.Clear();
                        e.Use();
                    }

                    break;
                case EventType.KeyUp:
                    switch (e.keyCode)
                    {
                        case KeyCode.Delete:
                            Delete();
                            e.Use();
                            break;
                        case KeyCode.A:
                            if (e.control)
                            {
                                /*m_Machine.selectionConnections.Clear();
                                m_Machine.selectionNodes.Clear();
                                m_Machine.selectionConnections.AddRange(m_Machine.Connections);
                                m_Machine.selectionNodes.AddRange(m_Machine.Nodes);*/
                            }

                            break;
                        case KeyCode.C:
                            if (e.control)
                            {
                                Copy();
                                e.Use();
                            }

                            break;
                        case KeyCode.V:
                            if (e.control)
                            {
                                Paste(true);
                                e.Use();
                            }

                            break;
                        case KeyCode.D:
                            if (e.control)
                            {
                                Duplicate();
                                e.Use();
                            }
                            break;
                    }

                    break;
            }
        }

        private void NodeMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(
                new GUIContent(NodeLanguages.createConnect),
                false,
                CreateConnection,
                m_ContextNode);
            menu.ShowAsContext();
            m_ContextNode = null;
        }

        private void RightMenu()
        {
            var menu = new GenericMenu();
            var mousePosition = Event.current.mousePosition;
            foreach (var pair in s_NodeDrawers)
            {
                var groupName = pair.Value.GetGroup();
                if(string.IsNullOrEmpty(groupName)) continue;

                var menuName = $"{groupName}/{pair.Value.GetName()}";
                if (string.IsNullOrEmpty(menuName)) continue;
                menu.AddItem(
                    new GUIContent(menuName),
                    false,
                    CreateNode,
                    new object[]
                    {
                        mousePosition,
                        pair.Key
                    });
            }

            menu.ShowAsContext();
        }

        private void CreateNode(object userdata)
        {
            Undo.RecordObject(this, "CreateNode");

            var objs = (object[]) userdata;
            var pos = (Vector2) objs[0];
            var type = (Type) objs[1];
            var node = (Node) Activator.CreateInstance(type);
            node.position = pos;
            AddNode(node);
        }

        private void AddNode(Node node)
        {
            node.position = Snap(node.position);
            node.Id = m_Nodes.Count + 1;
            m_Nodes.Add(node);
            m_IsDirty = true;
        }

        private void UpdateNodeIds()
        {
            m_SortNodes.Clear();
            m_BranchNodes.Clear();
            foreach (var node in m_Nodes)
            {
                node.Id = 0;
                node.branch = 0;
                m_SortNodes.Add(node);
            }

            foreach (var connection in m_Connections)
            {
                connection.inNode.Id = connection.outNode.Id + 1;
                if (connection.outNode is RootNode)
                {
                    m_BranchNodes.Add(connection.inNode);
                }
            }

            m_SortNodes.Sort(SortByDepth);

            for (var i = 0; i < m_SortNodes.Count; i++)
            {
                m_SortNodes[i].Id = i + 1;
            }

            m_BranchNodes.Sort(SortById);
            for (var i = 0; i < m_BranchNodes.Count; i++)
            {
                m_BranchNodes[i].branch = i + 1;
            }
        }

        private int SortByDepth(Node lhs, Node rhs)
        {
            var deltaX = lhs.position.x - rhs.position.x;
            var deltaY = lhs.position.y - rhs.position.y;
            if (Mathf.Abs(deltaY) > lhs.rect.height)
            {
                return Math.Sign(deltaY);
            }

            if (Mathf.Abs(deltaX) > lhs.rect.width / 2)
            {
                return Math.Sign(deltaX);
            }

            return 0;
        }

        private int SortById(Node lhs, Node rhs)
        {
            return lhs.Id - rhs.Id;
        }
        private void Duplicate()
        {
            if (Copy())
            {
                Paste(false);
            }
        }

        internal struct CopyData
        {
            public Vector2 center;
            public Dictionary<int, int> remappping;
            public List<Connection> connections;
            public List<Node> nodes;
        }

        private bool Copy()
        {
            var copyData = new CopyData
                           {
                               remappping = new Dictionary<int, int>(),
                               connections = new List<Connection>(),
                               nodes = new List<Node>(),
                               center = Vector2.zero
                           };

            for (var i = 0; i < m_NodeSelection.Count; i++)
            {
                var node = m_NodeSelection[i];
                if (node is RootNode) continue;

                copyData.remappping.Add(node.Id, i);
                copyData.nodes.Add(node);
                copyData.center += node.rect.center;
            }

            if (0 == copyData.nodes.Count) return false;


            for (var i = 0; i < m_Connections.Count; i++)
            {
                var connection = m_Connections[i];
                if (connection.outNode is RootNode) continue;

                if (m_NodeSelection.Contains(connection.inNode) &&
                    m_NodeSelection.Contains(connection.outNode))
                {
                    copyData.connections.Add(connection);
                }
            }

            copyData.center /= m_NodeSelection.Count;

            GUIUtility.systemCopyBuffer = JsonUtils.ToString(copyData);

            return true;
        }

        private void Paste(bool mousePostion)
        {
            Undo.RecordObject(this, "Paste");

            var copyData = JsonUtils.ToObject<CopyData>(GUIUtility.systemCopyBuffer);
            m_NodeSelection.Clear();
            m_ConnectionSelection.Clear();

            foreach (var node in copyData.nodes)
            {
                if (mousePostion)
                {
                    node.position = Event.current.mousePosition + (node.position - copyData.center);
                }
                else
                {
                    node.position += Vector2.one * 100;
                }

                AddNode(node);
                m_NodeSelection.Add(node);
            }

            foreach (var connection in copyData.connections)
            {
                connection.inNode = copyData.nodes[copyData.remappping[connection.InNodeId]];
                connection.outNode = copyData.nodes[copyData.remappping[connection.OutNodeId]];
                m_Connections.Add(connection);
                m_ConnectionSelection.Add(connection);
            }

            m_IsDirty = true;
            m_IsRepaint = true;
        }

        private void Delete()
        {
            foreach (var node in m_NodeSelection)
            {
                if(node is RootNode) continue;

                m_Nodes.Remove(node);
            }

            for (var i = 0; i < m_Connections.Count;)
            {
                if (m_NodeSelection.Contains(m_Connections[i].inNode) ||
                    m_NodeSelection.Contains(m_Connections[i].outNode))
                {
                    m_Connections.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }

            m_NodeSelection.Clear();

            foreach (var connection in m_ConnectionSelection)
            {
                m_Connections.Remove(connection);
            }

            m_ConnectionSelection.Clear();

            m_IsDirty = true;
            m_IsRepaint = true;
        }

        public void OnBeforeSerialize()
        { 
            m_Data = JsonUtils.ToString(GetData());
        }

        public void OnAfterDeserialize()
        {
            var data = JsonUtils.ToObject<AIData>(m_Data);
            data.Attach();
            m_Nodes = data.nodes;
            m_Connections = data.Connections;
            m_Translation = data.translation;
            m_Scale = data.scale;
            Repaint();
        }
    }
}