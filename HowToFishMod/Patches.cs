using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace AnglersToolkit
{
    /// <summary>
    /// Gameplay patches. Every server-side feature checks <see cref="Plugin.IsHost"/> so a
    /// client cannot accidentally try to run host-only code.
    /// </summary>
    internal static class Patches
    {
        [HarmonyPatch(typeof(Player), "get_BlockInputs")]
        internal static class PlayerBlockInputsPatch
        {
            private static void Postfix(ref bool __result)
            {
                if (Plugin.MenuOpen)
                {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerCamera), "OnApplicationFocus")]
        internal static class PlayerCameraFocusPatch
        {
            private static bool Prefix()
            {
                return !Plugin.MenuOpen;
            }
        }

        [HarmonyPatch(typeof(PauseManager), "PauseInput")]
        internal static class PauseInputPatch
        {
            private static bool Prefix()
            {
                return !Plugin.MenuOpen;
            }
        }

        [HarmonyPatch(typeof(Bait), "get_RandomizedCatchTime")]
        internal static class BaitCatchTimePatch
        {
            private static void Postfix(ref float __result)
            {
                if (Plugin.InstantBite != null && Plugin.InstantBite.Value)
                {
                    __result = 0f;
                }
            }
        }

        [HarmonyPatch(typeof(Creature), "OnStartServer")]
        internal static class CreatureStartServerPatch
        {
            private static readonly HashSet<int> WeightBoosted = new HashSet<int>();

            private static void Postfix(Creature __instance)
            {
                if (!Plugin.IsHost)
                {
                    return;
                }

                try
                {
                    if (Plugin.AlwaysShiny != null &&
                        Plugin.AlwaysShiny.Value &&
                        __instance.BossType == BossType.None)
                    {
                        __instance._isDrip.Value = true;
                    }

                    if (Plugin.WeightMultiplier != null && Plugin.WeightMultiplier.Value > 1.001f)
                    {
                        int id = __instance.GetInstanceID();
                        if (WeightBoosted.Add(id))
                        {
                            float weight = __instance._syncedRandomWeight.Value;
                            __instance._syncedRandomWeight.Value =
                                Mathf.Clamp(weight * Plugin.WeightMultiplier.Value, 0.01f, 5000f);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[Angler's Toolkit] Creature start patch: " + ex.Message);
                }
            }
        }

        [HarmonyPatch(typeof(Item), "SetAttachedRod")]
        internal static class ItemSetAttachedRodPatch
        {
            private static void Prefix(Item __instance, FishingRod rod)
            {
                if (rod == null || !Plugin.IsHost)
                {
                    return;
                }

                if (Plugin.AutoKill != null && Plugin.AutoKill.Value && __instance.Creature != null)
                {
                    // This runs before FishNet spawns the object, which is exactly what
                    // the game's own /spawndead command does.
                    __instance.Creature.ServerKillOnSpawn();
                }
            }
        }

        [HarmonyPatch(typeof(Item), "get_TotalWorth")]
        internal static class ItemTotalWorthPatch
        {
            private static void Postfix(ref int __result)
            {
                if (!Plugin.IsHost || Plugin.SellMultiplier == null)
                {
                    return;
                }

                float multiplier = Plugin.SellMultiplier.Value;
                if (multiplier <= 1.001f)
                {
                    return;
                }

                long value = (long)Math.Round(__result * (double)multiplier);
                __result = (int)Math.Min(value, int.MaxValue);
            }
        }

        [HarmonyPatch(typeof(FishingRod), "Update")]
        internal static class FishingRodUpdatePatch
        {
            private static readonly FieldInfo ReelingInField =
                AccessTools.Field(typeof(FishingRod), "_isReelingIn");

            private static readonly FieldInfo ReelingOutField =
                AccessTools.Field(typeof(FishingRod), "_isReelingOut");

            private static readonly MethodInfo StartReelMethod =
                AccessTools.Method(typeof(FishingRodCast), "StartReel");

            private static bool _loggedReelError;
            private static bool _loggedCollectError;

            private static void Postfix(FishingRod __instance)
            {
                try
                {
                    Player holder = __instance.Holder;
                    if (holder == null || holder.Owner == null || !holder.Owner.IsLocalClient)
                    {
                        return;
                    }

                    if (holder.Holding == null || holder.Holding.HeldItem != __instance)
                    {
                        return;
                    }

                    Bait bait = __instance.Bait;
                    if (bait == null)
                    {
                        return;
                    }

                    Item hooked = bait.ItemOnBait;
                    if (hooked == null)
                    {
                        return;
                    }

                    if (Plugin.AutoReel != null && Plugin.AutoReel.Value)
                    {
                        bool alreadyReeling = ReelingInField != null &&
                                              (bool)ReelingInField.GetValue(__instance);

                        if (!alreadyReeling &&
                            StartReelMethod != null &&
                            __instance is FishingRodCast)
                        {
                            try
                            {
                                StartReelMethod.Invoke(__instance, null);
                            }
                            catch (Exception ex)
                            {
                                if (!_loggedReelError)
                                {
                                    _loggedReelError = true;
                                    Debug.LogWarning("[Angler's Toolkit] Auto reel could not start: " + ex.Message);
                                }
                            }
                        }

                        if (ReelingInField != null)
                        {
                            ReelingInField.SetValue(__instance, true);
                        }

                        if (ReelingOutField != null)
                        {
                            ReelingOutField.SetValue(__instance, false);
                        }
                    }

                    if (Plugin.AutoCollect != null &&
                        Plugin.AutoCollect.Value &&
                        Plugin.IsHost &&
                        hooked.CanPickUp)
                    {
                        Transform playerTransform = holder.Transform;
                        if (playerTransform != null &&
                            Vector3.Distance(hooked.transform.position, playerTransform.position) <= 2.5f)
                        {
                            hooked.PickUp(holder, calledFromLocal: true, sendToServer: true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!_loggedCollectError)
                    {
                        _loggedCollectError = true;
                        Debug.LogWarning("[Angler's Toolkit] Auto collect error: " + ex);
                    }
                }
            }
        }
    }
}
