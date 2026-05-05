using Extensions;
using HarmonyLib;
using Mirror;

namespace SharedUpgrades.Patches
{
    /// <summary>
    /// Postfix on <see cref="UpgradeManager.ChangeUpgradeData"/>: after the server applies an
    /// upgrade delta to one player's steamId, re-apply the same delta to every other player.
    /// A re-entrancy guard prevents the postfix from recursing into itself during fan-out.
    /// </summary>
    [HarmonyPatch(typeof(UpgradeManager), nameof(UpgradeManager.ChangeUpgradeData))]
    internal static class UpgradeManagerPatch
    {
        private static bool _isFanningOut;

        [HarmonyPostfix]
        private static void Postfix(ulong steamId, PlayerUpgradeType type, float amount)
        {
            if (!Plugin.ConfigEnabled.Value) return;
            if (!NetworkServer.active) return;
            if (_isFanningOut) return;

            var local = MonoSingleton<LocalManager>.Instance;
            if (local == null) return;
            var players = local.players;
            if (players == null || players.Count <= 1) return;

            var manager = NetworkSingleton<UpgradeManager>.Instance;
            if (manager == null) return;

            _isFanningOut = true;
            try
            {
                int sharedCount = 0;
                foreach (var p in players)
                {
                    if (p == null || p.profile == null) continue;
                    var other = p.profile.steamId;
                    if (other == steamId) continue;
                    manager.ChangeUpgradeData(other, type, amount);
                    sharedCount++;
                }

                if (sharedCount > 0 && Plugin.ConfigVerboseLogging.Value)
                {
                    Plugin.Log.LogInfo($"Shared {type} (+{amount}) from {steamId} to {sharedCount} other player(s).");
                }
            }
            finally
            {
                _isFanningOut = false;
            }
        }
    }
}
