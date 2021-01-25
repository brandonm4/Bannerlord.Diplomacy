﻿using Diplomacy.DiplomaticAction.Usurp;
using Diplomacy.GauntletInterfaces;
using Diplomacy.GrantFief;
using System;
using System.ComponentModel;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomClan;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine.Screens;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModel
{
    class KingdomClanVMExtensionVM : KingdomClanVM, INotifyPropertyChanged, ICloseableVM
    {
        private bool _canGrantFiefToClan;
        private GrantFiefInterface _grantFiefInterface;
        private HintViewModel _grantFiefHint;

        private Action _executeExpel;
        private Action _executeSupport;
        private bool _canUsurpThrone;
        private bool _showUsurpThrone;
        private int _usurpThroneInfluenceCost;
        private string _usurpThroneExplanationText;
        private HintViewModel _usurpThroneHint;

        public KingdomClanVMExtensionVM(Action<TaleWorlds.CampaignSystem.Election.KingdomDecision> forceDecide) : base(forceDecide)
        {
            _executeExpel = () => typeof(KingdomClanVM).GetMethod("ExecuteExpelCurrentClan", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, new object[] { });
            _executeSupport = () => typeof(KingdomClanVM).GetMethod("ExecuteSupport", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, new object[] { });

            Events.FiefGranted.AddNonSerializedListener(this, RefreshCanGrantFief);
            _grantFiefInterface = new GrantFiefInterface();
            GrantFiefActionName = new TextObject("{=LpoyhORp}Grant Fief").ToString();
            GrantFiefExplanationText = new TextObject("{=98hwXUTp}Grant fiefs to clans in your kingdom").ToString();
            DonateGoldActionName = new TextObject("{=Gzq6VHPt}Donate Gold").ToString();
            DonateGoldExplanationText = new TextObject("{=7QvXkcxH}Donate gold to clans in your kingdom").ToString();
            UsurpThroneActionName = new TextObject("{=N7goPgiq}Usurp Throne").ToString();
            PropertyChanged += new PropertyChangedEventHandler(OnPropertyChanged);
            PropertyChangedWithValue += new PropertyChangedWithValueEventHandler(OnPropertyChangedWithValue);
            RefreshCanGrantFief();
            RefreshCanUsurpThrone();
        }

        public void OnClose()
        {
            Events.RemoveListeners(this);
        }

        private void ExecuteExpelCurrentClan()
        {
            _executeExpel();
        }

        private void ExecuteSupport()
        {
            _executeSupport();
        }

        // e1.4.2 compatible
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentSelectedClan")
            {
                RefreshCanGrantFief();
                RefreshCanUsurpThrone();
            }
        }

        // e1.4.3 compatible
        private void OnPropertyChangedWithValue(object sender, PropertyChangedWithValueEventArgs e)
        {
            if (e.PropertyName == "CurrentSelectedClan")
            {
                RefreshCanGrantFief();
                RefreshCanUsurpThrone();
            }
        }

        private void RefreshCanUsurpThrone()
        {
            ShowUsurpThrone = CurrentSelectedClan.Clan == Clan.PlayerClan;
            CanUsurpThrone = UsurpKingdomAction.CanUsurp(Clan.PlayerClan, out var errorMessage);
            UsurpThroneHint = errorMessage is not null ? new HintViewModel(errorMessage) : new HintViewModel();
            UsurpKingdomAction.GetClanSupport(Clan.PlayerClan, out var supportingClanTiers, out var opposingClanTiers);

            var textObject = new TextObject("{=WVe7QwhW}Usurp the throne of this kingdom\nClan Support: {SUPPORTING_TIERS} / {OPPOSING_TIERS}");
            textObject.SetTextVariable("SUPPORTING_TIERS", supportingClanTiers);
            textObject.SetTextVariable("OPPOSING_TIERS", opposingClanTiers + 1);
            UsurpThroneExplanationText = textObject.ToString();
            UsurpThroneInfluenceCost = (int)UsurpKingdomAction.GetUsurpInfluenceCost(Clan.PlayerClan);
        }

        public void UsurpThrone()
        {
            UsurpKingdomAction.Apply(Clan.PlayerClan);
            RefreshClan();
        }

        public void GrantFief()
        {
            _grantFiefInterface.ShowFiefInterface(ScreenManager.TopScreen, CurrentSelectedClan.Clan.Leader);
        }

        private void DonateGold()
        {
            new DonateGoldInterface().ShowInterface(ScreenManager.TopScreen, CurrentSelectedClan.Clan);
        }


        private void RefreshCanGrantFief(Town town)
        {
            RefreshClan();
            RefreshCanGrantFief();
        }

        private void RefreshCanGrantFief()
        {
            CanGrantFiefToClan = GrantFiefAction.CanGrantFief(CurrentSelectedClan.Clan, out var hint);
            GrantFiefHint = CanGrantFiefToClan ? new HintViewModel() : new HintViewModel(hint, null);
        }

        [DataSourceProperty]
        public bool CanGrantFiefToClan
        {
            get { return _canGrantFiefToClan; }

            set
            {
                if (value != _canGrantFiefToClan)
                {
                    _canGrantFiefToClan = value;
                    OnPropertyChanged("CanGrantFiefToClan");
                }
            }
        }

        [DataSourceProperty]
        public string GrantFiefActionName { get; }

        [DataSourceProperty]
        public HintViewModel GrantFiefHint
        {
            get
            {
                return _grantFiefHint;
            }
            set
            {
                if (value != _grantFiefHint)
                {
                    _grantFiefHint = value;
                    OnPropertyChanged("GrantFiefHint");
                }
            }
        }

        [DataSourceProperty]
        public string GrantFiefExplanationText { get; }
        [DataSourceProperty]
        public string UsurpThroneActionName { get; }
        [DataSourceProperty]
        public string UsurpThroneExplanationText
        {
            get { return _usurpThroneExplanationText; }

            set
            {
                if (value != _usurpThroneExplanationText)
                {
                    _usurpThroneExplanationText = value;
                    OnPropertyChanged("UsurpThroneExplanationText");
                }
            }

        }
        [DataSourceProperty]
        public string DonateGoldActionName { get; }
        [DataSourceProperty]
        public string DonateGoldExplanationText { get; }

        [DataSourceProperty]
        public bool ShowUsurpThrone
        {
            get { return _showUsurpThrone; }

            set
            {
                if (value != _showUsurpThrone)
                {
                    _showUsurpThrone = value;
                    OnPropertyChanged("ShowUsurpThrone");
                }
            }

        }

        [DataSourceProperty]
        public bool CanUsurpThrone
        {
            get { return _canUsurpThrone; }

            set
            {
                if (value != _canUsurpThrone)
                {
                    _canUsurpThrone = value;
                    OnPropertyChanged("CanUsurpThrone");
                }
            }
        }

        [DataSourceProperty]
        public int UsurpThroneInfluenceCost
        {
            get { return _usurpThroneInfluenceCost; }

            set
            {
                if (value != _usurpThroneInfluenceCost)
                {
                    _usurpThroneInfluenceCost = value;
                    OnPropertyChanged("UsurpThroneInfluenceCost");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel UsurpThroneHint
        {
            get { return _usurpThroneHint; }

            set
            {
                if (value != _usurpThroneHint)
                {
                    _usurpThroneHint = value;
                    OnPropertyChanged("UsurpThroneHint");
                }
            }
        }
    }
}