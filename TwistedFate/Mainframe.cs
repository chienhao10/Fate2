namespace TwistedFate
{
    using System;
    using System.Drawing;

    using LeagueSharp;
    using LeagueSharp.Common;

    using TwistedFate.Modes;

    internal static class Mainframe
    {
        #region Static Fields

        internal static readonly Random Rng = new Random((int)DateTime.UtcNow.Ticks);

        #endregion

        #region Properties

        internal static Orbwalking.Orbwalker Orbwalker { get; set; }

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

            Game.PrintChat("<font color='#FFFFFF'></font><font color='#DE5291'>Ready. Play TF like Gross Gore!</font>");

        }

        private static void DrawingOnOnEndScene(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {

                if (Config.IsChecked("drawRmap") && (!Config.IsChecked("drawOnlyReady") || Spells.R.IsReady()))
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, 5500, Color.PaleGreen, 2, 23, true);
                }
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                if (Config.IsChecked("drawQrange") && (!Config.IsChecked("drawOnlyReady") || Spells.Q.IsReady()))
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Spells.Q.Range, Color.CornflowerBlue);
                }

                if (Config.IsChecked("drawRrange") && (!Config.IsChecked("drawOnlyReady") || Spells.R.IsReady()))
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Spells.R.Range, Color.PaleGreen);
                }
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                ComboMode.Execute();
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                MixedMode.Execute();
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                JungleLogic.Execute();
            }

            ManualCards.Execute();

            Automated.Execute();

            QWaveClear.Execute();

            QChampions.Execute();
        }

        #endregion
    }
}