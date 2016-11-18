﻿namespace TwistedFate.Modes
{
    using System;
    using System.Windows.Input;

    using LeagueSharp;
    using LeagueSharp.Common;
    using System.Linq;

    using Config = TwistedFate.Config;

    internal static class QWaveClear
    {
        #region Methods

        internal static void Execute()
        {
            var qMana = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost;

            if (Config.IsKeyPressed("qClear")
                    && (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.LeftCtrl)
                        && !Keyboard.IsKeyDown(Key.LeftAlt)) && ObjectManager.Player.Mana >= qMana && Spells.Q.IsReady())
            {
                var minions = MinionManager.GetMinions(ObjectManager.Player.Position, Spells.Q.Range).Where(x => x.Type == GameObjectType.obj_AI_Minion && x.Team != ObjectManager.Player.Team).ToList();

                if (!minions.Any() || minions.Count < 3)
                {
                    return;
                }

                var minionPos = minions.Select(x => x.Position.To2D()).ToList();
                var farm = MinionManager.GetBestLineFarmLocation(minionPos, Spells.Q.Width, Spells.Q.Range);

                if (farm.MinionsHit >= 3)
                {
                    Spells.Q.Cast(farm.Position);
                }
            }
        }

        #endregion
    }
}