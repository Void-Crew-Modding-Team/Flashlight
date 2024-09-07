using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using VoidManager.CustomGUI;
using VoidManager.Utilities;

namespace Flashlight
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.USERS_PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Void Crew.exe")]
    [BepInDependency(VoidManager.MyPluginInfo.PLUGIN_GUID)]
    public class BepinPlugin : BaseUnityPlugin
    {
        internal static BepinPlugin instance;
        internal static ManualLogSource Log;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "N/A")]
        private void Awake()
        {
            instance = this;
            Log = Logger;
            Configs.Load();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
    public class Configs : ModSettingsMenu
    {
        public override string Name() => $"{MyPluginInfo.PLUGIN_NAME} Config";
        internal static Color DefaultColor = Color.white;

        internal static void Load()
        {
            MatchFlashlights = BepinPlugin.instance.Config.Bind("PlayerFlashlightColour", "Match", true); matchFlashlights = MatchFlashlights.Value;
            PlayerFlashlightColor = LoadColor("PlayerFlashlightColour");
            OthersFlashlightColor = LoadColor("OthersFlashlightColour");
            PlayerFlashlightRainbow = BepinPlugin.instance.Config.Bind("RainbowFlashlight", "Player", true); playerFlashlightRainbow = PlayerFlashlightRainbow.Value;
            OthersFlashlightRainbow = BepinPlugin.instance.Config.Bind("RainbowFlashlight", "Others", true); othersFlashlightRainbow = OthersFlashlightRainbow.Value;
            RainbowSpeed = BepinPlugin.instance.Config.Bind("RainbowFlashlight", "Speed", 1f); rainbowSpeed = RainbowSpeed.Value;
        }

        private static Color LoadColor(string colorPrefix)
        {
            var colorR = BepinPlugin.instance.Config.Bind(colorPrefix, "R", 1f);
            var colorG = BepinPlugin.instance.Config.Bind(colorPrefix, "G", 1f);
            var colorB = BepinPlugin.instance.Config.Bind(colorPrefix, "B", 1f);
            return new Color(colorR.Value, colorG.Value, colorB.Value);
        }

        public override void Draw()
        {
            if (GUITools.DrawColorPicker(new Rect(8, 50, 240, 160), "Player Flashlight Colour", ref Configs.PlayerFlashlightColor, Configs.DefaultColor, false, 0f, 5f))
            {
                UpdateFlashlightColor("PlayerFlashlightColour", PlayerFlashlightColor);
            }
            if (GUITools.DrawColorPicker(new Rect(256, 50, 240, 160), "Others Flashlight Colour", ref Configs.OthersFlashlightColor, Configs.DefaultColor, false, 0f, 5f))
            {
                UpdateFlashlightColor("OthersFlashlightColour", OthersFlashlightColor);
            }
            GUILayout.Space(170);
            if (GUITools.DrawCheckbox("Override Flashlight colour with Others colours", ref matchFlashlights))
            {
                MatchFlashlights.Value = matchFlashlights;
            }
            GUILayout.BeginHorizontal();
            if (GUITools.DrawCheckbox("Rainbow Player Flashlight", ref playerFlashlightRainbow))
            {
                PlayerFlashlightRainbow.Value = playerFlashlightRainbow;
            }
            if (GUITools.DrawCheckbox("Rainbow Others Flashlights", ref othersFlashlightRainbow))
            {
                OthersFlashlightRainbow.Value = othersFlashlightRainbow;
            }
            GUILayout.EndHorizontal();
            if (GUITools.DrawSlider(ref rainbowSpeed, 0f, 10f))
            {
                RainbowSpeed.Value = rainbowSpeed;
            }
        }

        internal static void UpdateFlashlightColor(string colorPrefix, Color color)
        {
            BepinPlugin.instance.Config.Bind(colorPrefix, "R", 1f).Value = color.r;
            BepinPlugin.instance.Config.Bind(colorPrefix, "G", 1f).Value = color.g;
            BepinPlugin.instance.Config.Bind(colorPrefix, "B", 1f).Value = color.b;
        }

        internal static ConfigEntry<bool> MatchFlashlights; internal static bool matchFlashlights;
        internal static Color PlayerFlashlightColor; internal static Color OthersFlashlightColor;
        internal static ConfigEntry<bool> PlayerFlashlightRainbow; internal static bool playerFlashlightRainbow;
        internal static ConfigEntry<bool> OthersFlashlightRainbow; internal static bool othersFlashlightRainbow;
        internal static ConfigEntry<float> RainbowSpeed; internal static float rainbowSpeed;
    }
}