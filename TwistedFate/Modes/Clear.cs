#region Use
using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common; 
#endregion

namespace TwistedFate.Modes
{
    using Config = TwistedFate.Config;

    internal static class Clear
    {
        #region Methods

        internal static void Execute()
        {
            var wMana = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ManaCost;
            var qMana = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost;

            var jungle = SebbyLib.Cache.GetMinions(ObjectManager.Player.ServerPosition, 700, MinionTeam.Neutral);

            if (!jungle.Any() || jungle.FirstOrDefault() == null)
            {
                return;
            }

            if (Spells._w.IsReadyPerfectly())
            {
                if (jungle.Any(x => x.Name.StartsWith("SRU_Baron") || x.Name.StartsWith("SRU_Dragon")))
                {
                    switch(CardSelector.Status)
                    {
                        case SelectStatus.Ready:
                        {
                            CardSelector.StartSelecting(Cards.Blue);
                            return;
                        }
                        case SelectStatus.Selecting:
                        {
                            CardSelector.JumpToCard(Cards.Blue);
                            return;
                        }
                    }
                }
                else
                {
                    var combinedManaPercent = (ObjectManager.Player.MaxMana / (wMana + qMana));

                    if (ObjectManager.Player.ManaPercent >= Math.Max(45, 20 + combinedManaPercent))
                    {
                        var targetAoE = jungle.Count(x => x.Distance(jungle.FirstOrDefault()) <= 250);

                        if (targetAoE > 2)
                        {
                            switch (CardSelector.Status)
                            {
                                case SelectStatus.Ready:
                                {
                                    CardSelector.StartSelecting(Cards.Red);
                                    return;
                                }
                                case SelectStatus.Selecting:
                                {
                                    CardSelector.JumpToCard(Cards.Red);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            if (jungle.FirstOrDefault().HealthPercent >= 70 && ObjectManager.Player.HealthPercent < 50)
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
                            else
                            {
                                CardSelector.StartSelecting(Cards.Blue);
                            }
                        }
                    }
                    else
                    {
                        CardSelector.StartSelecting(Cards.Blue);
                    }
                }
                
            }

            if(Spells._q.IsReadyPerfectly())
            {
                if (ObjectManager.Player.Mana - qMana >= wMana)
                {
                    var target = jungle.FirstOrDefault(x => x.IsValidTarget(Spells._q.Range));

                    if (target != null)
                    {
                        Spells._q.Cast(target);
                    }
                }
            }
        }

        #endregion
    }
}