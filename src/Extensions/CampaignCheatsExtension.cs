﻿using Diplomacy.DiplomaticAction.Alliance;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Diplomacy.Extensions
{
    class CampaignCheatsExtension
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("form_alliance", "campaign")]
        public static string FormAlliance(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
            {
                return CampaignCheats.ErrorType;
            }
            if (!CampaignCheats.CheckParameters(strings, 2) || CampaignCheats.CheckHelp(strings))
            {
                return "Format is faction names without space \"campaign.form_alliance [Faction1] [Faction2]\".";
            }
            var b = strings[0].ToLower();
            var b2 = strings[1].ToLower();
            Kingdom faction = null;
            Kingdom faction2 = null;
            foreach (var faction3 in Campaign.Current.Kingdoms)
            {
                var a = faction3.Name.ToString().ToLower().Replace(" ", "");
                if (a == b)
                {
                    faction = faction3;
                }
                else if (a == b2)
                {
                    faction2 = faction3;
                }
            }
            if (faction is not null && faction2 is not null)
            {
                DeclareAllianceAction.Apply(faction as Kingdom, faction2 as Kingdom, bypassCosts: true);
                return string.Concat(new object[]
                {
                    "Alliance declared between ",
                    faction.Name,
                    " and ",
                    faction2.Name
                });
            }
            if (faction is null)
            {
                return "Faction is not found: " + faction;
            }
            return "Faction is not found: " + faction2;
        }
    }
}