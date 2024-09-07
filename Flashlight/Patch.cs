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
                    flashlight.color = Configs.playerFlashlight.Colour;
                    if (Configs.playerFlashlight.Rainbow.Value) flashlight.color = GetRainbowColor();
                    flashlight.spotAngle = Configs.playerFlashlight.Angle.Value;
                    flashlight.range = Configs.playerFlashlight.Range.Value;
                    flashlight.intensity = Configs.playerFlashlight.Intensity.Value;
                    if (Configs.playerFlashlight.AOE.Value) flashlight.type = LightType.Point;
                    else flashlight.type = LightType.Spot;
                    return;
                }
                flashlight.color = Configs.othersFlashlight.Colour;
                if (Configs.othersFlashlight.Rainbow.Value) flashlight.color = GetRainbowColor();
                flashlight.spotAngle = Configs.othersFlashlight.Angle.Value;
                flashlight.range = Configs.othersFlashlight.Range.Value;
                flashlight.intensity = Configs.othersFlashlight.Intensity.Value;
                if (Configs.othersFlashlight.AOE.Value) flashlight.type = LightType.Point;
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
