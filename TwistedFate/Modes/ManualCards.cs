namespace TwistedFate.Modes
{
    using System.Windows.Input;

    using LeagueSharp;

    internal static class ManualCards
    {
        #region Methods

        internal static void Execute()
        {
            var wMana = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ManaCost;

            if (ObjectManager.Player.Mana >= wMana)
            {
                if (Config.IsKeyPressed("csGold")
                    && (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.LeftCtrl)
                        && !Keyboard.IsKeyDown(Key.LeftAlt)))
                {
                    CardSelector.StartSelecting(Cards.Yellow);
                }

                if (Config.IsKeyPressed("csBlue")
                    && (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.LeftCtrl)
                        && !Keyboard.IsKeyDown(Key.LeftAlt)))
                {
                    CardSelector.StartSelecting(Cards.Blue);
                }

                if (Config.IsKeyPressed("csRed")
                    && (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.LeftCtrl)
                        && !Keyboard.IsKeyDown(Key.LeftAlt)))
                {
                    CardSelector.StartSelecting(Cards.Red);
                }
            }
        }

        #endregion
    }
}