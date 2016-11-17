#region
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
#endregion

namespace TwistedFate
{
    internal class Program
    {
        /// <summary>
        /// BEST TWISTED FATE EUW --> CREDITS To: Kortaku - mztikk - Diabaths - badao
        /// </summary>

        private static Menu Config;

        private static Spell Q, W, R;
        private static readonly float Qangle = 28*(float) Math.PI/180;
        private static Orbwalking.Orbwalker SOW;
        private static Obj_AI_Hero Player;
        private static int CastQTick;
        private static bool HasBlue { get { return Player.HasBuff("bluecardpreattack"); } }
        private static bool HasRed { get { return Player.HasBuff("redcardpreattack"); } }
        private static bool HasGold { get { return Player.HasBuff("goldcardpreattack"); } }
        private static string HasACard
        {
            get
            {
                if (Player.HasBuff("bluecardpreattack"))
                    return "blue";
                if (Player.HasBuff("goldcardpreattack"))
                    return "gold";
                if (Player.HasBuff("redcardpreattack"))
                    return "red";
                return "none";
            }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "TwistedFate") return;
            Player = ObjectManager.Player;
            Q = new Spell(SpellSlot.Q, 1450);
            W = new Spell(SpellSlot.W);
            R = new Spell(SpellSlot.R, 5500);
            Q.SetSkillshot(0.25f, 40f, 1000f, false, SkillshotType.SkillshotLine);

            //Menu
            Config = new Menu("Gross Gore's Fate", "TwistedFate", true);

            //TS
            var TargetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(TargetSelectorMenu);
            Config.AddSubMenu(TargetSelectorMenu);

            //Orbwalker
            var SowMenu = new Menu("Orbwalking", "Orbwalking");
            SOW = new Orbwalking.Orbwalker(SowMenu);
            Config.AddSubMenu(SowMenu);

            //Q
            var q = new Menu("Q Spell", "Q");
            {
                q.AddItem(
                    new MenuItem("CastQ", "Use Q - Champions (Hold)").SetValue(new KeyBind("U".ToCharArray()[0], KeyBindType.Press)));
                q.AddItem(
                    new MenuItem("CastQClear", "Use Q - LaneClear (Hold)").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
                Config.AddSubMenu(q);
            }

            //W
            var w = new Menu("Pick a Card Helper", "W");
            {
                w.AddItem(
                    new MenuItem("SelectYellow", "Gold Card").SetValue(new KeyBind("W".ToCharArray()[0],
                        KeyBindType.Press)));
                w.AddItem(
                    new MenuItem("SelectBlue", "Blue Card").SetValue(new KeyBind("E".ToCharArray()[0],
                        KeyBindType.Press)));
                w.AddItem(
                    new MenuItem("SelectRed", "Red Card").SetValue(new KeyBind("T".ToCharArray()[0],
                        KeyBindType.Press)));
                Config.AddSubMenu(w);
            }

            //Drawings
            var drawings = new Menu("Drawings", "Drawings");
            {
                drawings.AddItem(
                    new MenuItem("Qcircle", "Q Range").SetValue(new Circle(true, Color.FromArgb(90, 255, 255, 119))));
                drawings.AddItem(
                    new MenuItem("Rcircle", "R Range").SetValue(new Circle(true, Color.FromArgb(90, 0, 255, 238))));
                drawings.AddItem(
                    new MenuItem("Rcircle2", "R Range (minimap)").SetValue(new Circle(true,
                        Color.FromArgb(90, 242, 255, 0))));
                Config.AddSubMenu(drawings);
            }

            //Damage after combo
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw Damage After Combo").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            Config.AddItem(new MenuItem("Combo", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));

            Config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += DrawingOnOnEndScene;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Orbwalking.BeforeAttack += OrbwalkingOnBeforeAttack;
            AntiGapcloser.OnEnemyGapcloser += Gapcloser_OnGapCloser;
            Interrupter2.OnInterruptableTarget += InterruptableSpell_OnInterruptableTarget;

            Game.PrintChat("<font color='#FFFFFF'>Best Twisted Fate - </font><font color='#FF2247'>Be Gross Gore!</font>");
        }

        private static void InterruptableSpell_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (sender.IsEnemy && Orbwalking.InAutoAttackRange(sender))
            {
                if (HasGold)
                {
                    ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, sender);
                }
            }
        }

        private static void Gapcloser_OnGapCloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsEnemy && Orbwalking.InAutoAttackRange(gapcloser.Sender))
            {
                if (HasGold)
                {
                    ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, gapcloser.Sender);
                }
            }
        }

        private static void AutoKS()
        {
            if (Q.IsReady())
            {
                foreach (var x in HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range) && Player.GetSpellDamage(x, SpellSlot.Q) > x.Health))
                {
                    Q.Cast(x);
                }
            }
        }

        private static void QClear()
        {
            if (Q.IsReady())
            {
                var minions = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range).Where(x => x.Type == GameObjectType.obj_AI_Minion && x.Team != ObjectManager.Player.Team).ToList();
                if (!minions.Any() || minions.Count < 3)
                {
                    return;
                }

                var minionPos = minions.Select(x => x.Position.To2D()).ToList();
                var farm = MinionManager.GetBestLineFarmLocation(minionPos, Q.Width, Q.Range);
                if (farm.MinionsHit >= 3)
                {
                    Q.Cast(farm.Position);
                }
            }
        }

        private static void LogicAutoQ()
        {
            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (qTarget != null && (qTarget.MoveSpeed < 275 || qTarget.IsStunned || !qTarget.CanMove || qTarget.IsRooted ||
                    qTarget.IsCharmed || qTarget.Distance(Player) < 450))
            {
                Q.Cast(qTarget);
            }
        }

        private static void CardHelper()
        {
            var _combo = Config.Item("Combo").GetValue<KeyBind>().Active;
            var _tmagic = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            //Select cards.
            if (Config.Item("SelectYellow").GetValue<KeyBind>().Active || _combo)
            {
                CardSelector.StartSelecting(Cards.Yellow);
            }

            if (Config.Item("SelectBlue").GetValue<KeyBind>().Active)
            {
                CardSelector.StartSelecting(Cards.Blue);
            }

            if (Config.Item("SelectRed").GetValue<KeyBind>().Active)
            {
                CardSelector.StartSelecting(Cards.Red);
            }

            if (SOW.ActiveMode == Orbwalking.OrbwalkingMode.Mixed
                && _tmagic != null
                && ObjectManager.Player.Distance(_tmagic) < Orbwalking.GetAttackRange(ObjectManager.Player) + 175)
            {
                CardSelector.StartSelecting(Cards.First);
            }
        }

        private static void JungleClear()
        {
            var jungle =
            MinionManager.GetMinions(ObjectManager.Player.Position, ObjectManager.Player.AttackRange + 200, MinionTypes.All, MinionTeam.Neutral)
                .Where(x => x.Team == GameObjectTeam.Neutral)
                .OrderByDescending(x => x.MaxHealth);
            if (!jungle.Any() || jungle.FirstOrDefault() == null)
            {
                return;
            }

            if (W.IsReady())
            {
                if (jungle.Any(x => x.Name.StartsWith("SRU_Baron") || x.Name.StartsWith("SRU_Dragon")))
                {
                    CardSelector.StartSelecting(Cards.Blue);
                }
                else
                {
                    var combinedManaPercent = ObjectManager.Player.MaxMana
                                                / (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ManaCost
                                                    + ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost);
                    if (ObjectManager.Player.ManaPercent
                        >= Math.Max(45, 20 + combinedManaPercent))
                    {
                        var targetAoE = jungle.Count(x => x.Distance(jungle.FirstOrDefault()) <= 250);
                        if (targetAoE > 2)
                        {
                            CardSelector.StartSelecting(Cards.Red);
                        }
                        else
                        {
                            if (jungle.FirstOrDefault().HealthPercent >= 40
                                && ObjectManager.Player.HealthPercent < 75)
                            {
                                CardSelector.StartSelecting(Cards.Yellow);
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

            if (Q.IsReady())
            {
                var target = jungle.FirstOrDefault(x => x.IsValidTarget(Q.Range));
                if (target != null)
                {
                    Q.Cast(target);
                }
            }
        }

        private static void OrbwalkingOnBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero)
                args.Process = CardSelector.Status != SelectStatus.Selecting &&
                               Utils.TickCount - CardSelector.LastWSent > 300;

            //Block AA while isSelecting(W) in combo/mixed
            if (CardSelector.Status == SelectStatus.Selecting
                && SOW.ActiveMode == Orbwalking.OrbwalkingMode.Combo
                && SOW.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                args.Process = false;
            }

            //Prioritize W-AA on enemy instead of last-hit
            var mode = new Orbwalking.OrbwalkingMode[] { Orbwalking.OrbwalkingMode.Mixed, Orbwalking.OrbwalkingMode.Combo };
            var _tmagic = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (HasACard != "none" && !HeroManager.Enemies.Contains(args.Target)
                && SOW.ActiveMode == Orbwalking.OrbwalkingMode.Mixed
                && _tmagic != null
                && ObjectManager.Player.Distance(_tmagic) < Orbwalking.GetAttackRange(ObjectManager.Player) + 400)
            {
                args.Process = false;
                var target = TargetSelector.GetTarget(Orbwalking.GetRealAutoAttackRange(Player), TargetSelector.DamageType.Magical);
                if (target.IsValidTarget() && !target.IsZombie)
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
        }

        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (args.SData.Name.Equals("Gate", StringComparison.InvariantCultureIgnoreCase))
            {
                CardSelector.StartSelecting(Cards.Yellow);
            }
        }

        private static void DrawingOnOnEndScene(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                var rCircle2 = Config.Item("Rcircle2").GetValue<Circle>();

                if (rCircle2.Active)
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, 5500, rCircle2.Color, 2, 23, true);
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Vector2 screenPoss = Drawing.WorldToScreen(Player.Position);
            var qCircle = Config.Item("Qcircle").GetValue<Circle>();
            var rCircle = Config.Item("Rcircle").GetValue<Circle>();

            if (!ObjectManager.Player.IsDead)
            {
                //Spells Range
                if (qCircle.Active)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, qCircle.Color);
                }
                if (rCircle.Active)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, 5500, rCircle.Color);
                }

                //Target focused
                var orbwalkerTarget = SOW.GetTarget();

                if (orbwalkerTarget.IsValidTarget())
                    Render.Circle.DrawCircle(orbwalkerTarget.Position, orbwalkerTarget.BoundingRadius, Color.PeachPuff);
                
                //Draw Minions Last Hit
                var xMinions =
                    MinionManager.GetMinions(
                        ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius + 300,
                        MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);

                foreach (var xMinion in xMinions)
                {
                    if (ObjectManager.Player.GetAutoAttackDamage(xMinion, true) >= xMinion.Health)
                        Render.Circle.DrawCircle(xMinion.Position, xMinion.BoundingRadius - 20,
                            Color.LightGreen, 3);
                    else if (ObjectManager.Player.GetAutoAttackDamage(xMinion, true) * 2 >= xMinion.Health)
                        Render.Circle.DrawCircle(xMinion.Position, xMinion.BoundingRadius - 20,
                            Color.LightCyan, 3);
                }

                //Draw Gankable R
                if (R.IsReady())
                {
                    var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
                    if (t.IsValidTarget())
                    {
                        var comboDMG = Q.GetDamage(t) + W.GetDamage(t) + Player.GetAutoAttackDamage(t) * 3;
                        if (comboDMG > t.Health)
                            Drawing.DrawText(screenPoss.X, screenPoss.Y + 15, System.Drawing.Color.Yellow, "--> (R Ready) kill: " + t.ChampionName + " Health -> " + t.Health);
                    }
                }
                
                //Draw R Timer
                var buffDestiny = Player.GetBuff("destiny_marker");
                var remainingTime = (buffDestiny.EndTime - Game.Time);
                var remainingTimeInt = (int)Math.Round(remainingTime, MidpointRounding.ToEven);
                Drawing.DrawText(screenPoss.X, screenPoss.Y - 75, System.Drawing.Color.Red, "R2 Timer: " + remainingTimeInt);
            }
        }

        private static int CountHits(Vector2 position, List<Vector2> points, List<int> hitBoxes)
        {
            var result = 0;

            var startPoint = ObjectManager.Player.ServerPosition.To2D();
            var originalDirection = Q.Range*(position - startPoint).Normalized();
            var originalEndPoint = startPoint + originalDirection;

            for (var i = 0; i < points.Count; i++)
            {
                var point = points[i];

                for (var k = 0; k < 3; k++)
                {
                    var endPoint = new Vector2();
                    if (k == 0) endPoint = originalEndPoint;
                    if (k == 1) endPoint = startPoint + originalDirection.Rotated(Qangle);
                    if (k == 2) endPoint = startPoint + originalDirection.Rotated(-Qangle);

                    if (point.Distance(startPoint, endPoint, true, true) <
                        (Q.Width + hitBoxes[i])*(Q.Width + hitBoxes[i]))
                    {
                        result++;
                        break;
                    }
                }
            }

            return result;
        }

        private static void CastQ(Obj_AI_Base unit, Vector2 unitPosition, int minTargets = 0)
        {
            var points = new List<Vector2>();
            var hitBoxes = new List<int>();

            var startPoint = ObjectManager.Player.ServerPosition.To2D();
            var originalDirection = Q.Range*(unitPosition - startPoint).Normalized();

            foreach (var enemy in HeroManager.Enemies)
            {
                if (enemy.IsValidTarget() && enemy.NetworkId != unit.NetworkId)
                {
                    var pos = Q.GetPrediction(enemy);
                    if (pos.Hitchance >= HitChance.Medium)
                    {
                        points.Add(pos.UnitPosition.To2D());
                        hitBoxes.Add((int) enemy.BoundingRadius);
                    }
                }
            }

            var posiblePositions = new List<Vector2>();

            for (var i = 0; i < 3; i++)
            {
                if (i == 0) posiblePositions.Add(unitPosition + originalDirection.Rotated(0));
                if (i == 1) posiblePositions.Add(startPoint + originalDirection.Rotated(Qangle));
                if (i == 2) posiblePositions.Add(startPoint + originalDirection.Rotated(-Qangle));
            }


            if (startPoint.Distance(unitPosition) < 900)
            {
                for (var i = 0; i < 3; i++)
                {
                    var pos = posiblePositions[i];
                    var direction = (pos - startPoint).Normalized().Perpendicular();
                    var k = (2/3*(unit.BoundingRadius + Q.Width));
                    posiblePositions.Add(startPoint - k*direction);
                    posiblePositions.Add(startPoint + k*direction);
                }
            }

            var bestPosition = new Vector2();
            var bestHit = -1;

            foreach (var position in posiblePositions)
            {
                var hits = CountHits(position, points, hitBoxes);
                if (hits > bestHit)
                {
                    bestPosition = position;
                    bestHit = hits;
                }
            }

            if (bestHit + 1 <= minTargets)
                return;

            Q.Cast(bestPosition.To3D(), true);
        }

        private static void QEnemy()
        {
            if (Config.Item("CastQ").GetValue<KeyBind>().Active)
            {
                CastQTick = Utils.TickCount;
            }

            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (Utils.TickCount - CastQTick < 500)
            {
                if (qTarget != null)
                {
                    Q.Cast(qTarget);
                }
            }

            if (ObjectManager.Player.Spellbook.CanUseSpell(SpellSlot.Q) == SpellState.Ready)
            {
                foreach (var enemy in HeroManager.Enemies)
                {
                    if (enemy.IsValidTarget(Q.Range * 2))
                    {
                        var pred = Q.GetPrediction(enemy);
                        if ((pred.Hitchance == HitChance.Immobile) ||
                            (pred.Hitchance == HitChance.Dashing))
                        {
                            CastQ(enemy, pred.UnitPosition.To2D());
                        }
                    }
                }
            }
        }

        private static float ComboDamage(Obj_AI_Hero hero)
        {
            var dmg = 0d;
            dmg += Player.GetSpellDamage(hero, SpellSlot.Q)*2;
            dmg += Player.GetSpellDamage(hero, SpellSlot.W);
            dmg += Player.GetSpellDamage(hero, SpellSlot.Q);

            if (ObjectManager.Player.GetSpellSlot("SummonerIgnite") != SpellSlot.Unknown)
            {
                dmg += ObjectManager.Player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            }

            return (float) dmg;
        }

        private static void checkbuff()
        {
            var temp = Player.Buffs.Aggregate("", (current, buff) => current + ("( " + buff.Name + " , " + buff.Count + " )"));
            if (temp != null)
                Game.PrintChat(temp);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (SOW.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                JungleClear();
            }

            AutoKS();

            if (Config.Item("CastQClear").GetValue<KeyBind>().Active)
            {
                QClear();
            }

            QEnemy();

            LogicAutoQ();

            CardHelper();
        }
    }
}
