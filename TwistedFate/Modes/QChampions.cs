﻿#region Use
using System;
using System.Windows.Input;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common; 
#endregion

namespace TwistedFate.Modes
{
    using Config = TwistedFate.Config;

    internal static class QChampions
    {

        #region Prop

        internal static int CastQTick;

        #endregion

        #region Methods

        internal static void Execute()
        {
            if (Config.UseQEnemy)
            {
                if (Spells._q.IsReadyPerfectly())
                {
                    CastQTick = Utils.TickCount;

                    foreach (var enemy in HeroManager.Enemies.Where(e => e.IsValidTarget(Spells._q.Range) && !e.IsDead))
                    {
                        if(Utils.TickCount - CastQTick < 500)
                        {
                            switch(Config.PredSemiQ)
                            {
                                //VeryHigh
                                case 0:
                                {
                                    Pred.CastSebbyPredict(Spells._q, enemy, HitChance.VeryHigh);
                                    break;
                                }
                                //High
                                case 1:
                                {
                                    Pred.CastSebbyPredict(Spells._q, enemy, Spells._q.MinHitChance);
                                    break;
                                }
                                //Medium
                                case 2:
                                {
                                    Pred.CastSebbyPredict(Spells._q, enemy, HitChance.Medium);
                                    break;
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