﻿using System;
using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes.CampaignEventBehaviors
{
    class DeclareWarCooldown : CampaignBehaviorBase
    {
        private CooldownManager _cooldownManager;

        public DeclareWarCooldown()
        {
            this._cooldownManager = new CooldownManager();
        }

        public override void RegisterEvents()
        {
            CampaignEvents.MakePeace.AddNonSerializedListener(this, new Action<IFaction, IFaction>(this.RegisterDeclareWarCooldown));
        }

        private void RegisterDeclareWarCooldown(IFaction faction1, IFaction faction2)
        {
            _cooldownManager.UpdateLastWarTime(faction1, faction2, CampaignTime.Now);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_cooldownManager", ref _cooldownManager);
            _cooldownManager.sync();
        }
    }
}
