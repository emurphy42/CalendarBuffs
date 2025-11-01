using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using StardewValley.TokenizableStrings;

namespace CalendarBuffs
{
    public class ModEntry : Mod
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod configuration from the player.</summary>
        private ModConfig Config;

        private const string BuffIconDelimiter = ":";
        private const string DefaultBuffIconTexture = "Maps\\Springobjects";
        private const string CustomBuffIconAttribute = "JohnPeters.CalendarBuffs/BuffIcon";

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();

            Helper.Events.GameLoop.DayStarted += (_sender, _e) => OnDayStarted(_sender, _e);
        }

        /*********
        ** Private methods
        *********/
        private string GetNormalizedIcon(string buffIcon)
        {
            if (!buffIcon.Contains(BuffIconDelimiter))
            {
                buffIcon = $"{DefaultBuffIconTexture}{BuffIconDelimiter}{buffIcon}";
            }
            return buffIcon;
        }

        private string GetTextureFromIcon(string buffIcon)
        {
            return GetNormalizedIcon(buffIcon).Split(BuffIconDelimiter)[0];
        }

        private string GetSheetIndexFromIcon(string buffIcon)
        {
            return GetNormalizedIcon(buffIcon).Split(BuffIconDelimiter)[1];
        }

        private string GetIconTexture(string buffIconID, string defaultBuffIcon)
        {
            return GetTextureFromIcon(
                this.Config.BuffIcons.ContainsKey(buffIconID)
                    ? this.Config.BuffIcons[buffIconID]
                    : defaultBuffIcon
            );
        }

        private int GetIconSheetIndex(string buffIconID, string defaultBuffIcon)
        {
            return int.Parse(GetSheetIndexFromIcon(
                this.Config.BuffIcons.ContainsKey(buffIconID)
                    ? this.Config.BuffIcons[buffIconID]
                    : defaultBuffIcon
            ));
        }

        private void OnDayStarted(object? _sender, DayStartedEventArgs _e)
        {
            this.Monitor.Log("[Calendar Buffs] Reacting to DayStarted", LogLevel.Trace);
            Game1.addMorningFluffFunction((Action)(() => AddCalendarBuffs()));
        }

        private void AddCalendarBuffs()
        {
            this.Monitor.Log("[Calendar Buffs] Checking bookseller", LogLevel.Trace);
            AddCalendarBuff_Bookseller();

            this.Monitor.Log("[Calendar Buffs] Checking festivals", LogLevel.Trace);
            AddCalendarBuffs_Festivals();

            this.Monitor.Log("[Calendar Buffs] Checking passive festivals", LogLevel.Trace);
            AddCalendarBuffs_PassiveFestivals();
        }

        private void AddCalendarBuff_Bookseller()
        {
            if (!Utility.getDaysOfBooksellerThisSeason().Contains(Game1.dayOfMonth))
            {
                return;
            }

            this.Monitor.Log("[Calendar Buffs] Adding bookseller buff", LogLevel.Debug);

            var defaultBuffIconValue = "Maps\\springobjects:102"; // Lost Book
            var iconTexture = GetIconTexture("Bookseller", defaultBuffIconValue);
            var iconSheetIndex = GetIconSheetIndex("Bookseller", defaultBuffIconValue);

            Game1.player.applyBuff(new Buff(
                id: "JohnPeters_CalendarBuff_Bookseller",
                source: "Calendar",
                displaySource: Helper.Translation.Get("Calendar"),
                duration: Buff.ENDLESS,
                iconTexture: Game1.content.Load<Texture2D>(iconTexture),
                iconSheetIndex: iconSheetIndex,
                displayName: Helper.Translation.Get("Bookseller"),
                description: Game1.content.LoadString("Strings\\1_6_Strings:BooksellerInTown")
            ));
        }

        private void AddCalendarBuffs_Festivals()
        {
            var seasonDay = Game1.currentSeason + Game1.dayOfMonth.ToString();
            var assetName = "Data\\Festivals\\" + seasonDay;
            if (!Game1.temporaryContent.DoesAssetExist<Dictionary<string, string>>(assetName))
            {
                return;
            }
            var dictionary = Game1.temporaryContent.Load<Dictionary<string, string>>(assetName);
            if (dictionary == null)
            {
                return;
            }

            this.Monitor.Log($"[Calendar Buffs] Adding festival buff for {seasonDay}", LogLevel.Debug);

            // priority:
            //   1) icon from this mod's config
            //   2) icon from Data/Festivals/<season><day> -> (CustomBuffIconAttribute)
            //   3) generic icon
            var buffIconID = $"Festival_{seasonDay.ToLower()}";
            var defaultBuffIconValue = "Maps\\springobjects:893"; // Fireworks (Red)
            if (dictionary.ContainsKey(CustomBuffIconAttribute))
            {
                defaultBuffIconValue = dictionary[CustomBuffIconAttribute];
            }
            var iconTexture = GetIconTexture(buffIconID, defaultBuffIconValue);
            var iconSheetIndex = GetIconSheetIndex(buffIconID, defaultBuffIconValue);

            var description = "";
            try
            {
                var placeTime = dictionary["conditions"].Split("/");
                var startEndTime = placeTime[1].Split(" ");
                description = Helper.Translation.Get("FestivalPlaceTime", new {
                    Place = placeTime[0],
                    StartTime = Game1.getTimeOfDayString(int.Parse(startEndTime[0])),
                    EndTime = Game1.getTimeOfDayString(int.Parse(startEndTime[1]))
                });
            }
            catch (Exception)
            {
                description = dictionary["conditions"];
            }

            Game1.player.applyBuff(new Buff(
                id: "JohnPeters_CalendarBuff_Festival",
                source: "Calendar",
                displaySource: Helper.Translation.Get("Calendar"),
                duration: Buff.ENDLESS,
                iconTexture: Game1.content.Load<Texture2D>(iconTexture),
                iconSheetIndex: iconSheetIndex,
                displayName: dictionary["name"],
                description: description
            ));
        }

        private void AddCalendarBuffs_PassiveFestivals()
        {
            var dictionary = Game1.temporaryContent.Load<Dictionary<string, PassiveFestivalData>>("Data\\PassiveFestivals");
            foreach (var passiveFestivalID in dictionary.Keys)
            {
                this.Monitor.Log($"[Calendar Buffs] Checking {passiveFestivalID}", LogLevel.Trace);
                if (!Utility.IsPassiveFestivalDay(passiveFestivalID))
                {
                    continue;
                }

                this.Monitor.Log($"[Calendar Buffs] Adding passive festival buff for {passiveFestivalID}", LogLevel.Debug);

                var passiveFestival = dictionary[passiveFestivalID];

                // priority:
                //   1) icon from this mod's config
                //   2) icon from Data/PassiveFestivals -> CustomFields -> (CustomBuffIconAttribute)
                //   3) generic icon
                var buffIconID = $"PassiveFestival_{passiveFestivalID.ToLower()}";
                var defaultBuffIconValue = "Maps\\springobjects:893"; // Fireworks (Red)
                if (passiveFestival.CustomFields != null && passiveFestival.CustomFields.ContainsKey(CustomBuffIconAttribute))
                {
                    defaultBuffIconValue = passiveFestival.CustomFields[CustomBuffIconAttribute];
                }
                var iconTexture = GetIconTexture(buffIconID, defaultBuffIconValue);
                var iconSheetIndex = GetIconSheetIndex(buffIconID, defaultBuffIconValue);

                var description = "";
                try
                {
                    description = Game1.getTimeOfDayString(passiveFestival.StartTime);
                }
                catch (Exception)
                {
                    // do nothing
                }

                var displayName = TokenParser.ParseText(passiveFestival.DisplayName);
                if (string.IsNullOrWhiteSpace(displayName))
                {
                    displayName = TokenParser.ParseText(passiveFestival.StartMessage);
                }

                Game1.player.applyBuff(new Buff(
                    id: "JohnPeters_CalendarBuff_PassiveFestival",
                    source: "Calendar",
                    displaySource: Helper.Translation.Get("Calendar"),
                    duration: Buff.ENDLESS,
                    iconTexture: Game1.content.Load<Texture2D>(iconTexture),
                    iconSheetIndex: iconSheetIndex,
                    displayName: displayName,
                    description: description
                ));
            }
        }
    }
}
