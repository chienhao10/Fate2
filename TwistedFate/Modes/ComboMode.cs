#region Use
using LeagueSharp;
using LeagueSharp.Common;
using System.Linq; 
#endregion

namespace TwistedFate.Modes
{
    using Config = TwistedFate.Config;

    internal static class ComboMode
    {
        #region Methods

        internal static void Execute()
        {
            var wMana = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ManaCost;

            foreach (var enemy in HeroManager.Enemies)
            {
                if (!enemy.IsDead && enemy != null)
                {
                    if (enemy.IsKillableAndValidTarget(Spells._w.GetDamage(enemy), Spells._w.DamageType, Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) + 200))
                    {
                        if (Spells._w.IsReadyPerfectly())
                        {
                            if (CardSelector.Status == SelectStatus.Ready)
                            {
                                CardSelector.StartSelecting(Cards.First);
                            }
                        }

                        if (CardSelector.Status == SelectStatus.Selecting)
                        {
                            CardSelector.JumpToCard(Cards.First);
                        }
                    }
                    else if (Config.UseGoldCombo)
                    {
                        if (Spells._w.IsReadyPerfectly())
                        {
                            if (CardSelector.Status == SelectStatus.Ready)
                            {
                                CardSelector.StartSelecting(Cards.Yellow);
                            }
                        }

                        if (CardSelector.Status == SelectStatus.Selecting)
                        {
                            CardSelector.JumpToCard(Cards.Yellow);
                        }
                    }
                }
            }
        }

        #endregion
    }
}