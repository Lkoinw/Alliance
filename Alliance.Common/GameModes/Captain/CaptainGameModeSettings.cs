﻿using Alliance.Common.Utilities;
using System.Collections.Generic;
using System.Linq;
using static TaleWorlds.MountAndBlade.MultiplayerOptions;

namespace Alliance.Common.GameModes.Captain
{
    public class CaptainGameModeSettings : GameModeSettings
    {
        public CaptainGameModeSettings() : base("CaptainX", "Captain", "Captain")
        {
        }

        public override void SetDefaultNativeOptions()
        {
            base.SetDefaultNativeOptions();
        }

        public override void SetDefaultModOptions()
        {
            base.SetDefaultModOptions();
        }

        public override List<string> GetAvailableMaps()
        {
            return SceneList.Scenes
                    .Where(scene => new[] { "captain_", "sergeant_" }
                    .Any(prefix => scene.Contains(prefix)))
                    .ToList();
        }

        public override List<OptionType> GetAvailableNativeOptions()
        {
            return new List<OptionType>
            {
                OptionType.CultureTeam1,
                OptionType.CultureTeam2,
                OptionType.NumberOfBotsPerFormation,
                OptionType.NumberOfBotsTeam1,
                OptionType.NumberOfBotsTeam2,
                OptionType.RoundPreparationTimeLimit,
                OptionType.RoundTimeLimit,
                OptionType.RoundTotal,
                OptionType.WarmupTimeLimit,
                OptionType.FriendlyFireDamageMeleeFriendPercent,
                OptionType.FriendlyFireDamageMeleeSelfPercent,
                OptionType.FriendlyFireDamageRangedFriendPercent,
                OptionType.FriendlyFireDamageRangedSelfPercent
            };
        }

        public override List<string> GetAvailableModOptions()
        {
            return base.GetAvailableModOptions();
        }
    }
}