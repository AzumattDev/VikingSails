using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using LocalizationManager;
using ServerSync;
using UnityEngine;

namespace VikingSails
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class VikingSailsPlugin : BaseUnityPlugin
    {
        internal const string ModName = "VikingSails";
        internal const string ModVersion = "1.1.5";
        internal const string Author = "Azumatt";
        private const string ModGUID = $"{Author}.{ModName}";
        private static string ConfigFileName = $"{ModGUID}.cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);
        
        internal static VikingSailsPlugin instance = null!;

        public static readonly ManualLogSource VikingSailsLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        private static readonly ConfigSync ConfigSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        public enum Toggle
        {
            On = 1,
            Off = 0
        }

        public void Awake()
        {
            instance = this;
            Localizer.Load();
            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On,
                new ConfigDescription("If on, the configuration is locked and can be changed by server admins only.", null, new ConfigurationManagerAttributes { Order = 6 }));
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);
            
            useServerSailURL = config("1 - General", "Use Server Sail URL", Toggle.Off, new ConfigDescription("A toggle that when turned on, sets all ship sails to use the Server Sail URL.", null, new ConfigurationManagerAttributes { Order = 5 }));
            serverSailURL = TextEntryConfig("1 - General", "Server Sail URL", "https://i.imgur.com/TbcJ3LU.png", new ConfigDescription("Put a valid image URL here, this entry field should contain the URL of the image to use for the sails when the `Use Server Sail URL` toggle is on.", null, new ConfigurationManagerAttributes { Order = 4 }));
            keyConfig = config("1 - General", "Edit Key", new KeyboardShortcut(KeyCode.Mouse1), new ConfigDescription("A keyboard shortcut that allows the player to interact with the ship to change the sail image", null, new ConfigurationManagerAttributes { Order = 3 }), false);
            requireKeyConfig = config("1 - General", "Require Key Press", Toggle.Off, new ConfigDescription($"A toggle that when turned on, requires the player to hold down the `Edit Key` <{keyConfig.Value.ToString()}> in order to interact with the ship to change the sails.", null, new ConfigurationManagerAttributes { Order = 2 }));
            showURLOnHover = config("1 - General", "Show URL On Hover", Toggle.Off, new ConfigDescription($"A toggle that when turned on, will show the URL after the interaction prompt so you might see the URL at quick glance. Note only will show to you if you have access to change the URL", null, new ConfigurationManagerAttributes { Order = 1 }), false);
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }

        public void Start()
        {
            AutoDoc();
        }
        
        internal static void AutoDoc()
        {
#if DEBUG

            // Store Regex to get all characters after a [
            Regex regex = new(@"\[(.*?)\]");

            // Strip using the regex above from Config[x].Description.Description
            string Strip(string x) => regex.Match(x).Groups[1].Value;
            StringBuilder sb = new();
            string lastSection = "";
            foreach (ConfigDefinition x in instance.Config?.Keys)
            {
                // skip first line
                if (x.Section != lastSection)
                {
                    lastSection = x.Section;
                    sb.Append($"{Environment.NewLine}`{x.Section}`{Environment.NewLine}");
                }

                sb.Append($"\n{x.Key} [{Strip(instance.Config?[x].Description.Description)}]" +
                          $"{Environment.NewLine}   * {instance.Config?[x].Description.Description.Replace("[Synced with Server]", "").Replace("[Not Synced with Server]", "")}" +
                          $"{Environment.NewLine}     * Default Value: {instance.Config?[x].GetSerializedValue()}{Environment.NewLine}");
            }

            File.WriteAllText(
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                    $"{ModName}_AutoDoc.md"), sb.ToString());
#endif
        }

        private void OnDestroy()
        {
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                VikingSailsLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                VikingSailsLogger.LogError($"There was an issue loading your {ConfigFileName}");
                VikingSailsLogger.LogError("Please check your config entries for spelling and format!");
            }
        }
        
        public static bool AllowInput()
        {
            if (requireKeyConfig.Value == Toggle.On && keyConfig.Value.IsKeyHeld()) { return true;}
            return requireKeyConfig.Value == Toggle.Off;
        }
        
        internal static void TextAreaDrawer(ConfigEntryBase entry)
        {
            GUILayout.ExpandHeight(true);
            GUILayout.ExpandWidth(true);
            entry.BoxedValue = GUILayout.TextArea((string)entry.BoxedValue, GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));
        }


        #region ConfigOptions

        private static ConfigEntry<Toggle> _serverConfigLocked = null!;
        private static ConfigEntry<Toggle> requireKeyConfig = null!;
        internal static ConfigEntry<Toggle> showURLOnHover = null!;
        private static ConfigEntry<KeyboardShortcut> keyConfig = null!;
        internal static ConfigEntry<Toggle> useServerSailURL = null!;
        internal static ConfigEntry<string> serverSailURL = null!;

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }
        
        internal ConfigEntry<T> TextEntryConfig<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigurationManagerAttributes attributes = new()
            {
                CustomDrawer = TextAreaDrawer
            };
            return config(group, name, value, description, synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            [UsedImplicitly] public int? Order;
            [UsedImplicitly] public bool? Browsable;
            [UsedImplicitly] public string? Category;
            [UsedImplicitly] public bool? HideSettingName;
            [UsedImplicitly] public bool? HideDefaultButton;
            [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer;
        }

        #endregion
    }
    
    public static class KeyboardExtensions
    { 
        // thank you to 'Margmas' for giving me this snippet from VNEI https://github.com/MSchmoecker/VNEI/blob/master/VNEI/Logic/BepInExExtensions.cs#L21
        // since KeyboardShortcut.IsPressed and KeyboardShortcut.IsDown behave unintuitively
        public static bool IsKeyDown(this KeyboardShortcut shortcut)
        {
            return shortcut.MainKey != KeyCode.None && Input.GetKeyDown(shortcut.MainKey) && shortcut.Modifiers.All(Input.GetKey);
        }

        public static bool IsKeyHeld(this KeyboardShortcut shortcut)
        {
            return shortcut.MainKey != KeyCode.None && Input.GetKey(shortcut.MainKey) && shortcut.Modifiers.All(Input.GetKey);
        }
    }
}