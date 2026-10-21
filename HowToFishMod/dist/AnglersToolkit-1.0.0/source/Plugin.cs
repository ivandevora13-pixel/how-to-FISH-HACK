using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace AnglersToolkit
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "dazed.howtofish.anglerstoolkit";
        public const string PluginName = "Angler's Toolkit";
        public const string PluginVersion = "1.0.0";

        internal static Plugin Instance { get; private set; }

        internal static bool MenuOpen { get; private set; }

        internal static bool IsHost
        {
            get
            {
                try
                {
                    return Server.Instance != null && Server.Instance.IsServerInitialized;
                }
                catch
                {
                    return false;
                }
            }
        }

        internal static ConfigEntry<KeyCode> MenuKey;
        internal static ConfigEntry<bool> EnableDevCheats;
        internal static ConfigEntry<bool> AutoReel;
        internal static ConfigEntry<bool> InstantBite;
        internal static ConfigEntry<bool> AlwaysShiny;
        internal static ConfigEntry<bool> AutoKill;
        internal static ConfigEntry<bool> AutoCollect;
        internal static ConfigEntry<bool> ShowCatchHud;
        internal static ConfigEntry<float> WeightMultiplier;
        internal static ConfigEntry<float> SellMultiplier;

        internal static string ToastText;
        internal static float ToastUntil;

        private Harmony _harmony;
        private float _cheatRefreshTimer;
        private float _lastGuiErrorTime;

        private void Awake()
        {
            Instance = this;

            MenuKey = Config.Bind(
                "General",
                "Menu Hotkey",
                KeyCode.F8,
                "Keyboard key that opens and closes the Angler's Toolkit menu.");

            EnableDevCheats = Config.Bind(
                "General",
                "Enable Developer Cheats",
                true,
                "Keeps How to Fish's built-in developer cheats enabled (M/N money keys, O island teleport, cheat commands, and more).");

            AutoReel = Config.Bind(
                "Fishing",
                "Auto Reel",
                true,
                "Automatically reels in anything that bites your line.");

            InstantBite = Config.Bind(
                "Fishing",
                "Instant Bite",
                false,
                "Makes fish bite as soon as the bait is underwater. Host/server only.");

            AlwaysShiny = Config.Bind(
                "Fishing",
                "Always Shiny",
                false,
                "Forces newly spawned creatures to be the rare shiny/drip variant. Host/server only.");

            AutoKill = Config.Bind(
                "Fishing",
                "Auto Kill Catches",
                false,
                "Kills creatures as soon as they are hooked so you can pick them up immediately. Host/server only.");

            AutoCollect = Config.Bind(
                "Fishing",
                "Auto Collect",
                false,
                "Automatically picks up a catch once it is close enough to you. Requires Auto Kill Catches for living creatures. Host/server only.");

            ShowCatchHud = Config.Bind(
                "Fishing",
                "Catch HUD",
                true,
                "Shows a small panel with the name, weight, and value of whatever is currently on your line.");

            WeightMultiplier = Config.Bind(
                "Fishing",
                "Weight Multiplier",
                1f,
                new ConfigDescription(
                    "Multiplies the randomized weight of newly spawned creatures. 1 = normal. Host/server only.",
                    new AcceptableValueRange<float>(1f, 50f)));

            SellMultiplier = Config.Bind(
                "Fishing",
                "Sell Value Multiplier",
                1f,
                new ConfigDescription(
                    "Multiplies the sale value of items. 1 = normal. Host/server only.",
                    new AcceptableValueRange<float>(1f, 50f)));

            _harmony = new Harmony(PluginGuid);
            try
            {
                _harmony.PatchAll();
                Logger.LogInfo("Harmony patches applied.");
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to apply one or more Harmony patches:\n" + ex);
            }
        }

        private void Start()
        {
            TryEnableDevCheats();
            Logger.LogInfo(PluginName + " " + PluginVersion + " loaded. Press " + MenuKey.Value + " for the menu.");
        }

        private void Update()
        {
            if (Input.GetKeyDown(MenuKey.Value))
            {
                ToggleMenu();
            }

            if (MenuOpen)
            {
                // Keep the cursor free while the menu is open. PlayerCamera normally
                // re-locks it when the game window regains focus.
                try
                {
                    PlayerCamera.ToggleMouse(true);
                }
                catch
                {
                    // Player/UI not ready yet.
                }
            }

            _cheatRefreshTimer += Time.unscaledDeltaTime;
            if (_cheatRefreshTimer >= 5f)
            {
                _cheatRefreshTimer = 0f;
                TryEnableDevCheats();
            }
        }

        internal static void ToggleMenu()
        {
            MenuOpen = !MenuOpen;

            if (MenuOpen)
            {
                MenuGUI.OnOpened();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                if (Player.LocalPlayer != null)
                {
                    try
                    {
                        PlayerCamera.ToggleMouse(true);
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                if (Player.LocalPlayer != null &&
                    !PauseManager.IsPaused &&
                    !MainMenuManager.IsInMenu)
                {
                    try
                    {
                        PlayerCamera.ToggleMouse(false);
                    }
                    catch
                    {
                    }
                }
            }
        }

        internal static void Toast(string message)
        {
            ToastText = message;
            ToastUntil = Time.unscaledTime + 4f;
        }

        private static void TryEnableDevCheats()
        {
            if (EnableDevCheats == null || !EnableDevCheats.Value)
            {
                return;
            }

            try
            {
                ClientSettings.ToggleCheats(true);
            }
            catch (Exception ex)
            {
                if (Instance != null)
                {
                    Instance.Logger.LogDebug("Could not enable developer cheats yet: " + ex.Message);
                }
            }
        }

        private void OnGUI()
        {
            try
            {
                MenuGUI.Draw();
            }
            catch (Exception ex)
            {
                // OnGUI is called many times per frame; do not flood the log.
                if (Time.unscaledTime - _lastGuiErrorTime > 10f)
                {
                    _lastGuiErrorTime = Time.unscaledTime;
                    Logger.LogError("Menu GUI error:\n" + ex);
                }
            }
        }

        private void OnDestroy()
        {
            if (_harmony != null)
            {
                _harmony.UnpatchSelf();
            }
        }
    }
}
