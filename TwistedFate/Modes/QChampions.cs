﻿#region Use
using System;
using System.Windows.Input;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common; 
#endregion

namespace TwistedFate.Modes
{
    using Config = TwistedFate.Config;

    internal static class QChampions
    {

        #region Prop

        internal static int CastQTick;

        #endregion

        #region Methods

        internal static void Execute()
        {
            var qMana = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost;

            if (Config.UseQEnemy)
            {
                if(Spells._q.IsReadyPerfectly())
                {
                    if(ObjectManager.Player.Mana >= qMana)
                    {
                        CastQTick = Utils.TickCount;
                    }
                }
            }

            if (Utils.TickCount - CastQTick < 500)
            {
                foreach (var enemy in HeroManager.Enemies)
                {
                    if (!enemy.IsDead && enemy != null)
                    {
                        if(enemy.IsValidTarget(Spells._q.Range))
                        {
                            Pred.CastSebbyPredict(Spells._q, enemy, HitChance.VeryHigh);
                        }
                    }
                }
            }
        }

        #endregion
    }
}