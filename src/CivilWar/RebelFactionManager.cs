﻿using Diplomacy.Costs;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace Diplomacy.CivilWar
{
    internal sealed class RebelFactionManager
    {
        public static RebelFactionManager? Instance { get; private set; }

        [SaveableProperty(1)]
        public Dictionary<Kingdom, List<RebelFaction>> RebelFactions { get; private set; }
        [SaveableProperty(2)]
        public List<Kingdom> DeadRebelKingdoms { get; private set; }
        [SaveableProperty(3)]
        public Dictionary<Kingdom, CampaignTime> LastCivilWar { get; private set; }

        public RebelFactionManager()
        {
            RebelFactions = new();
            DeadRebelKingdoms = new();
            LastCivilWar = new();
            Instance = this;
        }

        internal void Sync()
        {
            Instance = this;
        }

        public static void RegisterRebelFaction(RebelFaction rebelFaction)
        {
            Kingdom kingdom = rebelFaction.ParentKingdom;
            Clan clan = rebelFaction.SponsorClan;
            if (!CanStartRebelFaction(clan, out _))
            {
                return;
            }

            if (Instance!.RebelFactions.TryGetValue(kingdom, out List<RebelFaction> rebelFactions))
            {
                // if we're starting a secession faction, remove this clan from other secession factions
                if (rebelFaction.RebelDemandType == RebelDemandType.Secession)
                {
                    var otherSecessionFactions = rebelFactions.Where(x => x.RebelDemandType == RebelDemandType.Secession && x.Clans.Contains(rebelFaction.SponsorClan));
                    foreach (RebelFaction faction in otherSecessionFactions)
                    {
                        faction.RemoveClan(rebelFaction.SponsorClan);
                    }
                }
                Instance!.RebelFactions[kingdom].Add(rebelFaction);
            }
            else
            {
                List<RebelFaction> newRebelFactions = new() { rebelFaction };
                Instance!.RebelFactions[kingdom] = newRebelFactions;
            }

        }

        public static void DestroyRebelFaction(RebelFaction rebelFaction, bool rebelKingdomSurvived = false)
        {
            if (rebelFaction.AtWar && rebelFaction.RebelKingdom is not null)
            {
                Instance!.LastCivilWar[rebelFaction.ParentKingdom] = CampaignTime.Now;

                if (rebelKingdomSurvived)
                {
                    Instance!.LastCivilWar[rebelFaction.RebelKingdom!] = CampaignTime.Now;
                }
                else
                {
                    Instance!.DeadRebelKingdoms.Add(rebelFaction.RebelKingdom!);
                }
            }
            Instance!.RebelFactions[rebelFaction.ParentKingdom].Remove(rebelFaction);
        }

        public static bool CanStartRebelFaction(Clan clan, out TextObject? reason)
        {
            if (Instance!.RebelFactions.TryGetValue(clan.Kingdom, out List<RebelFaction> rebelFactions))
            {
                if (rebelFactions.Where(x => x.AtWar).Any())
                {
                    reason = new TextObject("{=ovgs58sT}Can't start a faction during an active rebellion.");
                    return false;
                }

                // players can exceed the max
                if (rebelFactions.Count >= 3 && clan != Clan.PlayerClan)
                {
                    reason = TextObject.Empty;
                    return false;
                }

            }

            if (Instance!.LastCivilWar.TryGetValue(clan.Kingdom, out CampaignTime lastTime))
            {
                float daysSinceLastCivilWar = lastTime.ElapsedDaysUntilNow;

                if (daysSinceLastCivilWar < Settings.Instance!.MinimumTimeSinceLastCivilWarInDays)
                {
                    reason = new TextObject("{=VbpiW2bd}Can't start a faction so soon after a civil war.");
                    return false;
                }
            }

            if (!new InfluenceCost(clan, Settings.Instance!.FactionCreationInfluenceCost).CanPayCost())
            {
                reason = new TextObject(StringConstants.NOT_ENOUGH_INFLUENCE);
                return false;
            }

            reason = null;
            return true;
        }

        public static bool HasRebelFaction(Kingdom kingdom)
        {
            return Instance!.RebelFactions.ContainsKey(kingdom);
        }

        public static IEnumerable<RebelFaction> GetRebelFaction(Kingdom kingdom)
        {
            if (kingdom is null)
            {
                return Enumerable.Empty<RebelFaction>();
            }

            if (Instance!.RebelFactions.TryGetValue(kingdom, out List<RebelFaction> rebelFactions))
            {
                return new List<RebelFaction>(rebelFactions);
            }
            else
            {
                return Enumerable.Empty<RebelFaction>();
            }
        }

        public static IReadOnlyDictionary<Kingdom, List<RebelFaction>> AllRebelFactions { get => Instance!.RebelFactions; }
    }
}