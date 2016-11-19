namespace TwistedFate
{
    #region Use

    using LeagueSharp.Common;
    using System.Drawing;

    #endregion

    internal static class Config
    {
        #region Properties

        internal static Menu QMenu { get; private set; }

        internal static Menu WMenu { get; private set; }

        internal static Menu ExtraMenu { get; private set; }

        internal static Menu DrawMenu { get; private set; }

        internal static Menu OrbwalkMenu { get; private set; }

        internal static Menu TargetSelectorMenu { get; private set; }

        internal static Menu TwistedFateMenu { get; private set; }

        #endregion

        #region Methods

        internal static void BuildConfig()
        {
            TwistedFateMenu = new Menu("Gross Gore - Fate", "twistedfate", true).SetFontStyle(FontStyle.Bold, SharpDX.Color.DodgerBlue);

            TargetSelectorMenu = new Menu("Target Selector", "targetselector");
            TargetSelector.AddToMenu(TargetSelectorMenu);
            TwistedFateMenu.AddSubMenu(TargetSelectorMenu);

            OrbwalkMenu = new Menu("Orbwalker", "orbwalker");
            Mainframe.Orbwalker = new Orbwalking.Orbwalker(OrbwalkMenu);
            TwistedFateMenu.AddSubMenu(OrbwalkMenu);

            QMenu = new Menu("Q Spell", "qSpellMenu");
            QMenu.AddItem(new MenuItem("qKeys", "Q Key Bindings")).SetFontStyle(FontStyle.Bold, SharpDX.Color.BlueViolet);
            QMenu.AddItem(
                new MenuItem("qClear", "Q Wave Clear (hold)").SetValue(
                    new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
            QMenu.AddItem(new MenuItem("qClearCount", "X min Creeps to hit").SetValue(new Slider(3, 2, 5)));
            QMenu.AddItem(
                new MenuItem("qEnemy", "Q Enemy (hold)").SetValue(
                    new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            QMenu.AddItem(new MenuItem("qAuto", "Q Automated")).SetFontStyle(FontStyle.Bold, SharpDX.Color.BlueViolet);
            QMenu.AddItem(new MenuItem("qDashing", "Q if Dashing").SetValue(false));
            QMenu.AddItem(new MenuItem("qSlowed", "Q if Slowed").SetValue(true));
            QMenu.AddItem(new MenuItem("qImmobile", "Q if Immobile").SetValue(true));
            QMenu.AddItem(new MenuItem("qKS", "Q if Killsteal").SetValue(true));
            QMenu.AddItem(new MenuItem("qBurst", "Burst Mode (Combo/Mixed)")).SetFontStyle(FontStyle.Bold, SharpDX.Color.BlueViolet);
            QMenu.AddItem(new MenuItem("qRed", "Red Card into instant Q").SetValue(true));
            QMenu.AddItem(new MenuItem("qGold", "Gold Card into instant Q").SetValue(true));
            TwistedFateMenu.AddSubMenu(QMenu);

            WMenu = new Menu("W Spell", "wSpellMenu");
            WMenu.AddItem(new MenuItem("wQuick", "Fast Harass")).SetFontStyle(FontStyle.Bold, SharpDX.Color.BlueViolet);
            WMenu.AddItem(new MenuItem("wHarass", "First Card if In AA Range").SetValue(true));
            WMenu.AddItem(new MenuItem("wHRange", "Extra AA Range").SetValue(new Slider(120, 0, 250)));
            WMenu.AddItem(new MenuItem("wSelector", "Koratu's Selector")).SetFontStyle(FontStyle.Bold, SharpDX.Color.BlueViolet);
            WMenu.AddItem(
                new MenuItem("csGold", "Select Gold Card").SetValue(new KeyBind("O".ToCharArray()[0], KeyBindType.Press)));
            WMenu.AddItem(
                new MenuItem("csBlue", "Select Blue Card").SetValue(
                    new KeyBind("U".ToCharArray()[0], KeyBindType.Press)));
            WMenu.AddItem(
                new MenuItem("csRed", "Select Red Card").SetValue(new KeyBind("I".ToCharArray()[0], KeyBindType.Press)));
            WMenu.AddItem(new MenuItem("wMiscs", "Miscs")).SetFontStyle(FontStyle.Bold, SharpDX.Color.BlueViolet);
            WMenu.AddItem(new MenuItem("wCGold", "Always Gold in Combo").SetValue(true));
            WMenu.AddItem(new MenuItem("wKS", "First Card if Killsteal").SetValue(true));
            TwistedFateMenu.AddSubMenu(WMenu);

            ExtraMenu = new Menu("More+", "extraMenu");
            ExtraMenu.AddItem(new MenuItem("moreDesc", "Need: Gold Card Ready!")).SetFontStyle(FontStyle.Bold, SharpDX.Color.BlueViolet);
            ExtraMenu.AddItem(new MenuItem("goldInter", "Interrupter").SetValue(true));
            ExtraMenu.AddItem(new MenuItem("goldGap", "Anti-GapCloser").SetValue(true));
            TwistedFateMenu.AddSubMenu(ExtraMenu);

            DrawMenu = new Menu("Drawings", "drawings");
            DrawMenu.AddItem(new MenuItem("drawQrange", "Q Range").SetValue(true));
            DrawMenu.AddItem(new MenuItem("drawRrange", "R Range").SetValue(true));
            DrawMenu.AddItem(new MenuItem("drawRmap", "R Range Minimap").SetValue(true));
            DrawMenu.AddItem(new MenuItem("drawOnlyReady", "Ready Only").SetValue(true));
            TwistedFateMenu.AddSubMenu(DrawMenu);

            TwistedFateMenu.AddToMainMenu();
        }

        #region Getters

        internal static int GetSliderValue(string itemName)
        {
            return TwistedFateMenu.Item(itemName).GetValue<Slider>().Value;
        }

        internal static int GetStringListValue(string itemName)
        {
            return TwistedFateMenu.Item(itemName).GetValue<StringList>().SelectedIndex;
        }

        internal static bool IsChecked(string itemName)
        {
            return TwistedFateMenu.Item(itemName).GetValue<bool>();
        }

        internal static bool IsKeyPressed(string itemName)
        {
            return TwistedFateMenu.Item(itemName).GetValue<KeyBind>().Active;
        }

        #endregion

        #endregion
    }
}