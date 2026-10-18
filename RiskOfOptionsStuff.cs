using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;

namespace LostAndFound
{
    // RiskOfOptions is a soft dependency. Its types are only touched inside the NoInlining methods,
    // so its assembly never gets loaded when it isn't installed and the config file works on its own.
    public static class RiskOfOptionsStuff
    {
        private static bool? enabled;

        // PLUGIN_GUID is a const, so reading it doesn't load the RiskOfOptions assembly.
        public static bool Enabled
        {
            get
            {
                if (enabled == null)
                    enabled = Chainloader.PluginInfos.ContainsKey(RiskOfOptions.PluginInfo.PLUGIN_GUID);

                return enabled.Value;
            }
        }

        public static void AddOption(ConfigEntry<bool> entry)
        {
            if (Enabled)
                AddCheckBox(entry);
        }

        public static void AddOption(ConfigEntry<KeyboardShortcut> entry)
        {
            if (Enabled)
                AddKeyBind(entry);
        }

        // The slider range comes from the entry's AcceptableValueRange.
        public static void AddOption(ConfigEntry<int> entry)
        {
            if (Enabled)
                AddIntSlider(entry);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void AddCheckBox(ConfigEntry<bool> entry)
        {
            ModSettingsManager.AddOption(new CheckBoxOption(entry));
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void AddKeyBind(ConfigEntry<KeyboardShortcut> entry)
        {
            ModSettingsManager.AddOption(new KeyBindOption(entry));
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void AddIntSlider(ConfigEntry<int> entry)
        {
            AcceptableValueRange<int> range = (AcceptableValueRange<int>)entry.Description.AcceptableValues;
            ModSettingsManager.AddOption(new IntSliderOption(entry, new IntSliderConfig { min = range.MinValue, max = range.MaxValue }));
        }
    }
}
