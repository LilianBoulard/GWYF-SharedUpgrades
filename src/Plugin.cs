using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace SharedUpgrades
{
    [BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static ConfigEntry<bool> ConfigEnabled;
        internal static ConfigEntry<bool> ConfigVerboseLogging;

        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;

            ConfigEnabled = Config.Bind(
                "General",
                "Enabled",
                true,
                "When true, any upgrade applied to one player is mirrored to every other player. Host-side only - clients without the mod see no behavior change.");

            ConfigVerboseLogging = Config.Bind(
                "General",
                "VerboseLogging",
                false,
                "When true, logs every fan-out event to BepInEx/LogOutput.log. Useful for verifying the mod is working; spammy in normal play.");

            _harmony = new Harmony(PluginInfo.Guid);
            _harmony.PatchAll(typeof(Patches.UpgradeManagerPatch));

            Log.LogInfo($"{PluginInfo.Name} {PluginInfo.Version} loaded (enabled={ConfigEnabled.Value}).");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }

    internal static class PluginInfo
    {
        public const string Guid = "com.lilianb.sharedupgrades";
        public const string Name = "Shared Upgrades";
        public const string Version = "1.0.0";
    }
}
