using StardewModdingAPI.Utilities;

namespace CalendarBuffs
{
    public sealed class ModConfig
    {
        public Dictionary<string, string> BuffIcons = new Dictionary<string, string>()
        {
            { "Bookseller", "102" }, // Lost Book

            { "Festival_spring13", "174" }, // Egg Festival -> Large Egg
            { "Festival_spring24", "591" }, // Flower Dance -> Tulip
            { "Festival_summer11", "199" }, // Luau -> Parsnip Soup
            { "Festival_summer28", "TileSheets\\Objects_2:134" }, // Dance of the Moonlight Jellies -> Sea Jelly
            { "Festival_fall16", "241" }, // Stardew Valley Fair -> Survival Burger
            { "Festival_fall27", "746" }, // Spirit's Eve -> Jack-O-Lantern
            { "Festival_winter8", "319" }, // Festival of Ice -> Ice Crystal
            { "Festival_winter25", "283" }, // Feast of the Winter Star -> Holly

            { "PassiveFestival_desertfestival", "TileSheets\\Objects_2:7" }, // Calico Egg
            { "PassiveFestival_troutderby", "138" }, // Rainbow Trout
            { "PassiveFestival_squidfest", "151" }, // Squid
            { "PassiveFestival_nightmarket", "799" } // Spook Fish
        };
    }
}
