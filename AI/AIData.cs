using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Game.Editor.AI
{
    public class ConditionNodes
    {
        [DataMember]
        public List<RoundCondition> RoundConditions;
        [DataMember]
        public List<BuffCondition> BuffConditions;
        [DataMember]
        public List<ExecuteCountCondition> ExecuteCountConditions;
        [DataMember]
        public List<HPCondition> HPConditions;
        [DataMember]
        public List<PeopleNumberCondition> PeopleNumberConditions;
        [DataMember]
        public List<PowerCondition> PowerConditions;
        [DataMember]
        public List<SmartSkillCondition> SmartSkillConditions;
        [DataMember]
        public List<TargetTypeCondition> TargetTypeConditions;
        [DataMember]
        public List<ActionSequenceCondition> ActionSequenceConditions;
        [DataMember]
        public List<ProbCondition> ProbConditions;
        [DataMember]
        public List<MPCondition> MPConditions;
        [DataMember]
        public List<CDCondition> CDConditions;
        [DataMember]
        public List<BuffTypeCondition> BuffTypeConditions;
        [DataMember]
        public List<SelfSummonCondition> SelfSummonConditions;
        [DataMember]
        public List<BuffTagCondition> BuffTagConditions;
    }

    public class ExecutorNodes
    {

        [DataMember]
        public List<AutofightExecutor> AutofightExecutors;
        [DataMember]
        public List<EnoughPowerExecutor> EnoughPowerExecutors;
        [DataMember]
        public List<IntendedKeyMappingExecutor> IntendedKeyMappingExecutors;
        [DataMember]
        public List<IntendedSkillExecutor> IntendedSkillExecutors;
    }


    public class FilterNodes
    {
        [DataMember]
        public List<AttributeFilter> AttributeFilters;
        [DataMember]
        public List<BuffFilter> BuffFilters;
        [DataMember]
        public List<HPFilter> HPFilters;
        [DataMember]
        public List<IntendedTargetFilter> IntendedTargetFilters;
        [DataMember]
        public List<ManualTargetFilter> ManualTargetFilters;
        [DataMember]
        public List<ProfessionFilter> ProfessionFilters;
        [DataMember]
        public List<RandomTargetFilter> RandomTargetFilter;
        [DataMember]
        public List<CurrentHPFilter> CurrentHPFilterFilter;
        [DataMember]
        public List<SelfhoodFilter> SelfhoodFilterFilter;
        [DataMember]
        public List<BuffTypeFilter> BuffTypeFilters;
        [DataMember]
        public List<BuffTagFilter> BuffTagFilters;
    }

    public class AIData
    {
        [IgnoreDataMember]
        public List<Node> nodes = new List<Node>();

        [DataMember]
        public Vector2 scale = Vector2.one;
        [DataMember]
        public Vector2 translation;

        [DataMember]
        public RootNode RootNode;

        [DataMember]
        public List<Connection> Connections;

        [DataMember]
        public ConditionNodes ConditionNodes = new ConditionNodes();

        [DataMember]
        public ExecutorNodes ExecutorNodes = new ExecutorNodes();

        [DataMember]
        public FilterNodes FilterNodes = new FilterNodes();



        public AIData()
        {

        }

        private void Add<T>(ref List<T> list, T val)
        {
            if (null == list)
            {
                list = new List<T>()
                       {
                           val
                       };
            }
            else
            {
                list.Add(val);
            }
        }
        private void AddNode<T>(List<T> _nodes) where T : Node
        {
            if (null == _nodes) return;

            foreach (var node in _nodes)
            {
                nodes.Add(node);
            }

        }


        public void Init(List<Node> _nodes, List<Connection> _connections)
        {
            nodes = _nodes;
            Connections = _connections;
            foreach (var node in nodes)
            {
                switch (node)
                {
                    case RoundCondition round:
                        Add(ref ConditionNodes.RoundConditions, round);
                        break;
                    case BuffCondition buff:
                        Add(ref ConditionNodes.BuffConditions, buff);
                        break;
                    case ExecuteCountCondition executeCount:
                        Add(ref ConditionNodes.ExecuteCountConditions, executeCount);
                        break;
                    case HPCondition hp:
                        Add(ref ConditionNodes.HPConditions, hp);
                        break;
                    case PeopleNumberCondition peopleNumber:
                        Add(ref ConditionNodes.PeopleNumberConditions, peopleNumber);
                        break;
                    case PowerCondition power:
                        Add(ref ConditionNodes.PowerConditions, power);
                        break;
                    case SmartSkillCondition smartSkill:
                        Add(ref ConditionNodes.SmartSkillConditions, smartSkill);
                        break;
                    case TargetTypeCondition targetType:
                        Add(ref ConditionNodes.TargetTypeConditions, targetType);
                        break;
                    case ActionSequenceCondition actionSequence:
                        Add(ref ConditionNodes.ActionSequenceConditions, actionSequence);
                        break;
                    case ProbCondition prob:
                        Add(ref ConditionNodes.ProbConditions, prob);
                        break;
                    case MPCondition mp:
                        Add(ref ConditionNodes.MPConditions, mp);
                        break;
                    case CDCondition cd:
                        Add(ref ConditionNodes.CDConditions, cd);
                        break;
                    case BuffTypeCondition buffTypeCondition:
                        Add(ref ConditionNodes.BuffTypeConditions, buffTypeCondition);
                        break;
                    case SelfSummonCondition selfSummon:
                        Add(ref ConditionNodes.SelfSummonConditions, selfSummon);
                        break;
                    case BuffTagCondition buffTagCondition:
                        Add(ref ConditionNodes.BuffTagConditions, buffTagCondition);
                        break;
                    case AutofightExecutor autofight:
                        Add(ref ExecutorNodes.AutofightExecutors, autofight);
                        break;
                    case EnoughPowerExecutor enoughPower:
                        Add(ref ExecutorNodes.EnoughPowerExecutors, enoughPower);
                        break;
                    case IntendedKeyMappingExecutor keyMapping:
                        Add(ref ExecutorNodes.IntendedKeyMappingExecutors, keyMapping);
                        break;
                    case IntendedSkillExecutor intendedSkill:
                        Add(ref ExecutorNodes.IntendedSkillExecutors, intendedSkill);
                        break;
                    case AttributeFilter attribute:
                        Add(ref FilterNodes.AttributeFilters, attribute);
                        break;
                    case BuffFilter buff:
                        Add(ref FilterNodes.BuffFilters, buff);
                        break;
                    case HPFilter hp:
                        Add(ref FilterNodes.HPFilters, hp);
                        break;
                    case IntendedTargetFilter target:
                        Add(ref FilterNodes.IntendedTargetFilters, target);
                        break;
                    case ManualTargetFilter target:
                        Add(ref FilterNodes.ManualTargetFilters, target);
                        break;
                    case ProfessionFilter profession:
                        Add(ref FilterNodes.ProfessionFilters, profession);
                        break;
                    case RandomTargetFilter target:
                        Add(ref FilterNodes.RandomTargetFilter, target);
                        break;
                    case CurrentHPFilter currentHP:
                        Add(ref FilterNodes.CurrentHPFilterFilter, currentHP);
                        break;
                    case SelfhoodFilter selfhood:
                        Add(ref FilterNodes.SelfhoodFilterFilter, selfhood);
                        break;
                    case BuffTypeFilter buffTypeFilter:
                        Add(ref FilterNodes.BuffTypeFilters, buffTypeFilter);
                        break;
                    case BuffTagFilter buffTagFilter:
                        Add(ref FilterNodes.BuffTagFilters, buffTagFilter);
                        break;
                    case RootNode rootNode:
                        RootNode = rootNode;
                        break;
                }
            }
        }

        public void Attach()
        {
            AddNode(ConditionNodes.RoundConditions);
            AddNode(ConditionNodes.BuffConditions);
            AddNode(ConditionNodes.ExecuteCountConditions);
            AddNode(ConditionNodes.HPConditions);
            AddNode(ConditionNodes.PeopleNumberConditions);
            AddNode(ConditionNodes.PowerConditions);
            AddNode(ConditionNodes.SmartSkillConditions);
            AddNode(ConditionNodes.TargetTypeConditions);
            AddNode(ConditionNodes.ActionSequenceConditions);
            AddNode(ConditionNodes.ProbConditions);
            AddNode(ConditionNodes.CDConditions);
            AddNode(ConditionNodes.MPConditions);
            AddNode(ConditionNodes.BuffTypeConditions);
            AddNode(ConditionNodes.SelfSummonConditions);
            AddNode(ConditionNodes.BuffTagConditions);
            //
            AddNode(ExecutorNodes.AutofightExecutors);
            AddNode(ExecutorNodes.EnoughPowerExecutors);
            AddNode(ExecutorNodes.IntendedKeyMappingExecutors);
            AddNode(ExecutorNodes.IntendedSkillExecutors);
            //
            AddNode(FilterNodes.AttributeFilters);
            AddNode(FilterNodes.BuffFilters);
            AddNode(FilterNodes.HPFilters);
            AddNode(FilterNodes.IntendedTargetFilters);
            AddNode(FilterNodes.ManualTargetFilters);
            AddNode(FilterNodes.ProfessionFilters);
            AddNode(FilterNodes.RandomTargetFilter);
            AddNode(FilterNodes.CurrentHPFilterFilter);
            AddNode(FilterNodes.SelfhoodFilterFilter);
            AddNode(FilterNodes.BuffTypeFilters);
            AddNode(FilterNodes.BuffTagFilters);

            if (null != RootNode)
            {
                nodes.Add(RootNode);
            }
            nodes.Sort(SortById);

            if (null != Connections)
            {
                foreach (var connection in Connections)
                {
                    connection.inNode = nodes[connection.InNodeId - 1];
                    connection.outNode = nodes[connection.OutNodeId - 1];
                }
            }
        }

        private int SortById(Node lhs, Node rhs)
        {
            return lhs.Id - rhs.Id;
        }
    }
}