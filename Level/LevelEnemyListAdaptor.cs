using Battle.Core;
using Battle.Table;
using Rotorz.Games.Collections;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Battle.Level.Editor
{
    public class LevelEnemyListAdaptor : GenericListAdaptor<LevelEnemyInfo>
    {
        private const int kMaxEnemyCount = 6;

        public LevelEnemyListAdaptor(IList<LevelEnemyInfo> list) : base(list, null, 16f)
        {
        }

        public override void DrawItem(Rect position, int index)
        {
            var enemy = List[index];
            float nameLength = 60f;
            float idLength = 80f;

            position.xMax = position.x + idLength;
            enemy.RoleId = EditorGUI.DelayedIntField(position, enemy.RoleId);

            position.x = position.xMax;
            position.xMax = position.x + nameLength;

            try
            {
                if (enemy.RoleId > 0)
                {
                    var role = TableMonster.Find(enemy.RoleId);
                    GUI.Label(position, $"{role.Name}");

                    position.x = position.xMax + 30f;
                    position.xMax = position.x + 30f;
                    GUI.Label(position, "我方");
                    position.x = position.x + 30f;
                    position.xMax = position.x + 30f;
                    enemy.IsAlly = EditorGUI.Toggle(position, enemy.IsAlly);



                    position.x = position.xMax + 30f;
                    position.xMax = position.x + 30f;
                    GUI.Label(position, "督战");
                    position.x = position.x + 30f;
                    position.xMax = position.x + 30f;
                    enemy.IsSupervise = EditorGUI.Toggle(position, enemy.IsSupervise);


                    position.x = position.xMax + 30f;
                    position.xMax = position.x + 30f;
                    GUI.Label(position, LevelLanguage.kStageOn);
                    position.x = position.x + 30f;
                    position.xMax = position.x + 30f;
                    enemy.ShowStageOn = EditorGUI.Toggle(position, enemy.ShowStageOn);

                    position.x = position.x + 30f;
                    position.xMax = position.x + 30f;
                    GUI.Label(position, LevelLanguage.kBornPos);

                    position.x = position.x + 30f;
                    position.xMax = position.x + 30f;
                    enemy.BornPos = EditorGUI.IntField(position, enemy.BornPos);

                    position.x = position.xMax + 10f;
                    position.xMax = position.x + 100f;

                    string qualityDescr = "";
                    switch (role.Quality)
                    {
                        default:
                            qualityDescr = "???";
                            break;
                        case 1:
                            qualityDescr = "N";
                            break;
                        case 2:
                            qualityDescr = "R";
                            break;
                        case 3:
                            qualityDescr = "SR";
                            break;
                        case 4:
                            qualityDescr = "SSR";
                            break;
                        case 5:
                            qualityDescr = "UR";
                            break;
                    }
                    GUI.Label(position, $"{role.Level}{LevelLanguage.kLevel} {role.Stars}{LevelLanguage.kStar} {qualityDescr}");

                    position.x = position.xMax + 20f;
                    position.xMax = position.x + 70f;

                    var monster = TableMonster.Find(enemy.RoleId);
                    if (enemy.Restraint == ERestraint.None)
                        enemy.Restraint = (ERestraint)TableRoleAttribute.Find(monster.Card_ID).Restraint;

                    Color color = Color.red;
                    switch(enemy.Restraint)
                    {
                        case ERestraint.Order:
                            color = Color.green;
                            break;
                        case ERestraint.Freedom:
                            color = Color.blue;
                            break;
                    }
                    using(new GUIColor(color, true))
                    {
                        enemy.Restraint = (ERestraint)EditorGUI.EnumPopup(position, enemy.Restraint);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }

        }

        public override void Add()
        {
            if (List.Count < kMaxEnemyCount)
                List.Add(new LevelEnemyInfo());
        }

        public override void Remove(int index)
        {
            if (EditorUtility.DisplayDialog(SkillLanguage.Tips, SkillLanguage.DeleteConfirm, SkillLanguage.Ok, SkillLanguage.Cancel))
                base.Remove(index);
        }
    }
}