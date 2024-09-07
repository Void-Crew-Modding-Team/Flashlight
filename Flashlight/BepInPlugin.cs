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
        
        private static Color DefaultColor = Color.white;
        private static float DefaultAngle = 45.1f;
        private static float DefaultRange = 25;
        private static float DefaultIntensity = 416.34f;
        internal static void Load()
        {
            SeperateFlashlights = BepinPlugin.instance.Config.Bind("PlayerFlashlight", "Enabled", false);
            PrecisionMode = BepinPlugin.instance.Config.Bind("PrecisionMode", "Enabled", true);
            PlayerFlashlightColor = LoadColor("PlayerFlashlight");
            OthersFlashlightColor = LoadColor("OthersFlashlight");
            PlayerFlashlightRainbow = BepinPlugin.instance.Config.Bind("PlayerFlashlight", "Rainbow", false);
            OthersFlashlightRainbow = BepinPlugin.instance.Config.Bind("OthersFlashlight", "Rainbow", false);
            RainbowSpeed = BepinPlugin.instance.Config.Bind("RainbowFlashlight", "Speed", 0.125f);
            PlayerFlashlightAOE = BepinPlugin.instance.Config.Bind("PlayerFlashlight", "AOE", false);
            OthersFlashlightAOE = BepinPlugin.instance.Config.Bind("OthersFlashlight", "AOE", false);
            PlayerFlashlightAngle = BepinPlugin.instance.Config.Bind("PlayerFlashlight", "Angle", DefaultAngle);
            OthersFlashlightAngle = BepinPlugin.instance.Config.Bind("OthersFlashlight", "Angle", DefaultAngle);
            PlayerFlashlightRange = BepinPlugin.instance.Config.Bind("PlayerFlashlight", "Range", DefaultRange);
            OthersFlashlightRange = BepinPlugin.instance.Config.Bind("OthersFlashlight", "Range", DefaultRange);
            PlayerFlashlightIntensity = BepinPlugin.instance.Config.Bind("PlayerFlashlight", "Intensity", DefaultIntensity);
            OthersFlashlightIntensity = BepinPlugin.instance.Config.Bind("OthersFlashlight", "Intensity", DefaultIntensity);
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

            GUITools.DrawCheckbox("Seperate Flashlight Options For Local Player", ref SeperateFlashlights);
            DrawLabeledSlider("Rainbow Flashlight Speed", ref RainbowSpeed, 0f, 0.4f, 0.125f);
            GUITools.DrawCheckbox("Precision Range and Intensity", ref PrecisionMode);

            GUILayout.BeginArea(new Rect(0, 130, 450, 315), "", "Box");
            GUILayout.Label("Local Flashlight");
            if (GUITools.DrawColorPicker(new Rect(4, 30, 442, 160), "Colour", ref Configs.PlayerFlashlightColor, Configs.DefaultColor, false, 0f, 1f))
            {
                UpdateFlashlightColor("PlayerFlashlight", PlayerFlashlightColor);
            }
            GUILayout.Space(160);
            GUILayout.BeginVertical("Box");
            DrawLabeledSlider("Angle", ref PlayerFlashlightAngle, 15f, 160, DefaultAngle);
            DrawLabeledSlider("Range", ref PlayerFlashlightRange, 0, (PrecisionMode.Value? 25 : 100), DefaultRange);
            DrawLabeledSlider("Intensity", ref PlayerFlashlightIntensity, 0, (PrecisionMode.Value ? 1000 : 10000), DefaultIntensity);
            GUITools.DrawCheckbox("Area Of Effect Flashlight", ref PlayerFlashlightAOE);
            GUITools.DrawCheckbox("Rainbow", ref PlayerFlashlightRainbow);
            GUILayout.EndVertical();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(458, 130, 450, 315), "", "Box");
            GUILayout.Label("Other Flashlights");
            if (GUITools.DrawColorPicker(new Rect(4, 30, 442, 160), "Colour", ref Configs.OthersFlashlightColor, Configs.DefaultColor, false, 0f, 1f))
            {
                UpdateFlashlightColor("OthersFlashlight", OthersFlashlightColor);
            }
            GUILayout.Space(160);
            GUILayout.BeginVertical("Box");
            DrawLabeledSlider("Angle", ref OthersFlashlightAngle, 15f, 160, DefaultAngle);
            DrawLabeledSlider("Range", ref OthersFlashlightRange, 0, (PrecisionMode.Value ? 25 : 100), DefaultRange);
            DrawLabeledSlider("Intensity", ref OthersFlashlightIntensity, 0, (PrecisionMode.Value ? 1000 : 10000), DefaultIntensity);
            GUITools.DrawCheckbox("Area Of Effect Flashlight", ref OthersFlashlightAOE);
            GUITools.DrawCheckbox("Rainbow", ref OthersFlashlightRainbow);
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        public static void DrawLabeledSlider(string label, ref ConfigEntry<float> value, float minValue, float maxValue, float defaultValue)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label} {value.Value:F3}", GUILayout.Width(230));
            if (GUILayout.Button("Reset", GUILayout.Width(60)))
            {
                value.Value = defaultValue;
            }
            GUITools.DrawSlider(ref value, minValue, maxValue);
            GUILayout.EndHorizontal();
        }

        internal static void UpdateFlashlightColor(string colorPrefix, Color color)
        {
            BepinPlugin.instance.Config.Bind(colorPrefix, "R", 1f).Value = color.r;
            BepinPlugin.instance.Config.Bind(colorPrefix, "G", 1f).Value = color.g;
            BepinPlugin.instance.Config.Bind(colorPrefix, "B", 1f).Value = color.b;
        }

        internal static ConfigEntry<bool> SeperateFlashlights;
        internal static ConfigEntry<bool> PrecisionMode;
        internal static Color PlayerFlashlightColor; internal static Color OthersFlashlightColor;
        internal static ConfigEntry<bool> PlayerFlashlightRainbow; internal static ConfigEntry<bool> OthersFlashlightRainbow;
        internal static ConfigEntry<bool> PlayerFlashlightAOE; internal static ConfigEntry<bool> OthersFlashlightAOE;
        internal static ConfigEntry<float> RainbowSpeed;
        internal static ConfigEntry<float> PlayerFlashlightAngle; internal static ConfigEntry<float> OthersFlashlightAngle;
        internal static ConfigEntry<float> PlayerFlashlightRange; internal static ConfigEntry<float> OthersFlashlightRange;
        internal static ConfigEntry<float> PlayerFlashlightIntensity; internal static ConfigEntry<float> OthersFlashlightIntensity;
    }
}