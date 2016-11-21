﻿namespace TwistedFate
{
    using System;
    using System.Drawing;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;

    using TwistedFate.Modes;

    internal static class Mainframe
    {
        #region Static Fields

        internal static readonly Random Rng = new Random((int)DateTime.UtcNow.Ticks);

        #endregion

        #region Properties

        internal static Orbwalking.Orbwalker Orbwalker { get; set; }
        internal static bool UltEnabled { get { return ObjectManager.Player.HasBuff("destiny_marker"); } }
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

        #region Methods

        internal static void Init()
        {
            Game.OnUpdate += OnUpdate;

            Obj_AI_Base.OnProcessSpellCast += Computed.OnProcessSpellCast;
            Obj_AI_Base.OnProcessSpellCast += Computed.YellowIntoQ;
            Obj_AI_Base.OnProcessSpellCast += Computed.RedIntoQ;
            Orbwalking.BeforeAttack += Computed.OnBeforeAttack;
            AntiGapcloser.OnEnemyGapcloser += Computed.Gapcloser_OnGapCloser;
            Interrupter2.OnInterruptableTarget += Computed.InterruptableSpell_OnInterruptableTarget;
            Drawing.OnEndScene += DrawingOnOnEndScene;
            Drawing.OnDraw += OnDraw;

            Game.PrintChat("<font color='#DE5291'>Ready. Play TF like Dopa!</font>");

        }

        private static void DrawingOnOnEndScene(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                if (Config.DrawRm && Spells._r.IsReadyPerfectly())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, 5500, System.Drawing.Color.PaleGreen, 2, 23, true);
                }
            }
        }

        public static void drawText(string msg, Vector3 Hero, System.Drawing.Color color, int weight = 0)
        {
            var wts = Drawing.WorldToScreen(Hero);

            Drawing.DrawText(wts[0] + (msg.Length), wts[1] + weight, color, msg);
        }

        private static void OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                if (Config.DrawQ && Spells._q.IsReadyPerfectly())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Spells._q.Range, System.Drawing.Color.CornflowerBlue);
                }

                if (Config.DrawR && Spells._r.IsReadyPerfectly())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Spells._r.Range, System.Drawing.Color.PaleGreen);
                }

                #region Timers

                if (HasACard != "empty")
                {
                    if (HasGold)
                    {
                        var buffG = ObjectManager.Player.GetBuff("goldcardpreattack");
                        var timeLastG = (buffG.EndTime - Game.Time);
                        var timeLastGInt = (int)Math.Round(timeLastG, MidpointRounding.ToEven);

                        drawText("Gold Ready: " + timeLastGInt + " s", ObjectManager.Player.Position, System.Drawing.Color.LightGreen, -75);

                    }
                    else if (HasBlue)
                    {
                        var buffB = ObjectManager.Player.GetBuff("bluecardpreattack");
                        var timeLastB = (buffB.EndTime - Game.Time);
                        var timeLastBInt = (int)Math.Round(timeLastB, MidpointRounding.ToEven);

                        drawText("Blue Ready: " + timeLastBInt + " s", ObjectManager.Player.Position, System.Drawing.Color.LightGreen, -75);

                    }
                    else
                    {
                        var buffR = ObjectManager.Player.GetBuff("redcardpreattack");
                        var timeLastR = (buffR.EndTime - Game.Time);
                        var timeLastRInt = (int)Math.Round(timeLastR, MidpointRounding.ToEven);

                        drawText("Red Ready: " + timeLastRInt + " s", ObjectManager.Player.Position, System.Drawing.Color.LightGreen, -75);
                    }
                }

                if (UltEnabled)
                {
                    var buffUlt = ObjectManager.Player.GetBuff("destiny_marker");
                    var timeLastUlt = (buffUlt.EndTime - Game.Time);
                    var timeLastUltInt = (int)Math.Round(timeLastUlt, MidpointRounding.ToEven);
                    drawText(timeLastUltInt + " s to TP!", ObjectManager.Player.Position, System.Drawing.Color.LightGoldenrodYellow, -45);
                }

                #endregion

                if (Spells._r.IsReadyPerfectly())
                {
                    var target = TargetSelector.GetTarget(Spells._r.Range, Spells._q.DamageType);

                    if (target.IsValidTarget())
                    {
                        var comboDMG = Spells._q.GetDamage(target) + Spells._w.GetDamage(target) + ObjectManager.Player.GetAutoAttackDamage(target) * 3;

                        if (comboDMG > target.Health)
                        {
                            drawText("You should check: " + target.ChampionName, ObjectManager.Player.Position, System.Drawing.Color.LightGoldenrodYellow);
                        }
                    }
                }

                var xMinions =
                    MinionManager.GetMinions(
                        ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius + 300,
                        MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);

                foreach (var xMinion in xMinions)
                {
                    if (ObjectManager.Player.GetAutoAttackDamage(xMinion, true) >= xMinion.Health)

                        Render.Circle.DrawCircle(xMinion.Position, xMinion.BoundingRadius - 20, System.Drawing.Color.GreenYellow, 3);

                    else if (ObjectManager.Player.GetAutoAttackDamage(xMinion, true) * 2 >= xMinion.Health)

                        Render.Circle.DrawCircle(xMinion.Position, xMinion.BoundingRadius - 20, System.Drawing.Color.OrangeRed, 3);
                }
            }  
        }

        private static void OnUpdate(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                switch(Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                    {
                            ComboMode.Execute();
                            break;
                    }
                    case Orbwalking.OrbwalkingMode.Mixed:
                    {
                            MixedMode.Execute();
                            break;
                    }
                    case Orbwalking.OrbwalkingMode.LaneClear:
                    {
                            Clear.Execute();
                            break;
                    }
                }

                ManualCards.Execute(); Automated.Execute(); QWaveClear.Execute(); QChampions.Execute();
            }
        }

        #endregion
    }
}