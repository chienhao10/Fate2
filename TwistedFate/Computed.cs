#region Use
using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common; 
#endregion

namespace TwistedFate
{
    internal static class Computed

    {
        #region Properties

        internal static bool HasBlue { get { return ObjectManager.Player.HasBuff("bluecardpreattack"); } }
        internal static bool HasRed { get { return ObjectManager.Player.HasBuff("redcardpreattack"); } }
        internal static bool HasGold { get { return ObjectManager.Player.HasBuff("goldcardpreattack"); } }
        internal static string HasACard
        {
            get
            {
                if (ObjectManager.Player.HasBuff("bluecardpreattack"))
                    return "blue";
                if (ObjectManager.Player.HasBuff("goldcardpreattack"))
                    return "gold";
                if (ObjectManager.Player.HasBuff("redcardpreattack"))
                    return "red";
                return "empty";
            }
        }

        #endregion

        #region Public Methods and Operators

        public static void InterruptableSpell_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            if (Config.UseInterrupter)
            {
                var wMana = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ManaCost;

                if (sender.IsValidTarget(Spells._w.Range))
                {
                    switch(CardSelector.Status)
                    {
                        case SelectStatus.Selecting:
                        {
                            CardSelector.JumpToCard(Cards.Yellow);
                            return;
                        }
                        case SelectStatus.Ready:
                        {
                            if(ObjectManager.Player.ManaPercent >= wMana)
                            {
                                CardSelector.StartSelecting(Cards.Yellow);
                            }
                            return;
                        }
                    }

                    if(HasGold)
                    {
                        if (Orbwalking.InAutoAttackRange(sender))
                        {
                            if (Orbwalking.CanAttack())
                            {
                                ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, sender);
                            }
                        }
                    }
                }
            }
        }

        public static void Gapcloser_OnGapCloser(ActiveGapcloser gapcloser)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            if (Config.UseAntiGapCloser)
            {
                var wMana = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ManaCost;

                if (gapcloser.Sender.IsValidTarget(Spells._w.Range))
                {
                    switch (CardSelector.Status)
                    {
                        case SelectStatus.Selecting:
                        {
                            CardSelector.JumpToCard(Cards.Yellow);
                            return;
                        }
                        case SelectStatus.Ready:
                        {
                            if (ObjectManager.Player.ManaPercent >= wMana)
                            {
                                CardSelector.StartSelecting(Cards.Yellow);
                            }
                            return;
                        }
                    }

                    if (HasGold)
                    {
                        if (Orbwalking.InAutoAttackRange(gapcloser.Sender))
                        {
                            if (Orbwalking.CanAttack())
                            {
                                ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, gapcloser.Sender);
                            }
                        }
                    }
                }
            }
        }

        public static void OnBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            if (args.Target is Obj_AI_Hero)
            {
                args.Process = CardSelector.Status != SelectStatus.Selecting
                               && Environment.TickCount - CardSelector.LastWSent > 300;
            }

            if (CardSelector.Status == SelectStatus.Selecting)
            {
                if(Mainframe.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo
                    || Mainframe.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                {
                    args.Process = false;
                }
            }

            if(Mainframe.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if(HasACard != "empty")
                {
                    if(!HeroManager.Enemies.Contains(args.Target))
                    {
                        var targetDis = TargetSelector.GetTarget(Spells._q.Range, Spells._q.DamageType);

                        if (targetDis.IsValidTarget(Spells._q.Range))
                        {
                            if(!targetDis.IsZombie)
                            {
                                if((ObjectManager.Player.Distance(targetDis) <= Orbwalking.GetAttackRange(ObjectManager.Player) + 150))
                                {
                                    args.Process = false;

                                    var target = TargetSelector.GetTarget(Orbwalking.GetRealAutoAttackRange(ObjectManager.Player), Spells._w.DamageType);

                                    if(target.IsValidTarget(Spells._w.Range))
                                    {
                                        if(!target.IsZombie)
                                        {
                                            ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if(ObjectManager.Player.IsDead)
            {
                return;
            }

            if (sender != null)
            {
                if (sender.IsMe)
                {
                    if (args.Slot == SpellSlot.R)
                    {
                        if (args.SData.Name.ToLowerInvariant() == "gate")
                        {
                            switch (CardSelector.Status)
                            {
                                case SelectStatus.Selecting:
                                {
                                    CardSelector.JumpToCard(Cards.Yellow);
                                    return;
                                }
                                case SelectStatus.Ready:
                                {
                                    CardSelector.StartSelecting(Cards.Yellow);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void YellowIntoQ(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!Config.PredictQ || ObjectManager.Player.IsDead
                || (Mainframe.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo
                && Mainframe.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Mixed))
            {
                return;
            }

            if (sender.IsMe)
            {
                if (args.SData.Name.ToLower() == "goldcardpreattack")
                {
                    if (Spells._q.IsReadyPerfectly())
                    {
                        if (ObjectManager.Player.ManaPercent >= Config.AutoqMana)
                        {
                            foreach (var enemy in HeroManager.Enemies)
                            {
                                if (!enemy.IsDead)
                                {
                                    if (!enemy.IsKillableAndValidTarget(Spells._w.GetDamage(enemy), Spells._w.DamageType, Spells._q.Range))
                                    {
                                        if (enemy.IsValidTarget(Spells._q.Range / 2))
                                        {
                                            Pred.CastSebbyPredict(Spells._q, enemy, Spells._q.MinHitChance);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void RedIntoQ(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!Config.PredictQ || ObjectManager.Player.IsDead
                || (Mainframe.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo
                && Mainframe.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Mixed))
            {
                return;
            }

            if (sender.IsMe)
            {
                if (args.SData.Name.ToLower() == "redcardpreattack")
                {
                    if (Spells._q.IsReadyPerfectly())
                    {
                        if (ObjectManager.Player.ManaPercent >= Config.AutoqMana)
                        {
                            foreach (var enemy in HeroManager.Enemies)
                            {
                                if (!enemy.IsDead)
                                {
                                    if(!enemy.IsKillableAndValidTarget(Spells._w.GetDamage(enemy), Spells._w.DamageType, Spells._q.Range))
                                    {
                                        if (enemy.IsValidTarget(Spells._q.Range / 2))
                                        {
                                            Pred.CastSebbyPredict(Spells._q, enemy, Spells._q.MinHitChance);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static float GetComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (!ObjectManager.Player.IsWindingUp)
            {
                damage += (float)ObjectManager.Player.GetAutoAttackDamage(enemy, true);
            }

            if (Spells._q.IsReadyPerfectly())
            {
                damage += Spells._q.GetDamage(enemy);
            }

            if (Spells._w.IsReadyPerfectly())
            {
                damage += Spells._w.GetDamage(enemy, 3);
            }

            if (ObjectManager.Player.HasBuff("cardmasterstackparticle"))
            {
                damage += Spells._e.GetDamage(enemy, 1);
            }

            // Items

            /*
             * Luden
             * */
            if (Items.HasItem(3285))
                damage += (float)ObjectManager.Player.CalcDamage(enemy, Damage.DamageType.Magical, 100 + ObjectManager.Player.FlatMagicDamageMod * 0.1);
            /*
            * Sheen
            * */
            if (Items.HasItem(3057))
                damage += (float)ObjectManager.Player.CalcDamage(enemy, Damage.DamageType.Physical, 0.5 * ObjectManager.Player.BaseAttackDamage);
            /*
            * Lich
            * */
            if (Items.HasItem(3100))
                damage += (float)ObjectManager.Player.CalcDamage(enemy, Damage.DamageType.Magical, 0.5 * ObjectManager.Player.FlatMagicDamageMod + 0.75 * ObjectManager.Player.BaseAttackDamage);

            return (float)damage;
        }

        #endregion
    }
}