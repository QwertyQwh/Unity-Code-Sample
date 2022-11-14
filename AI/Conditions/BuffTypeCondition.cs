
using System.Collections.Generic;
using Myproto;

namespace Game.Editor.AI
{
    public class BuffTypeData
    {
        public BuffType BuffType;
        public bool NonBuffType;
    }
    public sealed class BuffTypeCondition : ConditionNode
    {
        public List<BuffTypeData> BuffTypeDatas = new List<BuffTypeData>();
    }
}