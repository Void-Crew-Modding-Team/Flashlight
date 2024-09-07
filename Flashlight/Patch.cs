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
                if (__instance is LocalPlayer && !Configs.MatchFlashlights.Value)
                {
                    flashlight.color = Configs.PlayerFlashlightColor;
                    if (Configs.PlayerFlashlightRainbow.Value) flashlight.color = GetRainbowColor();
                    return;
                }
                flashlight.color = Configs.OthersFlashlightColor;
                if (Configs.OthersFlashlightRainbow.Value) flashlight.color = GetRainbowColor();
            }
            else
            {
                Light Flashlight = __instance.GameObject.GetComponentInChildren<Light>();
                if (Flashlight != null) playerFlashlights.Add(__instance, Flashlight);
            }
        }
        private static Color GetRainbowColor()
        {
            float hue = (Time.time * Configs.RainbowSpeed.Value) % 1f;
            return Color.HSVToRGB(hue, 1f, 1f);
        }
    }
}
