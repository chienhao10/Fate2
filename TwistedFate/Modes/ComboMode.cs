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

            if (ObjectManager.Player.Mana >= wMana)
            {
                var entKs = HeroManager.Enemies.FirstOrDefault(
                x => !x.IsDead && x.IsValidTarget()
                && (ObjectManager.Player.Distance(x) < Orbwalking.GetAttackRange(ObjectManager.Player) + 200)
                && x.Health < ObjectManager.Player.GetSpellDamage(x, SpellSlot.W));

                if (entKs != null)
                {
                    if (Spells._w.IsReadyPerfectly())
                    {
                        switch(CardSelector.Status)
                        {
                            case SelectStatus.Ready:
                            {
                                CardSelector.StartSelecting(Cards.First);
                                return;
                            }
                            case SelectStatus.Selecting:
                            {
                                CardSelector.JumpToCard(Cards.First);
                                return;
                            }
                        }
                    }
                }else
                {
                    if(Config.UseGoldCombo)
                    {
                        if (Spells._w.IsReadyPerfectly())
                        {
                            switch (CardSelector.Status)
                            {
                                case SelectStatus.Ready:
                                {
                                    CardSelector.StartSelecting(Cards.Yellow);
                                    return;
                                }
                                case SelectStatus.Selecting:
                                {
                                    CardSelector.JumpToCard(Cards.Yellow);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}