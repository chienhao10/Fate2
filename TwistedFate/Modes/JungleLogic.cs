namespace TwistedFate.Modes
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using Config = TwistedFate.Config;

    internal static class JungleLogic
    {
        #region Methods

        internal static void Execute()
        {
            if (ObjectManager.Player.ManaPercent < 10)
            {
                return;
            }

            var jungle =
                MinionManager.GetMinions(ObjectManager.Player.Position, ObjectManager.Player.AttackRange + 200, MinionTypes.All, MinionTeam.Neutral)
                    .Where(x => x.Team == GameObjectTeam.Neutral)
                    .OrderByDescending(x => x.MaxHealth);
            if (!jungle.Any() || jungle.FirstOrDefault() == null)
            {
                return;
            }

            if (Spells.W.IsReady())
            {
                if (jungle.Any(x => x.Name.StartsWith("SRU_Baron") || x.Name.StartsWith("SRU_Dragon")))
                {
                    CardSelector.StartSelecting(Cards.Blue);
                }
            }
        }

        #endregion
    }
}