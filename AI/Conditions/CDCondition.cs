
using UnityEngine;

namespace Game.Editor.AI
{
    public enum TargetType
    {
        [InspectorName(NodeLanguages.kEnumAll)]
        All,
        [InspectorName(NodeLanguages.kEnumEnemy)]
        Enemy = 1,
        [InspectorName(NodeLanguages.kEnumFriend)]
        Friend = 2,
        [InspectorName(NodeLanguages.kEnumSelf)]
        Self = 3,
        [InspectorName(NodeLanguages.kEnumEnemySummon)]
        EnemySummon = 4,
        [InspectorName(NodeLanguages.kEnumFriendSummon)]
        FriendSummon = 5,
        [InspectorName(NodeLanguages.kEnumEnemyWithoutSummon)]
        EnemyWithoutSummon = 7,
        [InspectorName(NodeLanguages.kEnumFriendWithoutSummon)]
        FriendWithoutSummon = 8,
        [InspectorName(NodeLanguages.kEnumFriendWithoutSelf)]
        FriendWithoutSelf = 9,
    }

    public sealed class HPCondition : ConditionNode
    {
        public CompareType CompareType;
        public int Percent;
    }
}