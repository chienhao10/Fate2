﻿namespace TwistedFate.Modes
{
    using LeagueSharp;
    using LeagueSharp.Common;

    using Config = TwistedFate.Config;

    internal static class MixedMode
    {
        #region Methods

        internal static void Execute()
        {
            var target = TargetSelector.GetTarget(Spells._q.Range, Spells._q.DamageType);

            if (!Config.Rotate || target == null || !target.IsValidTarget(Spells._q.Range))
            {
                return;
            }

            if(Spells._w.IsReadyPerfectly())
            {
                if(target.Distance(ObjectManager.Player) <= (ObjectManager.Player.AttackRange + Config.RotateRange))
                {
                    if(ObjectManager.Player.ManaPercent >= Config.RotateMana)
                    {
                        CardSelector.RotateCards();
                    }
                }
            }

            if(target.Distance(ObjectManager.Player) <= (ObjectManager.Player.AttackRange + 100))
            {
                CardSelector.LockCard();
            }
        }

        #endregion
    }
}