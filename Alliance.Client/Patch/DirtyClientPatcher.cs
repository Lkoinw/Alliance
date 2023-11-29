﻿using Alliance.Client.Patch.HarmonyPatch;
using static Alliance.Common.Utilities.Logger;

namespace Alliance.Client.Patch
{
    /// <summary>
    /// This class is responsible for applying Harmony patches and fixing native code in "roundabout" ways.
    /// Must be used as last resort.    
    /// </summary>
    public static class DirtyClientPatcher
    {
        public static bool Patch()
        {
            bool patchSuccess = true;
            patchSuccess &= Patch_MissionMultiplayerGameModeFlagDominationClient.Patch();
            patchSuccess &= Patch_KeyBinder.Patch();
            patchSuccess &= Patch_MissionNetworkComponent.Patch();
            patchSuccess &= Patch_HeroClassVM.Patch();

            //TODO : Check if can be completely removed from 1.2
            //patchSuccess &= Patch_WidgetsMultiplayerHelper.Patch(); 

            if (patchSuccess) Log(SubModule.ModuleId + " - Patches successful", LogLevel.Information);
            return patchSuccess;
        }
    }
}
