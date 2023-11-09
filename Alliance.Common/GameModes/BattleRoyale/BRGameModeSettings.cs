﻿using Alliance.Common.Core.Configuration.Models;
using Alliance.Common.Utilities;
using System.Collections.Generic;
using System.Linq;
using static TaleWorlds.MountAndBlade.MultiplayerOptions;

namespace Alliance.Common.GameModes.BattleRoyale
{
    public class BRGameModeSettings : GameModeSettings
    {
        public BRGameModeSettings() : base("BattleRoyale", "BattleRoyale", "Last man standing.")
        {
        }

        public override void SetDefaultNativeOptions()
        {
            base.SetDefaultNativeOptions();
            SetNativeOption(OptionType.CultureTeam1, "empire");
            SetNativeOption(OptionType.NumberOfBotsTeam1, 5);
            SetNativeOption(OptionType.NumberOfBotsTeam2, 0);
        }

        public override void SetDefaultModOptions()
        {
            base.SetDefaultModOptions();
            ModOptions.BRZoneLifeTime = 300;
        }

        public override List<string> GetAvailableMaps()
        {
            return SceneList.Scenes
                    .Where(scene => new[] { "character_", "editor_" } // Invalid maps
                    .All(prefix => !scene.Contains(prefix)))
                    .ToList();
        }

        public override List<OptionType> GetAvailableNativeOptions()
        {
            return new List<OptionType>
            {
                OptionType.CultureTeam1,
                OptionType.NumberOfBotsTeam1
            };
        }

        public override List<string> GetAvailableModOptions()
        {
            return new List<string>
            {
                nameof(Config.BRZoneLifeTime),
                nameof(Config.AllowCustomBody),
                nameof(Config.RandomizeAppearance),
                nameof(Config.ShowFlagMarkers),
                nameof(Config.ShowOfficers),
                nameof(Config.ShowWeaponTrail),
                nameof(Config.KillFeedEnabled),
                nameof(Config.OfficerHPMultip)
            };
        }
    }
}