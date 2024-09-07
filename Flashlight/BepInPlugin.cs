using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Gameplay.Chat;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Profiling;
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
            profilesDirectiory = Path.Combine(Paths.ConfigPath, "FlashlightProfiles");
            Directory.CreateDirectory(profilesDirectiory);
            Configs.Load();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }
        internal static string profilesDirectiory;
    }
    internal class ProfileData
    {
        public string Name;
        public ConfigFile ConfigFile;
        public Color Colour;
        public ConfigEntry<float> Angle;
        public ConfigEntry<float> Range;
        public ConfigEntry<float> Intensity;
        public ConfigEntry<bool> AOE;
        public ConfigEntry<bool> Rainbow;
    }
    public class Configs : ModSettingsMenu
    {
        public override string Name() => $"{MyPluginInfo.PLUGIN_NAME} Config";
        
        private static Color DefaultColor = Color.white;
        private static float DefaultAngle = 45.1f;
        private static float DefaultRange = 25;
        private static float DefaultIntensity = 416.34f;

        internal static ConfigEntry<string> PlayerFlashlightProfile;  internal static ProfileData playerFlashlight;
        internal static ConfigEntry<string> OthersFlashlightProfile;  internal static ProfileData othersFlashlight;

        internal static void Load()
        {
            SeperateFlashlights = BepinPlugin.instance.Config.Bind("SeperateFlashlights", "Enabled", true);
            PrecisionMode = BepinPlugin.instance.Config.Bind("PrecisionMode", "Enabled", true);
            RainbowSpeed = BepinPlugin.instance.Config.Bind("RainbowFlashlight", "Speed", 0.125f);
            PlayerFlashlightProfile = BepinPlugin.instance.Config.Bind("Profile", "Player", "default");
            playerFlashlight = Configs.LoadProfile(PlayerFlashlightProfile.Value);
            OthersFlashlightProfile = BepinPlugin.instance.Config.Bind("Profile", "Others", "default");
            othersFlashlight = Configs.LoadProfile(OthersFlashlightProfile.Value);
        }

        internal static ProfileData LoadProfile(string profileName)
        {
            string configPath = Path.Combine(BepinPlugin.profilesDirectiory, $"{profileName}.cfg");
            ConfigFile currentConfigFile = new ConfigFile(configPath, true);
            ProfileData profile = new ProfileData();
            profile.Name = profileName;
            profile.ConfigFile = currentConfigFile;
            profile.Colour = LoadColor("Colour", currentConfigFile);
            profile.Angle = currentConfigFile.Bind("Settings", "Angle", DefaultAngle);
            profile.Range = currentConfigFile.Bind("Settings", "Range", DefaultRange);
            profile.Intensity = currentConfigFile.Bind("Settings", "Intensity", DefaultIntensity);
            profile.AOE = currentConfigFile.Bind("Settings", "AOE", false);
            profile.Rainbow = currentConfigFile.Bind("Settings", "Rainbow", false);
            BepinPlugin.Log.LogInfo($"Loaded profile: {profileName}");
            return profile;
        }

        private static Color LoadColor(string colorPrefix, ConfigFile currentConfigFile = null)
        {
            if (currentConfigFile == null) currentConfigFile = BepinPlugin.instance.Config;
            var colorR = currentConfigFile.Bind(colorPrefix, "R", 1f);
            var colorG = currentConfigFile.Bind(colorPrefix, "G", 1f);
            var colorB = currentConfigFile.Bind(colorPrefix, "B", 1f);
            return new Color(colorR.Value, colorG.Value, colorB.Value);
        }
        internal static void UpdateColor(string colorPrefix, ref Color color, ConfigFile currentConfigFile = null)
        {
            if (currentConfigFile == null) currentConfigFile = BepinPlugin.instance.Config;
            currentConfigFile.Bind(colorPrefix, "R", 1f).Value = color.r;
            currentConfigFile.Bind(colorPrefix, "G", 1f).Value = color.g;
            currentConfigFile.Bind(colorPrefix, "B", 1f).Value = color.b;
        }
        
        internal static void DrawProfile((float,float) loc, ProfileData profile) // new Rect(0, 130, 450, 315)
        {
            GUILayout.BeginArea(new Rect(loc.Item1, loc.Item2, 450, 315), "", "Box");
            GUILayout.Label(profile.Name);
            if (GUITools.DrawColorPicker(new Rect(4, 30, 442, 160), "Colour", ref profile.Colour, Configs.DefaultColor, false, 0f, 1f))
            {
                UpdateColor("Colour", ref profile.Colour, profile.ConfigFile);
            }
            GUILayout.Space(160);
            GUILayout.BeginVertical("Box");
            DrawLabeledSlider("Angle", ref profile.Angle, 15f, 160, DefaultAngle);
            DrawLabeledSlider("Range", ref profile.Range, 0, (PrecisionMode.Value ? 25 : 100), DefaultRange);
            DrawLabeledSlider("Intensity", ref profile.Intensity, 0, (PrecisionMode.Value ? 1000 : 10000), DefaultIntensity);
            GUITools.DrawCheckbox("Area Of Effect Flashlight", ref profile.AOE);
            GUITools.DrawCheckbox("Rainbow", ref profile.Rainbow);
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        public static bool DrawTextField(string label, ref string value, string defaultValue = null, float minWidth = 80)
        {
            bool changed = false;
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}: ");
            value = GUILayout.TextField(value, GUILayout.MinWidth(minWidth));
            GUILayout.FlexibleSpace();
            if (defaultValue != null)
            {
                if (GUILayout.Button("Reset"))
                {
                    value = defaultValue;
                    changed = true;
                }
            }
            if (GUILayout.Button("Create"))
            {
                changed = true;
            }
            GUILayout.EndHorizontal();
            return changed;
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

        private static Dictionary<string, ProfileData> Profiles = new Dictionary<string, ProfileData>();
        private static ProfileData SelectedProfile = null;
        private static bool HomePage = true;
        public override void Draw()
        {
            if (HomePage) DrawHomePage();
            else if (SelectedProfile == null) DrawProfileList();
            else
            {
                DrawProfile();
            }
        }
        private static void DrawHomePage()
        {
            if (GUILayout.Button("Browse Flashlight Profiles")) { SelectedProfile = null; HomePage = false; }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"Player Flashlight: {PlayerFlashlightProfile.Value}")) { SelectedProfile = playerFlashlight; HomePage = false; }
            if (GUILayout.Button($"Others Flashlight: {OthersFlashlightProfile.Value}")) { SelectedProfile = othersFlashlight; HomePage = false; }
            GUILayout.EndHorizontal();

            DrawProfile((0,130), playerFlashlight);
            DrawProfile((458, 130), othersFlashlight);
        }
        private static void DrawProfile()
        {
            if (GUILayout.Button("Back to Flashlight Profile list")) SelectedProfile = null;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"Player Flashlight: {PlayerFlashlightProfile.Value}")) SelectedProfile = playerFlashlight; HomePage = false;
            if (GUILayout.Button($"Others Flashlight: {OthersFlashlightProfile.Value}")) SelectedProfile = othersFlashlight; HomePage = false;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Assign"))
            {
                PlayerFlashlightProfile.Value = SelectedProfile.Name;
                playerFlashlight = SelectedProfile;
            }
            if (GUILayout.Button("Assign"))
            {
                OthersFlashlightProfile.Value = SelectedProfile.Name;
                othersFlashlight = SelectedProfile;
            }
            GUILayout.EndHorizontal();

            DrawProfile((0, 130), SelectedProfile);
        }

        private static string searchValue = "";
        private static void DrawProfileList()
        {
            if (GUILayout.Button("Home")) { SelectedProfile = null; HomePage = true; }
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Player Flashlight: {PlayerFlashlightProfile.Value}");
            GUILayout.Label($"Others Flashlight: {OthersFlashlightProfile.Value}");
            GUILayout.EndHorizontal();
            string[] profileFiles = Directory.GetFiles(BepinPlugin.profilesDirectiory, "*.cfg");
            if (DrawTextField("Search", ref searchValue, "", 200))
            {
                Profiles.Add(searchValue, LoadProfile(searchValue));
            }
            foreach (string filePath in profileFiles)
            {
                string profileName = Path.GetFileNameWithoutExtension(filePath);
                if (!Profiles.ContainsKey(profileName)) Profiles.Add(profileName, LoadProfile(profileName));
                if (!profileName.ToLower().Contains(searchValue.ToLower()) && searchValue != "") return;
                if (GUILayout.Button(profileName))
                {
                    SelectedProfile = Profiles[profileName];
                }
            }
        }

        internal static ConfigEntry<bool> SeperateFlashlights;
        internal static ConfigEntry<bool> PrecisionMode;
        internal static ConfigEntry<float> RainbowSpeed;
    }
}