﻿namespace TwistedFate
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;
    using System.Linq;

    internal static class Computed

    {
        #region Properties

        internal static Orbwalking.Orbwalker Orbwalker { get; set; }
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
            if (Config.IsChecked("goldInter"))
            {
                if (sender.IsEnemy && Orbwalking.InAutoAttackRange(sender))
                {
                    if (HasGold)
                    {
                        ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, sender);
                    }
                }
            }
        }

        public static void Gapcloser_OnGapCloser(ActiveGapcloser gapcloser)
        {
            if (Config.IsChecked("goldGap"))
            {
                if (gapcloser.Sender.IsEnemy && Orbwalking.InAutoAttackRange(gapcloser.Sender))
                {
                    if (HasGold)
                    {
                        ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, gapcloser.Sender);
                    }
                }
            }
        }

        public static void TwistedFate_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            if (sender != null)
            {
                Utility.HpBarDamageIndicator.Enabled = e.GetNewValue<bool>();
                CustomDamageIndicator.Enabled = e.GetNewValue<bool>();
            }
        }

        public static void OnBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero)
            {
                args.Process = CardSelector.Status != SelectStatus.Selecting
                               && Environment.TickCount - CardSelector.LastWSent > 300;
            }

            if (CardSelector.Status == SelectStatus.Selecting
                && ((Mainframe.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    || (Mainframe.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)))
            {
                args.Process = false;
            }

            var targetDis = TargetSelector.GetTarget(Spells.Q.Range, TargetSelector.DamageType.Magical);

            if (HasACard != "empty"
                && !HeroManager.Enemies.Contains(args.Target)
                && targetDis.IsValidTarget()
                && !targetDis.IsZombie
                && Mainframe.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed
                && (ObjectManager.Player.Distance(targetDis) < Orbwalking.GetAttackRange(ObjectManager.Player) + 300))
            {
                args.Process = false;

                var target = TargetSelector.GetTarget(Orbwalking.GetRealAutoAttackRange(ObjectManager.Player), TargetSelector.DamageType.Magical);

                if (target.IsValidTarget() && !target.IsZombie)
                    ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
        }

        public static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "Gate")
            {
                CardSelector.StartSelecting(Cards.Yellow);
            }

            if (!sender.IsMe)
            {
                //none
            }
        }

        public static void SafeCast(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            //none
        }

        public static void YellowIntoQ(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var canWKill =
            HeroManager.Enemies.FirstOrDefault(
                h =>
                !h.IsDead && h.IsValidTarget(Spells.Q.Range)
                && h.Health < ObjectManager.Player.GetSpellDamage(h, SpellSlot.W));

            if (!Config.IsChecked("qGold") || !sender.IsMe || args.SData.Name.ToLower() != "goldcardpreattack" || !Spells.Q.IsReady() || canWKill != null)
            {
                return;
            }

            var qTarget = args.Target as Obj_AI_Base;

            if (qTarget == null || !qTarget.IsValidTarget(Spells.Q.Range))
            {
                return;
            }

            if (Mainframe.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo
                || Mainframe.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                var target = TargetSelector.GetTarget(Spells.Q.Range, TargetSelector.DamageType.Magical);

                if (target.IsValidTarget(Spells.Q.Range))
                {
                    var qPred = Spells.Q.GetPrediction(target);
                    if (qPred.Hitchance >= HitChance.VeryHigh)
                    {
                        Spells.Q.Cast(qPred.CastPosition);
                    }
                }
            }
        }

        public static void RedIntoQ(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var canWKill =
                HeroManager.Enemies.FirstOrDefault(
                h =>
                !h.IsDead && h.IsValidTarget(Spells.Q.Range)
                && h.Health < ObjectManager.Player.GetSpellDamage(h, SpellSlot.W));

            if (!Config.IsChecked("qRed") || !sender.IsMe || args.SData.Name.ToLower() != "redcardpreattack" || !Spells.Q.IsReady() || canWKill != null)
            {
                return;
            }

            var qTarget = args.Target as Obj_AI_Base;

            if (qTarget == null || !qTarget.IsValidTarget(Spells.Q.Range))
            {
                return;
            }

            if (Mainframe.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo
                || Mainframe.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                var target = TargetSelector.GetTarget(Spells.Q.Range, TargetSelector.DamageType.Magical);

                if (target.IsValidTarget(Spells.Q.Range))
                {
                    var qPred = Spells.Q.GetPrediction(target);
                    if (qPred.Hitchance >= HitChance.VeryHigh)
                    {
                        Spells.Q.Cast(qPred.CastPosition);
                    }
                }
            }
        }

        #endregion
    }
}