
using System.Collections.Generic;
using Myproto;

namespace Game.Editor.AI
{
    public sealed class BuffTagCondition : ConditionNode
    {
        public List<BuffTagData> BuffTagDatas = new List<BuffTagData>();
        public TargetType TargetType = TargetType.Enemy;
    }
}