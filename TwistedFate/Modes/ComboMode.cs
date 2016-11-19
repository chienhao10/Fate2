namespace TwistedFate.Modes
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using System.Linq;

    using Config = TwistedFate.Config;

    internal static class ComboMode
    {
        #region Methods

        internal static void Execute()
        {
            var wMana = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ManaCost;

            if (ObjectManager.Player.Mana >= wMana && Spells.W.IsReady())
            {
                var entKs =
                    HeroManager.Enemies.FirstOrDefault(
                        h =>
                        !h.IsDead && h.IsValidTarget()
                        && (ObjectManager.Player.Distance(h) < Orbwalking.GetAttackRange(ObjectManager.Player) + 300)
                        && h.Health < ObjectManager.Player.GetSpellDamage(h, SpellSlot.W));

                if (Config.IsChecked("wKS") && entKs != null)
                {
                    CardSelector.StartSelecting(Cards.First);
                }else
                {
                    if (Config.IsChecked("wCGold"))
                    {
                        CardSelector.StartSelecting(Cards.Yellow);
                    }
                }
            }
        }

        #endregion
    }
}