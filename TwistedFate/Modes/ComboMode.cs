namespace TwistedFate.Modes
{
    using LeagueSharp;
    using LeagueSharp.Common;

    using Config = TwistedFate.Config;

    internal static class ComboMode
    {
        #region Methods

        internal static void Execute()
        {
            if (Spells.W.IsReady())
            {
                CardSelector.StartSelecting(Cards.Yellow);
            }
        }

        #endregion
    }
}