using BepInEx.Configuration;
using RiskOfOptions.Options;
using System.Runtime.CompilerServices;

namespace src
{
    public static class RiskOfOptionsStuff
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(RiskOfOptions.PluginInfo.PLUGIN_GUID);
                }
                return (bool)_enabled;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddOption<T>(ConfigEntry<T> opt)
        {
            switch (opt)
            {
                case ConfigEntry<bool> ceBool:
                    RiskOfOptions.ModSettingsManager.AddOption(new CheckBoxOption(ceBool));
                    break;

                case ConfigEntry<KeyboardShortcut> ceKb:
                    RiskOfOptions.ModSettingsManager.AddOption(new KeyBindOption(ceKb));
                    break;
            }
        }
    }
}
