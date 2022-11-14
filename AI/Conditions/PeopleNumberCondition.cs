
namespace Game.Editor.AI
{
    public sealed class PeopleNumberCondition : ConditionNode
    {
        public CompareType CompareType;
        public bool Living;
        public int Num;
        public TargetType TargetType = TargetType.Enemy;
    }
}