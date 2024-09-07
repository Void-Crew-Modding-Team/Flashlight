using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using CG.Game.Player;
using Photon.Pun;

namespace Flashlight
{
    [HarmonyPatch(typeof(CustomPunCharacterTransformMonitor), "Update")]
    internal class Patch
    {
        internal static Dictionary<GameObject, Light> playerFlashlights = new Dictionary<GameObject, Light> { };
        static void Postfix(CustomPunCharacterTransformMonitor __instance)
        {
            GameObject player = __instance.gameObject;
            if (player == null) return;
            if (playerFlashlights.TryGetValue(player, out Light flashlight))
            {
                if (player.GetPhotonView().IsMine && Configs.SeperateFlashlights.Value)
                {
                    flashlight.color = Configs.PlayerFlashlightColor;
                    if (Configs.PlayerFlashlightRainbow.Value) flashlight.color = GetRainbowColor();
                    flashlight.spotAngle = Configs.PlayerFlashlightAngle.Value;
                    flashlight.range = Configs.PlayerFlashlightRange.Value;
                    flashlight.intensity = Configs.PlayerFlashlightIntensity.Value;
                    if (Configs.PlayerFlashlightAOE.Value) flashlight.type = LightType.Point;
                    else flashlight.type = LightType.Spot;
                    return;
                }
                flashlight.color = Configs.OthersFlashlightColor;
                if (Configs.OthersFlashlightRainbow.Value) flashlight.color = GetRainbowColor();
                flashlight.spotAngle = Configs.OthersFlashlightAngle.Value;
                flashlight.range = Configs.OthersFlashlightRange.Value;
                flashlight.intensity = Configs.OthersFlashlightIntensity.Value;
                if (Configs.OthersFlashlightAOE.Value) flashlight.type = LightType.Point;
                else flashlight.type = LightType.Spot;
            }
            else
            {
                Light Flashlight = player.GetComponentInChildren<Light>();
                if (Flashlight != null)
                {
                    playerFlashlights.Add(player, Flashlight);
                }
            }
        }
        private static Color GetRainbowColor()
        {
            float hue = (Time.time * Configs.RainbowSpeed.Value) % 1f;
            return Color.HSVToRGB(hue, 1f, 1f);
        }
    }
}
