using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using UnityEngine;

namespace AnglersToolkit
{
    internal static class MenuGUI
    {
        private const int WindowId = 0x485446; // "HTF"

        private static readonly string[] TabNames =
        {
            "Fishing", "Money", "Unlocks", "Spawn", "Player", "Help"
        };

        private static readonly string[] PageTitles =
        {
            "Fishing", "Money", "Unlocks", "Spawn", "Player", "Help"
        };

        private static readonly string[] PageSubtitles =
        {
            "Automation, catch tweaks, and value multipliers.",
            "Top up your wallet without grinding.",
            "Unlock progression, cosmetics, and boss tools.",
            "Search the game's items and creatures, then spawn them in front of you.",
            "Survival, travel, and session cheats.",
            "Controls, developer commands, and configuration."
        };

        private static readonly Color TextColor = new Color(0.91f, 0.95f, 0.98f);
        private static readonly Color MutedColor = new Color(0.55f, 0.64f, 0.72f);
        private static readonly Color AccentColor = new Color(0.30f, 0.89f, 0.76f);
        private static readonly Color GoodColor = new Color(0.42f, 0.92f, 0.62f);
        private static readonly Color WarningColor = new Color(0.96f, 0.70f, 0.37f);
        private static readonly Color DangerColor = new Color(1f, 0.46f, 0.43f);
        private static readonly Color BorderColor = new Color(0.14f, 0.20f, 0.26f, 0.95f);

        private static Rect _windowRect;
        private static bool _hasWindowPosition;
        private static float _lastScreenWidth = -1f;
        private static float _lastScreenHeight = -1f;
        private static int _selectedTab;
        private static Vector2 _contentScroll;
        private static Vector2 _spawnScroll;
        private static string _spawnSearch = string.Empty;
        private static bool _spawnDead;
        private static string _spawnError;
        private static List<SpawnEntry> _spawnEntries;
        private static FieldInfo _itemWeightField;
        private static bool _loggedScreenInfo;

        private static Font _uiFont;
        private static Font _monoFont;

        private static Texture2D _windowTexture;
        private static Texture2D _cardTexture;
        private static Texture2D _buttonTexture;
        private static Texture2D _buttonHoverTexture;
        private static Texture2D _accentTexture;
        private static Texture2D _primaryButtonTexture;
        private static Texture2D _primaryButtonHoverTexture;
        private static Texture2D _dangerButtonTexture;
        private static Texture2D _dangerButtonHoverTexture;
        private static Texture2D _selectedNavTexture;
        private static Texture2D _navHoverTexture;
        private static Texture2D _fieldTexture;
        private static Texture2D _switchOffTexture;
        private static Texture2D _switchOnTexture;
        private static Texture2D _sliderTrackTexture;
        private static Texture2D _sliderThumbTexture;
        private static Texture2D _hudTexture;
        private static Texture2D _toastTexture;
        private static Texture2D _goodChipTexture;
        private static Texture2D _warnChipTexture;
        private static Texture2D _dividerTexture;
        private static Texture2D _smallButtonTexture;
        private static Texture2D _smallButtonHoverTexture;
        private static Texture2D _deleteTexture;

        private static GUIStyle _windowStyle;
        private static GUIStyle _titleStyle;
        private static GUIStyle _subtitleStyle;
        private static GUIStyle _pageTitleStyle;
        private static GUIStyle _pageSubtitleStyle;
        private static GUIStyle _cardStyle;
        private static GUIStyle _cardTitleStyle;
        private static GUIStyle _accentDotStyle;
        private static GUIStyle _rowLabelStyle;
        private static GUIStyle _rowDescriptionStyle;
        private static GUIStyle _valueStyle;
        private static GUIStyle _bigValueStyle;
        private static GUIStyle _mediumValueStyle;
        private static GUIStyle _buttonStyle;
        private static GUIStyle _primaryButtonStyle;
        private static GUIStyle _dangerButtonStyle;
        private static GUIStyle _smallButtonStyle;
        private static GUIStyle _smallAccentButtonStyle;
        private static GUIStyle _closeStyle;
        private static GUIStyle _navStyle;
        private static GUIStyle _navSelectedStyle;
        private static GUIStyle _switchStyle;
        private static GUIStyle _fieldStyle;
        private static GUIStyle _sliderStyle;
        private static GUIStyle _sliderThumbStyle;
        private static GUIStyle _statusHostStyle;
        private static GUIStyle _statusClientStyle;
        private static GUIStyle _sidebarHintStyle;
        private static GUIStyle _warningStyle;
        private static GUIStyle _dangerStyle;
        private static GUIStyle _hintStyle;
        private static GUIStyle _columnHeaderStyle;
        private static GUIStyle _codeStyle;
        private static GUIStyle _hudBoxStyle;
        private static GUIStyle _hudTitleStyle;
        private static GUIStyle _hudNameStyle;
        private static GUIStyle _hudLineStyle;
        private static GUIStyle _toastStyle;

        private sealed class SpawnEntry
        {
            public Item Prefab;
            public string Name;
            public string AssetName;
            public string TypeLabel;
            public bool IsCreature;
        }

        internal static void OnOpened()
        {
            _contentScroll = Vector2.zero;

            if (!_loggedScreenInfo)
            {
                _loggedScreenInfo = true;
                Debug.Log(
                    "[Angler's Toolkit] Screen " + Screen.width + "x" + Screen.height +
                    ", dpi " + Screen.dpi.ToString("0") +
                    ", ui scale " + GetGuiScale().ToString("0.00"));
            }
        }

        internal static void Draw()
        {
            EnsureStyles();

            if (!Plugin.MenuOpen)
            {
                DrawCatchHud();
                DrawToast();
                return;
            }

            UpdateWindowRect();

            GUI.depth = -1000;
            _windowRect = GUI.Window(WindowId, _windowRect, DrawWindow, GUIContent.none, _windowStyle);
            DrawToast();
        }

        private static void UpdateWindowRect()
        {
            float scale = GetGuiScale();
            float screenWidth = Screen.width / scale;
            float screenHeight = Screen.height / scale;

            if (_hasWindowPosition &&
                Math.Abs(screenWidth - _lastScreenWidth) < 1f &&
                Math.Abs(screenHeight - _lastScreenHeight) < 1f)
            {
                return;
            }

            float width = Mathf.Clamp(screenWidth - 96f, 760f, 1000f);
            float height = Mathf.Clamp(screenHeight - 96f, 500f, 680f);
            _windowRect = new Rect(
                Mathf.Max(12f, (screenWidth - width) * 0.5f),
                Mathf.Max(12f, (screenHeight - height) * 0.5f),
                width,
                height);

            _lastScreenWidth = screenWidth;
            _lastScreenHeight = screenHeight;
            _hasWindowPosition = true;
        }

        private static void DrawWindow(int id)
        {
            float width = _windowRect.width;
            float height = _windowRect.height;

            DrawHeader(width);
            DrawSidebar(height);
            DrawContent(width, height);

            GUI.DragWindow(new Rect(0f, 0f, width - 62f, 76f));
        }

        private static void DrawHeader(float width)
        {
            GUI.DrawTexture(new Rect(22f, 20f, 3f, 24f), _accentTexture, ScaleMode.StretchToFill, true);
            GUI.Label(
                new Rect(34f, 12f, width - 100f, 34f),
                "<b>ANGLER'S</b> <color=#4DE3C2>TOOLKIT</color>",
                _titleStyle);
            GUI.Label(
                new Rect(36f, 45f, width - 100f, 18f),
                "HOW TO FISH   /   v1.0.0",
                _subtitleStyle);

            if (GUI.Button(new Rect(width - 52f, 18f, 30f, 30f), "×", _closeStyle))
            {
                Plugin.ToggleMenu();
            }

            GUI.DrawTexture(
                new Rect(22f, 79f, width - 44f, 1f),
                _dividerTexture,
                ScaleMode.StretchToFill,
                true);
        }

        private static void DrawSidebar(float height)
        {
            GUILayout.BeginArea(new Rect(22f, 96f, 188f, height - 118f));
            GUILayout.BeginVertical();

            for (int i = 0; i < TabNames.Length; i++)
            {
                bool selected = _selectedTab == i;
                if (GUILayout.Button(
                    TabNames[i],
                    selected ? _navSelectedStyle : _navStyle,
                    GUILayout.Height(38f)))
                {
                    _selectedTab = i;
                    _contentScroll = Vector2.zero;
                }

                GUILayout.Space(2f);
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label(
                Plugin.IsHost ? "HOST MODE" : "CLIENT MODE",
                Plugin.IsHost ? _statusHostStyle : _statusClientStyle,
                GUILayout.Height(26f));
            GUILayout.Space(8f);
            GUILayout.Label("F8  /  CLOSE", _sidebarHintStyle);
            GUILayout.EndVertical();
            GUILayout.EndArea();

            GUI.DrawTexture(
                new Rect(218f, 96f, 1f, height - 118f),
                _dividerTexture,
                ScaleMode.StretchToFill,
                true);
        }

        private static void DrawContent(float width, float height)
        {
            GUILayout.BeginArea(new Rect(240f, 94f, width - 264f, height - 114f));

            GUILayout.Label(PageTitles[_selectedTab], _pageTitleStyle);
            GUILayout.Label(PageSubtitles[_selectedTab], _pageSubtitleStyle);
            GUILayout.Space(12f);

            _contentScroll = GUILayout.BeginScrollView(
                _contentScroll,
                false,
                false,
                GUILayout.ExpandHeight(true));

            switch (_selectedTab)
            {
                case 0:
                    DrawFishingTab();
                    break;
                case 1:
                    DrawMoneyTab();
                    break;
                case 2:
                    DrawUnlocksTab();
                    break;
                case 3:
                    DrawSpawnTab();
                    break;
                case 4:
                    DrawPlayerTab();
                    break;
                default:
                    DrawHelpTab();
                    break;
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private static void DrawFishingTab()
        {
            BeginCard("AUTOMATION");
            SwitchRow(Plugin.AutoReel, "Auto Reel", "Holds the reel automatically when something bites.");
            SwitchRow(Plugin.ShowCatchHud, "Catch HUD", "Shows the name, weight, and value of your current catch.");
            EndCard();

            BeginCard("CATCH TWEAKS");
            SwitchRow(Plugin.InstantBite, "Instant Bite", "Fish bite as soon as the bait is underwater. Host/server only.");
            SwitchRow(Plugin.AlwaysShiny, "Always Shiny", "Every new creature is the rare shiny variant. Host/server only.");
            SwitchRow(Plugin.AutoKill, "Auto Kill Catches", "Caught creatures arrive dead and ready to pick up. Host/server only.");
            SwitchRow(Plugin.AutoCollect, "Auto Collect", "Picks up dead catches when they are close. Host/server only.");
            EndCard();

            BeginCard("VALUE MULTIPLIERS");
            SliderRow(Plugin.WeightMultiplier, "Weight", "Multiplies the randomized weight of new creatures.", 1f, 50f);
            SliderRow(Plugin.SellMultiplier, "Sell value", "Multiplies the value used when items are sold.", 1f, 50f);
            EndCard();
        }

        private static void DrawMoneyTab()
        {
            BeginCard("BALANCE");
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label("DOLLARS", _cardTitleStyle);
            GUILayout.Label("$" + MoneyManager.Money.ToString("N0"), _bigValueStyle);
            GUILayout.EndVertical();
            GUILayout.Space(56f);
            GUILayout.BeginVertical();
            GUILayout.Label("EUROS", _cardTitleStyle);
            GUILayout.Label(MoneyManager.Euro.ToString("N0"), _mediumValueStyle);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EndCard();

            BeginCard("ADD MONEY");
            if (!Plugin.IsHost)
            {
                GUILayout.Label("Host a game first to add money.", _warningStyle);
                GUILayout.Space(8f);
            }

            GUILayout.BeginHorizontal();
            MoneyButton("+ $1,000", 1000, Currency.Dollar);
            MoneyButton("+ $10,000", 10000, Currency.Dollar);
            MoneyButton("+ $100,000", 100000, Currency.Dollar);
            MoneyButton("+ $1,000,000", 1000000, Currency.Dollar);
            GUILayout.EndHorizontal();

            GUILayout.Space(8f);

            GUILayout.BeginHorizontal();
            MoneyButton("+ €100", 100, Currency.Euro);
            MoneyButton("+ €1,000", 1000, Currency.Euro);
            MoneyButton("+ €10,000", 10000, Currency.Euro);
            MoneyButton("+ €100,000", 100000, Currency.Euro);
            GUILayout.EndHorizontal();
            EndCard();

            BeginCard("BUILT-IN KEYS");
            GUILayout.Label("M  adds $99,999   /   Ctrl+M  adds €999   /   N  removes money.", _rowDescriptionStyle);
            EndCard();
        }

        private static void DrawUnlocksTab()
        {
            BeginCard("PROGRESSION");
            if (!Plugin.IsHost)
            {
                GUILayout.Label("These actions need you to be hosting a lobby.", _warningStyle);
                GUILayout.Space(8f);
            }

            GUILayout.BeginHorizontal();
            Button("Unlock All Islands", MenuActions.UnlockAllIslands, GUILayout.ExpandWidth(true));
            Button("Boat + Radar + Motor", MenuActions.UnlockBoat, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.Space(8f);

            GUILayout.BeginHorizontal();
            Button("Unlock the Grill", MenuActions.UnlockGrill, GUILayout.ExpandWidth(true));
            DangerButton("Kill Current Boss", MenuActions.KillBoss, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            EndCard();

            BeginCard("COSMETICS & JOURNAL");
            GUILayout.BeginHorizontal();
            Button("Unlock All Skins", MenuActions.UnlockAllSkins, GUILayout.ExpandWidth(true));
            Button("Unlock Achievements", MenuActions.UnlockAchievements, GUILayout.ExpandWidth(true));
            Button("Complete Fish Journal", MenuActions.CompleteFishJournal, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            GUILayout.Space(6f);
            GUILayout.Label("Skins and achievements are saved locally. The journal option marks every entry as found.", _rowDescriptionStyle);
            EndCard();
        }

        private static void DrawSpawnTab()
        {
            BeginCard("SEARCH");
            GUILayout.Label("Find an item or creature by name, then spawn it in front of the camera.", _rowDescriptionStyle);
            GUILayout.Space(6f);

            GUILayout.BeginHorizontal();
            _spawnSearch = GUILayout.TextField(_spawnSearch ?? string.Empty, _fieldStyle);
            if (GUILayout.Button("Clear", _smallButtonStyle, GUILayout.Width(64f)))
            {
                _spawnSearch = string.Empty;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4f);
            SwitchRow("Spawn as dead", "Creatures arrive dead so they can be picked up immediately.", ref _spawnDead);
            EndCard();

            BeginCard("RESULTS");
            EnsureSpawnEntries();

            if (!string.IsNullOrEmpty(_spawnError))
            {
                GUILayout.Label("Spawn list unavailable: " + _spawnError, _dangerStyle);
                EndCard();
                return;
            }

            if (_spawnEntries == null || _spawnEntries.Count == 0)
            {
                GUILayout.Label("Loading spawn list...", _rowDescriptionStyle);
                EndCard();
                return;
            }

            int shown = 0;
            for (int i = 0; i < _spawnEntries.Count; i++)
            {
                if (MatchesSearch(_spawnEntries[i]))
                {
                    shown++;
                }
            }

            GUILayout.Label(
                shown + " of " + _spawnEntries.Count + " spawnables",
                _rowDescriptionStyle);
            GUILayout.Space(4f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("NAME", _columnHeaderStyle, GUILayout.ExpandWidth(true));
            GUILayout.Label("TYPE", _columnHeaderStyle, GUILayout.Width(88f));
            GUILayout.Space(136f);
            GUILayout.EndHorizontal();

            GUI.DrawTexture(
                new Rect(GUILayoutUtility.GetLastRect().x, GUILayoutUtility.GetLastRect().yMax, 1f, 1f),
                _dividerTexture);

            _spawnScroll = GUILayout.BeginScrollView(
                _spawnScroll,
                false,
                true,
                GUILayout.Height(292f));

            int rendered = 0;
            for (int i = 0; i < _spawnEntries.Count; i++)
            {
                SpawnEntry entry = _spawnEntries[i];
                if (!MatchesSearch(entry))
                {
                    continue;
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label(entry.Name, _rowLabelStyle, GUILayout.ExpandWidth(true));
                GUILayout.Label(entry.TypeLabel, _rowDescriptionStyle, GUILayout.Width(88f));

                if (GUILayout.Button("Spawn", _smallAccentButtonStyle, GUILayout.Width(68f)))
                {
                    MenuActions.SpawnItem(entry.Prefab, _spawnDead);
                }

                if (GUILayout.Button("Dead", _smallButtonStyle, GUILayout.Width(52f)))
                {
                    MenuActions.SpawnItem(entry.Prefab, true);
                }

                GUILayout.EndHorizontal();

                rendered++;
                if (rendered >= 160)
                {
                    GUILayout.Label("Refine the search to see more results.", _rowDescriptionStyle);
                    break;
                }
            }

            if (rendered == 0)
            {
                GUILayout.Label("Nothing matches that search.", _rowDescriptionStyle);
            }

            GUILayout.EndScrollView();
            EndCard();
        }

        private static void DrawPlayerTab()
        {
            BeginCard("SURVIVAL");
            GUILayout.BeginHorizontal();
            PrimaryButton("Heal & Feed Self", MenuActions.HealSelf, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            GUILayout.Space(6f);
            GUILayout.Label("Restores health and fullness to your local character.", _rowDescriptionStyle);
            EndCard();

            BeginCard("TRAVEL");
            GUILayout.BeginHorizontal();
            Button("Previous Island", () => MenuActions.TeleportIsland(true), GUILayout.ExpandWidth(true));
            Button("Next Island", () => MenuActions.TeleportIsland(false), GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            GUILayout.Space(6f);
            GUILayout.Label("Travel moves the whole lobby. Host only.", _rowDescriptionStyle);
            EndCard();

            BeginCard("CHEAT MODES");
            StateSwitchRow(
                "God Mode",
                "Prevents damage to players. Host/server only.",
                PlayerManager.InGodMode,
                MenuActions.ToggleGodMode);
            StateSwitchRow(
                "One Shot",
                "Makes most damage lethal. Host/server only.",
                OneShotEnabled(),
                MenuActions.ToggleOneShot);
            EndCard();
        }

        private static void DrawHelpTab()
        {
            BeginCard("QUICK START");
            GUILayout.Label("F8 opens and closes this menu.", _rowLabelStyle);
            GUILayout.Label("Gameplay input is blocked while the menu is open and the mouse cursor is freed.", _rowDescriptionStyle);
            GUILayout.Space(6f);
            GUILayout.Label("Buttons marked host/server only need you to play solo or host a lobby.", _rowDescriptionStyle);
            EndCard();

            BeginCard("DEVELOPER CHEATS");
            GUILayout.Label(
                "M  +$99,999\n" +
                "Ctrl+M  +€999\n" +
                "N  remove money\n" +
                "O  next island  /  Shift+O  random mini island\n" +
                "T  slow motion  /  G  test damage  /  H  heal",
                _codeStyle);
            GUILayout.Space(8f);
            GUILayout.Label(
                "/spawn <name>   /spawndead <name>   /spawndrip <name>\n" +
                "/addmoney   /nextisland   /previsland   /godmode   /oneshot\n" +
                "/killboss   /allskins   /unlockachievements   /finishgame",
                _codeStyle);
            EndCard();

            BeginCard("CONFIG");
            GUILayout.Label(
                "Settings are saved to BepInEx\\config\\dazed.howtofish.anglerstoolkit.cfg",
                _rowDescriptionStyle);
            EndCard();
        }

        private static void BeginCard(string title)
        {
            GUILayout.BeginVertical(_cardStyle);

            if (!string.IsNullOrEmpty(title))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("●", _accentDotStyle, GUILayout.Width(12f));
                GUILayout.Label(title, _cardTitleStyle);
                GUILayout.EndHorizontal();
                GUILayout.Space(10f);
            }
        }

        private static void EndCard()
        {
            GUILayout.EndVertical();
            GUILayout.Space(12f);
        }

        private static void SwitchRow(ConfigEntry<bool> entry, string label, string description)
        {
            if (entry == null)
            {
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label(label, _rowLabelStyle);
            if (!string.IsNullOrEmpty(description))
            {
                GUILayout.Label(description, _rowDescriptionStyle);
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Space(8f);
            GUILayout.BeginVertical();
            GUILayout.Space(3f);
            bool value = GUILayout.Toggle(
                entry.Value,
                GUIContent.none,
                _switchStyle,
                GUILayout.Width(46f),
                GUILayout.Height(24f));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (value != entry.Value)
            {
                entry.Value = value;
            }

            GUILayout.Space(8f);
        }

        private static void SwitchRow(string label, string description, ref bool value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label(label, _rowLabelStyle);
            if (!string.IsNullOrEmpty(description))
            {
                GUILayout.Label(description, _rowDescriptionStyle);
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Space(8f);
            GUILayout.BeginVertical();
            GUILayout.Space(3f);
            value = GUILayout.Toggle(
                value,
                GUIContent.none,
                _switchStyle,
                GUILayout.Width(46f),
                GUILayout.Height(24f));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(8f);
        }

        private static void StateSwitchRow(string label, string description, bool value, Action toggle)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label(label, _rowLabelStyle);
            if (!string.IsNullOrEmpty(description))
            {
                GUILayout.Label(description, _rowDescriptionStyle);
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Space(8f);
            GUILayout.BeginVertical();
            GUILayout.Space(3f);
            bool newValue = GUILayout.Toggle(
                value,
                GUIContent.none,
                _switchStyle,
                GUILayout.Width(46f),
                GUILayout.Height(24f));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (newValue != value)
            {
                toggle();
            }

            GUILayout.Space(8f);
        }

        private static void SliderRow(
            ConfigEntry<float> entry,
            string label,
            string description,
            float minimum,
            float maximum)
        {
            if (entry == null)
            {
                return;
            }

            float value = entry.Value;

            GUILayout.BeginHorizontal();
            GUILayout.Label(label, _rowLabelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label(value.ToString("0.0") + "x", _valueStyle);
            GUILayout.EndHorizontal();

            float newValue = GUILayout.HorizontalSlider(
                value,
                minimum,
                maximum,
                _sliderStyle,
                _sliderThumbStyle,
                GUILayout.Height(18f));
            newValue = Mathf.Round(newValue * 10f) / 10f;

            if (Math.Abs(newValue - entry.Value) > 0.001f)
            {
                entry.Value = newValue;
            }

            if (!string.IsNullOrEmpty(description))
            {
                GUILayout.Label(description, _rowDescriptionStyle);
            }

            GUILayout.Space(10f);
        }

        private static void MoneyButton(string label, int amount, Currency currency)
        {
            Button(label, () => MenuActions.AddMoney(amount, currency), GUILayout.ExpandWidth(true));
        }

        private static void Button(string label, Action action, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(label, _buttonStyle, options))
            {
                action();
            }
        }

        private static void PrimaryButton(string label, Action action, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(label, _primaryButtonStyle, options))
            {
                action();
            }
        }

        private static void DangerButton(string label, Action action, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(label, _dangerButtonStyle, options))
            {
                action();
            }
        }

        private static bool OneShotEnabled()
        {
            try
            {
                return ServerSettings.Instance != null && ServerSettings.OneShotEnabled;
            }
            catch
            {
                return false;
            }
        }

        private static void DrawCatchHud()
        {
            if (Plugin.ShowCatchHud == null || !Plugin.ShowCatchHud.Value)
            {
                return;
            }

            Player player = Player.LocalPlayer;
            if (player == null || player.Holding == null)
            {
                return;
            }

            Item held = player.Holding.HeldItem;
            if (held == null || held.FishingRod == null)
            {
                return;
            }

            Bait bait = held.FishingRod.Bait;
            if (bait == null || bait.ItemOnBait == null)
            {
                return;
            }

            Item hooked = bait.ItemOnBait;
            string name = SafeName(hooked);
            string extra = string.Empty;

            if (hooked.Creature != null)
            {
                if (hooked.Creature.IsDrip)
                {
                    extra += "   <color=#4DE3C2>SHINY</color>";
                }

                if (hooked.Creature.BossType != BossType.None)
                {
                    extra += "   <color=#F2B65A>BOSS</color>";
                }
            }

            float weight = 0f;
            if (_itemWeightField == null)
            {
                _itemWeightField = typeof(Item).GetField(
                    "_weight",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            }

            if (_itemWeightField != null)
            {
                object raw = _itemWeightField.GetValue(hooked);
                if (raw is float)
                {
                    weight = (float)raw;
                }
            }

            weight *= hooked.RandomizedWeight;

            float distance = 0f;
            if (player.Transform != null)
            {
                distance = Vector3.Distance(hooked.transform.position, player.Transform.position);
            }

            Rect rect = new Rect(18f, 18f, 412f, 116f);
            GUI.Box(rect, GUIContent.none, _hudBoxStyle);
            GUI.DrawTexture(
                new Rect(rect.x + 1f, rect.y + 16f, 3f, rect.height - 32f),
                _accentTexture,
                ScaleMode.StretchToFill,
                true);

            GUI.Label(
                new Rect(rect.x + 16f, rect.y + 10f, rect.width - 28f, 16f),
                "ON THE LINE",
                _hudTitleStyle);
            GUI.Label(
                new Rect(rect.x + 16f, rect.y + 29f, rect.width - 28f, 24f),
                name + extra,
                _hudNameStyle);
            GUI.Label(
                new Rect(rect.x + 16f, rect.y + 57f, rect.width - 28f, 18f),
                "Weight  " + weight.ToString("0.00") + " kg",
                _hudLineStyle);
            GUI.Label(
                new Rect(rect.x + 16f, rect.y + 77f, rect.width - 28f, 18f),
                "Value  $" + hooked.TotalWorth.ToString("N0"),
                _hudLineStyle);
            GUI.Label(
                new Rect(rect.x + 16f, rect.y + 95f, rect.width - 28f, 16f),
                distance.ToString("0.0") + " m away" +
                (Plugin.AutoReel != null && Plugin.AutoReel.Value ? "   /   auto-reeling" : string.Empty),
                _hudTitleStyle);
        }

        private static void DrawToast()
        {
            if (string.IsNullOrEmpty(Plugin.ToastText) || Time.unscaledTime >= Plugin.ToastUntil)
            {
                return;
            }

            EnsureStyles();
            float scale = GetGuiScale();
            float screenWidth = Screen.width / scale;
            float screenHeight = Screen.height / scale;
            float width = Mathf.Min(560f, screenWidth - 48f);
            Rect rect = new Rect(
                (screenWidth - width) * 0.5f,
                screenHeight - 72f,
                width,
                40f);
            GUI.Label(rect, Plugin.ToastText, _toastStyle);
        }

        private static void EnsureStyles()
        {
            if (_windowStyle != null)
            {
                return;
            }

            _uiFont = LoadFont(new[] { "Segoe UI", "Arial" }, 14);
            _monoFont = LoadFont(new[] { "Cascadia Mono", "Consolas", "Courier New" }, 12);

            _windowTexture = MakeRoundedTexture(
                128, 26, 2,
                new Color(0.035f, 0.050f, 0.068f, 0.985f),
                new Color(0.12f, 0.18f, 0.23f, 1f));
            _cardTexture = MakeRoundedTexture(
                64, 13, 2,
                new Color(0.065f, 0.085f, 0.108f, 0.96f),
                new Color(0.13f, 0.18f, 0.23f, 0.9f));
            _buttonTexture = MakeRoundedTexture(
                40, 9, 2,
                new Color(0.10f, 0.14f, 0.18f, 0.98f),
                new Color(0.18f, 0.24f, 0.30f, 0.95f));
            _buttonHoverTexture = MakeRoundedTexture(
                40, 9, 2,
                new Color(0.13f, 0.22f, 0.24f, 1f),
                new Color(0.25f, 0.54f, 0.50f, 0.95f));
            _primaryButtonTexture = MakeRoundedTexture(
                40, 9, 2,
                AccentColor,
                new Color(0.55f, 1f, 0.90f, 0.9f));
            _primaryButtonHoverTexture = MakeRoundedTexture(
                40, 9, 2,
                new Color(0.45f, 0.98f, 0.84f, 1f),
                new Color(0.70f, 1f, 0.94f, 1f));
            _dangerButtonTexture = MakeRoundedTexture(
                40, 9, 2,
                new Color(0.16f, 0.08f, 0.09f, 0.98f),
                new Color(0.52f, 0.22f, 0.22f, 0.98f));
            _dangerButtonHoverTexture = MakeRoundedTexture(
                40, 9, 2,
                new Color(0.30f, 0.10f, 0.11f, 1f),
                DangerColor);
            _smallButtonTexture = MakeRoundedTexture(
                32, 8, 2,
                new Color(0.10f, 0.14f, 0.18f, 0.98f),
                new Color(0.18f, 0.24f, 0.30f, 0.95f));
            _smallButtonHoverTexture = MakeRoundedTexture(
                32, 8, 2,
                new Color(0.13f, 0.22f, 0.24f, 1f),
                new Color(0.25f, 0.54f, 0.50f, 0.95f));
            _selectedNavTexture = MakeRoundedTexture(
                40, 9, 2,
                new Color(0.08f, 0.20f, 0.19f, 0.98f),
                new Color(0.20f, 0.58f, 0.52f, 0.95f));
            _navHoverTexture = MakeRoundedTexture(
                40, 9, 2,
                new Color(0.09f, 0.13f, 0.17f, 1f),
                new Color(0.16f, 0.23f, 0.28f, 0.9f));
            _fieldTexture = MakeRoundedTexture(
                40, 8, 2,
                new Color(0.025f, 0.038f, 0.052f, 1f),
                new Color(0.15f, 0.21f, 0.27f, 1f));
            _hudTexture = MakeRoundedTexture(
                48, 12, 2,
                new Color(0.035f, 0.052f, 0.068f, 0.88f),
                new Color(0.18f, 0.32f, 0.32f, 0.9f));
            _toastTexture = MakeRoundedTexture(
                48, 12, 2,
                new Color(0.035f, 0.052f, 0.068f, 0.97f),
                new Color(0.26f, 0.62f, 0.55f, 0.95f));
            _goodChipTexture = MakeRoundedTexture(
                32, 8, 2,
                new Color(0.06f, 0.18f, 0.12f, 0.95f),
                new Color(0.22f, 0.50f, 0.32f, 0.95f));
            _warnChipTexture = MakeRoundedTexture(
                32, 8, 2,
                new Color(0.20f, 0.13f, 0.05f, 0.95f),
                new Color(0.55f, 0.38f, 0.14f, 0.95f));
            _dividerTexture = MakeTexture(new Color(0.13f, 0.18f, 0.23f, 0.95f));
            _accentTexture = MakeTexture(AccentColor);
            _deleteTexture = MakeTexture(DangerColor);

            _switchOffTexture = MakeSwitchTexture(false);
            _switchOnTexture = MakeSwitchTexture(true);
            _sliderTrackTexture = MakeSliderTrackTexture();
            _sliderThumbTexture = MakeCircleTexture(32, new Color(0.82f, 0.94f, 0.92f, 1f));

            _windowStyle = new GUIStyle();
            _windowStyle.normal.background = _windowTexture;
            _windowStyle.border = new RectOffset(28, 28, 28, 28);
            _windowStyle.padding = new RectOffset(0, 0, 0, 0);
            _windowStyle.margin = new RectOffset(0, 0, 0, 0);

            _titleStyle = NewLabelStyle(20, FontStyle.Bold, TextColor);
            _subtitleStyle = NewLabelStyle(10, FontStyle.Normal, MutedColor);
            _pageTitleStyle = NewLabelStyle(23, FontStyle.Bold, TextColor);
            _pageSubtitleStyle = NewLabelStyle(11, FontStyle.Normal, MutedColor);
            _cardTitleStyle = NewLabelStyle(10, FontStyle.Bold, AccentColor);
            _accentDotStyle = NewLabelStyle(10, FontStyle.Bold, AccentColor);
            _rowLabelStyle = NewLabelStyle(13, FontStyle.Normal, TextColor);
            _rowDescriptionStyle = NewLabelStyle(11, FontStyle.Normal, MutedColor);
            _rowDescriptionStyle.wordWrap = true;
            _valueStyle = NewLabelStyle(13, FontStyle.Bold, AccentColor);
            _bigValueStyle = NewLabelStyle(29, FontStyle.Bold, AccentColor);
            _mediumValueStyle = NewLabelStyle(22, FontStyle.Bold, TextColor);
            _warningStyle = NewLabelStyle(11, FontStyle.Normal, WarningColor);
            _dangerStyle = NewLabelStyle(11, FontStyle.Normal, DangerColor);
            _hintStyle = NewLabelStyle(10, FontStyle.Normal, MutedColor);
            _columnHeaderStyle = NewLabelStyle(9, FontStyle.Bold, MutedColor);

            _cardStyle = new GUIStyle();
            _cardStyle.normal.background = _cardTexture;
            _cardStyle.border = new RectOffset(16, 16, 16, 16);
            _cardStyle.padding = new RectOffset(18, 18, 16, 16);
            _cardStyle.margin = new RectOffset(0, 0, 0, 0);

            _buttonStyle = NewButtonStyle(_buttonTexture, _buttonHoverTexture, _buttonTexture, TextColor);
            _primaryButtonStyle = NewButtonStyle(_primaryButtonTexture, _primaryButtonHoverTexture, _primaryButtonTexture, new Color(0.025f, 0.10f, 0.08f));
            _dangerButtonStyle = NewButtonStyle(_dangerButtonTexture, _dangerButtonHoverTexture, _dangerButtonTexture, DangerColor);
            _smallButtonStyle = NewSmallButtonStyle(_smallButtonTexture, _smallButtonHoverTexture, TextColor);
            _smallAccentButtonStyle = NewSmallButtonStyle(_smallButtonTexture, _smallButtonHoverTexture, AccentColor);
            _closeStyle = NewButtonStyle(_buttonTexture, _dangerButtonHoverTexture, _dangerButtonTexture, MutedColor);
            _closeStyle.fontSize = 17;
            _closeStyle.fixedWidth = 30f;
            _closeStyle.fixedHeight = 30f;
            _closeStyle.padding = new RectOffset(0, 0, 4, 4);

            _navStyle = new GUIStyle(GUI.skin.button);
            _navStyle.font = _uiFont;
            _navStyle.fontSize = 13;
            _navStyle.alignment = TextAnchor.MiddleLeft;
            _navStyle.padding = new RectOffset(14, 8, 4, 4);
            _navStyle.margin = new RectOffset(0, 0, 0, 0);
            _navStyle.normal.background = null;
            _navStyle.hover.background = _navHoverTexture;
            _navStyle.active.background = _navHoverTexture;
            _navStyle.normal.textColor = MutedColor;
            _navStyle.hover.textColor = TextColor;
            _navStyle.active.textColor = TextColor;

            _navSelectedStyle = new GUIStyle(_navStyle);
            _navSelectedStyle.normal.background = _selectedNavTexture;
            _navSelectedStyle.hover.background = _selectedNavTexture;
            _navSelectedStyle.active.background = _selectedNavTexture;
            _navSelectedStyle.normal.textColor = AccentColor;
            _navSelectedStyle.hover.textColor = AccentColor;
            _navSelectedStyle.active.textColor = AccentColor;
            _navSelectedStyle.fontStyle = FontStyle.Bold;

            _switchStyle = new GUIStyle(GUI.skin.toggle);
            _switchStyle.font = _uiFont;
            _switchStyle.normal.background = _switchOffTexture;
            _switchStyle.hover.background = _switchOffTexture;
            _switchStyle.active.background = _switchOffTexture;
            _switchStyle.onNormal.background = _switchOnTexture;
            _switchStyle.onHover.background = _switchOnTexture;
            _switchStyle.onActive.background = _switchOnTexture;
            _switchStyle.border = new RectOffset(0, 0, 0, 0);
            _switchStyle.padding = new RectOffset(0, 0, 0, 0);
            _switchStyle.margin = new RectOffset(0, 0, 0, 0);
            _switchStyle.fixedWidth = 46f;
            _switchStyle.fixedHeight = 24f;

            _fieldStyle = new GUIStyle(GUI.skin.textField);
            _fieldStyle.font = _uiFont;
            _fieldStyle.fontSize = 13;
            _fieldStyle.fixedHeight = 32f;
            _fieldStyle.padding = new RectOffset(11, 11, 6, 6);
            _fieldStyle.alignment = TextAnchor.MiddleLeft;
            _fieldStyle.normal.background = _fieldTexture;
            _fieldStyle.focused.background = _fieldTexture;
            _fieldStyle.hover.background = _fieldTexture;
            _fieldStyle.normal.textColor = TextColor;
            _fieldStyle.focused.textColor = TextColor;
            _fieldStyle.hover.textColor = TextColor;

            _sliderStyle = new GUIStyle();
            _sliderStyle.normal.background = _sliderTrackTexture;
            _sliderStyle.border = new RectOffset(12, 12, 0, 0);
            _sliderStyle.fixedHeight = 18f;
            _sliderStyle.margin = new RectOffset(0, 0, 0, 0);

            _sliderThumbStyle = new GUIStyle();
            _sliderThumbStyle.normal.background = _sliderThumbTexture;
            _sliderThumbStyle.hover.background = _sliderThumbTexture;
            _sliderThumbStyle.active.background = _sliderThumbTexture;
            _sliderThumbStyle.fixedWidth = 16f;
            _sliderThumbStyle.fixedHeight = 16f;
            _sliderThumbStyle.border = new RectOffset(0, 0, 0, 0);

            _statusHostStyle = NewChipStyle(_goodChipTexture, GoodColor);
            _statusClientStyle = NewChipStyle(_warnChipTexture, WarningColor);
            _sidebarHintStyle = NewLabelStyle(9, FontStyle.Bold, MutedColor);
            _sidebarHintStyle.alignment = TextAnchor.MiddleCenter;

            _codeStyle = new GUIStyle(GUI.skin.label);
            _codeStyle.font = _monoFont != null ? _monoFont : _uiFont;
            _codeStyle.fontSize = 11;
            _codeStyle.richText = false;
            _codeStyle.wordWrap = true;
            _codeStyle.normal.textColor = new Color(0.72f, 0.82f, 0.86f);

            _hudBoxStyle = new GUIStyle(GUI.skin.box);
            _hudBoxStyle.normal.background = _hudTexture;
            _hudBoxStyle.border = new RectOffset(12, 12, 12, 12);
            _hudBoxStyle.padding = new RectOffset(10, 10, 8, 8);

            _hudTitleStyle = NewLabelStyle(9, FontStyle.Bold, AccentColor);
            _hudNameStyle = NewLabelStyle(15, FontStyle.Bold, TextColor);
            _hudNameStyle.clipping = TextClipping.Clip;
            _hudLineStyle = NewLabelStyle(11, FontStyle.Normal, TextColor);

            _toastStyle = new GUIStyle(GUI.skin.box);
            _toastStyle.font = _uiFont;
            _toastStyle.fontSize = 12;
            _toastStyle.alignment = TextAnchor.MiddleCenter;
            _toastStyle.richText = true;
            _toastStyle.wordWrap = true;
            _toastStyle.normal.background = _toastTexture;
            _toastStyle.normal.textColor = TextColor;
            _toastStyle.border = new RectOffset(12, 12, 12, 12);
            _toastStyle.padding = new RectOffset(14, 14, 6, 6);
        }

        private static GUIStyle NewLabelStyle(int size, FontStyle fontStyle, Color color)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.font = _uiFont;
            style.fontSize = size;
            style.fontStyle = fontStyle;
            style.richText = true;
            style.wordWrap = false;
            style.normal.textColor = color;
            return style;
        }

        private static GUIStyle NewButtonStyle(
            Texture2D normal,
            Texture2D hover,
            Texture2D active,
            Color textColor)
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.font = _uiFont;
            style.fontSize = 12;
            style.fixedHeight = 34f;
            style.padding = new RectOffset(12, 12, 5, 5);
            style.margin = new RectOffset(0, 0, 0, 0);
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.background = normal;
            style.hover.background = hover;
            style.active.background = active;
            style.focused.background = normal;
            style.onNormal.background = active;
            style.normal.textColor = textColor;
            style.hover.textColor = textColor;
            style.active.textColor = textColor;
            style.focused.textColor = textColor;
            return style;
        }

        private static GUIStyle NewSmallButtonStyle(Texture2D normal, Texture2D hover, Color textColor)
        {
            GUIStyle style = NewButtonStyle(normal, hover, normal, textColor);
            style.fontSize = 11;
            style.fixedHeight = 24f;
            style.padding = new RectOffset(8, 8, 3, 3);
            style.border = new RectOffset(0, 0, 0, 0);
            return style;
        }

        private static GUIStyle NewChipStyle(Texture2D background, Color textColor)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.font = _uiFont;
            style.fontSize = 9;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.background = background;
            style.normal.textColor = textColor;
            style.border = new RectOffset(8, 8, 8, 8);
            style.padding = new RectOffset(6, 6, 3, 3);
            style.fixedHeight = 26f;
            return style;
        }

        private static Font LoadFont(string[] names, int size)
        {
            try
            {
                return Font.CreateDynamicFontFromOSFont(names, size);
            }
            catch
            {
                return null;
            }
        }

        private static float GetGuiScale()
        {
            float dpi = Screen.dpi;
            if (dpi <= 0f)
            {
                return 1f;
            }

            return Mathf.Clamp(dpi / 96f, 1f, 2f);
        }

        private static void EnsureSpawnEntries()
        {
            if (_spawnEntries != null || !string.IsNullOrEmpty(_spawnError))
            {
                return;
            }

            try
            {
                Item[] all = Resources.LoadAll<Item>("Items");
                List<SpawnEntry> entries = new List<SpawnEntry>();
                HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (Item prefab in all)
                {
                    if (prefab == null)
                    {
                        continue;
                    }

                    string name = SafeName(prefab);
                    string key = prefab.name + "|" + name;
                    if (!seen.Add(key))
                    {
                        continue;
                    }

                    entries.Add(new SpawnEntry
                    {
                        Prefab = prefab,
                        Name = name,
                        AssetName = prefab.name,
                        IsCreature = prefab.Creature != null,
                        TypeLabel = prefab.Creature != null
                            ? (prefab.Creature.BossType != BossType.None ? "Boss" : "Creature")
                            : prefab.Type.ToString()
                    });
                }

                entries.Sort((a, b) =>
                {
                    int typeCompare = b.IsCreature.CompareTo(a.IsCreature);
                    if (typeCompare != 0)
                    {
                        return typeCompare;
                    }

                    return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
                });

                _spawnEntries = entries;
            }
            catch (Exception ex)
            {
                _spawnError = ex.Message;
                Debug.LogError("[Angler's Toolkit] Could not build spawn list: " + ex);
            }
        }

        private static bool MatchesSearch(SpawnEntry entry)
        {
            if (string.IsNullOrWhiteSpace(_spawnSearch))
            {
                return true;
            }

            string search = _spawnSearch.Trim();
            return Contains(entry.Name, search) ||
                   Contains(entry.AssetName, search) ||
                   Contains(entry.TypeLabel, search);
        }

        private static bool Contains(string haystack, string needle)
        {
            return !string.IsNullOrEmpty(haystack) &&
                   haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string SafeName(Item item)
        {
            try
            {
                string localized = item.GetName();
                if (!string.IsNullOrWhiteSpace(localized))
                {
                    return localized;
                }
            }
            catch
            {
            }

            return item.name;
        }

        private static Texture2D MakeRoundedTexture(
            int size,
            int radius,
            int borderThickness,
            Color fill,
            Color border)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[size * size];
            float half = size * 0.5f;
            float box = half - radius;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;
                    float qx = Mathf.Abs(px - half) - box;
                    float qy = Mathf.Abs(py - half) - box;
                    float outside = Mathf.Sqrt(
                        Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) +
                        Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f));
                    float inside = Mathf.Min(Mathf.Max(qx, qy), 0f);
                    float distance = outside + inside - radius;
                    float alpha = Mathf.Clamp01(0.5f - distance);

                    if (alpha <= 0f)
                    {
                        pixels[y * size + x] = new Color(0f, 0f, 0f, 0f);
                        continue;
                    }

                    float borderAmount = Mathf.Clamp01(distance + borderThickness + 0.5f);
                    Color pixel = Color.Lerp(fill, border, borderAmount);
                    pixel.a *= alpha;
                    pixels[y * size + x] = pixel;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;
            return texture;
        }

        private static Texture2D MakeSwitchTexture(bool on)
        {
            const int width = 92;
            const int height = 48;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[width * height];

            Color track = on
                ? new Color(0.12f, 0.68f, 0.58f, 1f)
                : new Color(0.16f, 0.20f, 0.25f, 1f);
            Color trackBorder = on
                ? new Color(0.30f, 0.90f, 0.76f, 1f)
                : new Color(0.25f, 0.31f, 0.38f, 1f);
            Color knob = new Color(0.92f, 0.97f, 1f, 1f);
            float knobCenterX = on ? 68f : 24f;
            float knobCenterY = 24f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;

                    float qx = Mathf.Abs(px - 46f) - 44f;
                    float qy = Mathf.Abs(py - 24f) - 21f;
                    float outside = Mathf.Sqrt(
                        Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) +
                        Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f));
                    float inside = Mathf.Min(Mathf.Max(qx, qy), 0f);
                    float distance = outside + inside - 21f;
                    float alpha = Mathf.Clamp01(0.5f - distance);

                    if (alpha <= 0f)
                    {
                        pixels[y * width + x] = new Color(0f, 0f, 0f, 0f);
                        continue;
                    }

                    float borderAmount = Mathf.Clamp01(distance + 2.5f);
                    Color color = Color.Lerp(track, trackBorder, borderAmount);

                    float knobDistance = Mathf.Sqrt(
                        (px - knobCenterX) * (px - knobCenterX) +
                        (py - knobCenterY) * (py - knobCenterY)) - 17f;
                    float knobAlpha = Mathf.Clamp01(0.5f - knobDistance);

                    if (knobAlpha > 0f)
                    {
                        color = Color.Lerp(color, knob, knobAlpha);
                    }

                    color.a *= alpha;
                    pixels[y * width + x] = color;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;
            return texture;
        }

        private static Texture2D MakeSliderTrackTexture()
        {
            const int width = 64;
            const int height = 18;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;

                    float qx = Mathf.Abs(px - 32f) - 30f;
                    float qy = Mathf.Abs(py - 9f) - 2.5f;
                    float outside = Mathf.Sqrt(
                        Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) +
                        Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f));
                    float inside = Mathf.Min(Mathf.Max(qx, qy), 0f);
                    float distance = outside + inside - 2.5f;
                    float alpha = Mathf.Clamp01(0.5f - distance);

                    if (alpha <= 0f)
                    {
                        pixels[y * width + x] = new Color(0f, 0f, 0f, 0f);
                        continue;
                    }

                    Color color = new Color(0.16f, 0.21f, 0.27f, 1f);
                    color.a *= alpha;
                    pixels[y * width + x] = color;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;
            return texture;
        }

        private static Texture2D MakeCircleTexture(int size, Color color)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[size * size];
            float center = size * 0.5f;
            float radius = center - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Mathf.Sqrt(
                        (x + 0.5f - center) * (x + 0.5f - center) +
                        (y + 0.5f - center) * (y + 0.5f - center)) - radius;
                    float alpha = Mathf.Clamp01(0.5f - distance);
                    Color pixel = color;
                    pixel.a *= alpha;
                    pixels[y * size + x] = pixel;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;
            return texture;
        }

        private static Texture2D MakeTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;
            return texture;
        }
    }
}
