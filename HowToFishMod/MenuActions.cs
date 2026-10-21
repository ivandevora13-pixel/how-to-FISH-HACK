using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnglersToolkit
{
    internal static class MenuActions
    {
        internal static void AddMoney(int amount, Currency currency)
        {
            if (!RequireHost() || Player.LocalPlayer == null)
            {
                return;
            }

            MoneyManager.AddMoney(amount, Player.LocalPlayer, currency);
            Plugin.Toast((currency == Currency.Euro ? "Added " : "Added $") + amount.ToString("N0"));
        }

        internal static void UnlockAllIslands()
        {
            if (!RequireHost())
            {
                return;
            }

            if (OnlineIslandManager.Instance == null)
            {
                Plugin.Toast("Load into a world first.");
                return;
            }

            OnlineIslandManager.Instance.UnlockIsland(byte.MaxValue);
            Plugin.Toast("All islands unlocked for this session.");
        }

        internal static void UnlockBoat()
        {
            if (!RequireHost())
            {
                return;
            }

            Boat boat = BoatManager.Boat;
            if (boat == null)
            {
                Plugin.Toast("Boat is not spawned yet.");
                return;
            }

            boat.UnlockBoat();
            boat.UnlockBoatRadar();
            boat.SetMotor(2);
            Plugin.Toast("Boat, radar, and motor unlocked.");
        }

        internal static void UnlockGrill()
        {
            if (!RequireHost())
            {
                return;
            }

            NPCManager.UnlockGrill();
            Plugin.Toast("Grill unlocked.");
        }

        internal static void UnlockAllSkins()
        {
            try
            {
                SaveManager.LockAllSkins();

                Item[] items = GameInfo.ItemWithSkinsforCommands;
                if (items != null)
                {
                    foreach (Item item in items)
                    {
                        if (item == null || item.SkinPreset == null || item.SkinPreset.Skins == null)
                        {
                            continue;
                        }

                        for (int i = 0; i < item.SkinPreset.Skins.Count; i++)
                        {
                            SaveManager.UnlockSkin(item.ID, (byte)i);
                        }
                    }
                }

                Boat boat = BoatManager.Boat;
                if (boat != null && boat.SkinPreset != null && boat.SkinPreset.Skins != null)
                {
                    for (int i = 0; i < boat.SkinPreset.Skins.Count; i++)
                    {
                        SaveManager.UnlockSkin(byte.MaxValue, (byte)i);
                    }
                }

                Plugin.Toast("All skins unlocked.");
            }
            catch (Exception ex)
            {
                Plugin.Toast("Could not unlock skins. Load into a save first.");
                Debug.LogWarning("[Angler's Toolkit] Unlock skins error: " + ex);
            }
        }

        internal static void UnlockAchievements()
        {
            try
            {
                AchievementManager.ToggleAllAchievements(true);
                Plugin.Toast("All achievements unlocked.");
            }
            catch (Exception ex)
            {
                Plugin.Toast("Achievements are not available right now.");
                Debug.LogWarning("[Angler's Toolkit] Unlock achievements error: " + ex);
            }
        }

        internal static void CompleteFishJournal()
        {
            try
            {
                GameInfo.ToggleAllCreaturesKilled(true, false);
                Plugin.Toast("Journal marked complete.");
            }
            catch (Exception ex)
            {
                Plugin.Toast("Could not update the journal.");
                Debug.LogWarning("[Angler's Toolkit] Journal error: " + ex);
            }
        }

        internal static void KillBoss()
        {
            if (!RequireHost())
            {
                return;
            }

            if (BossManager.Boss == null || Player.LocalPlayer == null)
            {
                Plugin.Toast("No boss is active.");
                return;
            }

            BossManager.Boss.LocalHit(
                BossManager.Boss.transform,
                BossManager.Boss.transform.position,
                Vector3.up,
                Player.LocalPlayer,
                999999,
                false,
                Vector3.zero);

            Plugin.Toast("Boss hit for 999,999 damage.");
        }

        internal static void ToggleGodMode()
        {
            if (!RequireHost())
            {
                return;
            }

            PlayerManager.ToggleGodMode();
            Plugin.Toast("God mode: " + (PlayerManager.InGodMode ? "ON" : "OFF"));
        }

        internal static void ToggleOneShot()
        {
            if (!RequireHost())
            {
                return;
            }

            if (ServerSettings.Instance == null)
            {
                Plugin.Toast("Server settings are not available yet.");
                return;
            }

            ServerSettings.Instance.ToggleOneShot();
            Plugin.Toast("One shot: " + (ServerSettings.OneShotEnabled ? "ON" : "OFF"));
        }

        internal static void HealSelf()
        {
            if (!RequireHost())
            {
                return;
            }

            if (Player.LocalPlayer == null || Player.LocalPlayer.Vitals == null)
            {
                return;
            }

            Player.LocalPlayer.Vitals.Heal(9999);
            Player.LocalPlayer.Vitals.RestoreFullness(9999);
            Plugin.Toast("Health and fullness restored.");
        }

        internal static void TeleportIsland(bool previous)
        {
            if (!RequireHost())
            {
                return;
            }

            try
            {
                OnlineIslandManager.TpToNextIsland(previous);
                Plugin.Toast(previous ? "Traveling to the previous island..." : "Traveling to the next island...");
            }
            catch (Exception ex)
            {
                Plugin.Toast("Could not teleport right now.");
                Debug.LogWarning("[Angler's Toolkit] Island teleport error: " + ex);
            }
        }

        internal static void SpawnItem(Item prefab, bool asDead)
        {
            if (!RequireHost())
            {
                return;
            }

            if (prefab == null || Player.LocalPlayer == null)
            {
                return;
            }

            try
            {
                Camera cam = GameInfo.CurCamera;
                Transform spawnTransform = cam != null ? cam.transform : Player.LocalPlayer.CamObject;
                if (spawnTransform == null)
                {
                    Plugin.Toast("Could not find the player camera.");
                    return;
                }

                Vector3 position = spawnTransform.position + spawnTransform.forward * 2.2f;
                Item item = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);

                if (asDead && item.Creature != null)
                {
                    item.Creature.ServerKillOnSpawn();
                }

                Server.Instance.Spawn(item.gameObject);

                string name = item.GetName();
                Plugin.Toast("Spawned " + (string.IsNullOrEmpty(name) ? prefab.name : name));
            }
            catch (Exception ex)
            {
                Plugin.Toast("Spawn failed.");
                Debug.LogWarning("[Angler's Toolkit] Spawn error: " + ex);
            }
        }

        private static bool RequireHost()
        {
            if (!Plugin.IsHost)
            {
                Plugin.Toast("Start a solo game or host a lobby first.");
                return false;
            }

            return true;
        }
    }
}
