
using System.Collections.Generic;
using Myproto;

namespace Game.Editor.AI
{
    public class BuffData
    {
        public TargetType TargetType = TargetType.Enemy;
        public string BuffName;
        public int Count;
        //public BuffType BuffType;
        public bool NonBuffType;
    }
    public sealed class BuffCondition : ConditionNode
    {
        public List<BuffData> BuffDatas = new List<BuffData>();
    }
}