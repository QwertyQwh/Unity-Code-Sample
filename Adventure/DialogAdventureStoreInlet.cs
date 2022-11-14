using Table.Structure;
using UnityEngine;

namespace Game.Runtime
{
    [UISettings(UILayer.Default, UIContext.Blur | UIContext.Popup | UIContext.Tips)]
    public class DialogAdventureStoreInlet : CommonDialog_Tips_w
    {
        [Hotfix] private RectTransform m_UnitsRoot;

        protected override void OnPreload(params object[] args)
        {
            base.OnPreload(args);

            var ptr = TableStrongholdDesc.GetEnumerator();
            while (ptr.MoveNext())
            {
                var data = TableStrongholdDesc.Find(ptr.Current.Key);
                if (ConditionHelper.CheckSysFunc(data.FuncId))
                {
                    CreateWidget<UnitAdventureStoreInlet>(m_UnitsRoot, true, data);
                }
            }
            ptr.Dispose();
        }
    }
}