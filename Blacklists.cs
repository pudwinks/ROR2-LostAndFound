using System.Collections.Generic;

namespace LostAndFound
{
    public static class Blacklists
    {
        // Display name tokens (or object names) that never go in the list.
        public static readonly HashSet<string> blacklistedNames = new HashSet<string>
        {
            "FROG_NAME",
            "BAZAAR_CAULDRON_NAME",
            "MOON_BATTERY_SOUL_NAME",
            "MOON_BATTERY_BLOOD_NAME",
            "MOON_BATTERY_DESIGN_NAME",
            "MOON_BATTERY_MASS_NAME",
            "VENDING_MACHINE_NAME",
            "SHRINE_COMBAT_NAME",
            "SHRINE_BLOOD_NAME",
            "SHRINE_HEALING_NAME",
            "SHRINE_RESTACK_NAME",
            "SHRINE_COLOSSUS_NAME",
            "SHRINE_CLEANSE_NAME",
            "LOCKEDTREEBOT_NAME",
            "MULTISHOP_TERMINAL_NAME",
            "DRONE_VENDOR_TERMINAL_NAME",
            "FAN_NAME",
            "NULL_WARD_NAME",

            // Alloyed Collective
            "DRIFTERBAGCHEST_NAME",
            "COINSLOT_STATUE_NAME",
            "BROKENROBOT_NAME",
            "COLLECTIVE_SHRINE_NAME",
        };

        // Display name tokens hidden by their own "Show ..." option.
        public const string newtAltarName = "NEWT_STATUE_NAME";
        public const string lunarPodName = "LUNAR_CHEST_NAME";
        public const string droneScrapperName = "DRONE_SCRAPPER_NAME";

        // White and green printers share this token; red and boss printers have their own and always show.
        public const string printerName = "DUPLICATOR_NAME";

        // Item name token of a ground pickup that never goes in the list.
        public const string powerPyramidName = "ITEM_POWERPYRAMID_NAME";

        // Display name tokens of drones hidden by the "Show drones" option.
        public static readonly HashSet<string> droneNames = new HashSet<string>
        {
            "DRONE_GUNNER_INTERACTABLE_NAME",
            "DRONE_HEALING_INTERACTABLE_NAME",
            "DRONE_MISSILE_INTERACTABLE_NAME",
            "EQUIPMENTDRONE_INTERACTABLE_NAME",
            "FLAMEDRONE_INTERACTABLE_NAME",
            "EMERGENCYDRONE_INTERACTABLE_NAME",
            "TURRET1_INTERACTABLE_NAME",

            // Alloyed Collective drones
            "DRONE_JUNK_INTERACTABLE_NAME",
            "DRONE_HAULER_INTERACTABLE_NAME",
            "DRONE_JAILER_INTERACTABLE_NAME",
            "DRONE_CLEANUP_INTERACTABLE_NAME",
            "DRONE_COPYCAT_INTERACTABLE_NAME",
            "DRONE_RECHARGE_INTERACTABLE_NAME",
            "DRONE_BOMBARDMENT_INTERACTABLE_NAME",
        };
    }
}
