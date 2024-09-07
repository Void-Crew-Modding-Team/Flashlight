using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using CG.Game.Player;

namespace Flashlight
{
    [HarmonyPatch(typeof(Player), "Update")]
    internal class Patch
    {
        internal static Dictionary<Player, Light> playerFlashlights = new Dictionary<Player, Light> { };
        static void Postfix(Player __instance)
        {
            if (playerFlashlights.TryGetValue(__instance, out Light flashlight))
            {
                if (__instance is LocalPlayer)
                {
                    flashlight.color = Configs.PlayerFlashlightColor;
                    return;
                }
                flashlight.color = Configs.OthersFlashlightColor;
            }
            else
            {
                Light Flashlight = __instance.GameObject.GetComponentInChildren<Light>();
                if (Flashlight != null) playerFlashlights.Add(__instance, Flashlight);
            }
        }
    }
}
