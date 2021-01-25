﻿using Diplomacy.ViewModel;
using HarmonyLib;
using SandBox.GauntletUI;
using System.Reflection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;

namespace Diplomacy.Patches
{
    [HarmonyPatch(typeof(EncyclopediaData))]
    class EncyclopediaDataPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("GetEncyclopediaPageInstance")]
        public static void GetEncyclopediaPageInstancePatch(ref EncyclopediaPageVM __result)
        {
            if (__result is EncyclopediaHeroPageVM)
            {
                var args = (EncyclopediaPageArgs)typeof(EncyclopediaPageVM).GetField("_args", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__result);
                __result = new EncyclopediaHeroPageVMExtensionVM(args);
            }
            else if (__result is EncyclopediaFactionPageVM)
            {
                var args = (EncyclopediaPageArgs)typeof(EncyclopediaPageVM).GetField("_args", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__result);
                __result = new EncyclopediaFactionPageVMExtensionVM(args);
            }
        }
    }
}