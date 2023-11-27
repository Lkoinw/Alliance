﻿using Alliance.Client.Extensions.GameModeMenu.Views;
using Alliance.Common.Core.Security.Extension;
using Alliance.Common.GameModes.PvC.Behaviors;
using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.GauntletUI.Mission;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;

namespace Alliance.Client.Extensions.ExNativeUI.EscapeMenu.Views
{
    [OverrideView(typeof(MissionGauntletEscapeMenuBase))]
    public class EscapeMenuView : MissionGauntletEscapeMenuBase
    {
        public EscapeMenuView(string gameType)
            : base("MultiplayerEscapeMenu")
        {
            _gameType = gameType;
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            _missionOptionsComponent = Mission.GetMissionBehavior<MissionOptionsComponent>();
            _missionLobbyEquipmentNetworkComponent = Mission.GetMissionBehavior<MissionLobbyEquipmentNetworkComponent>();
            _missionLobbyComponent = Mission.GetMissionBehavior<MissionLobbyComponent>();
            _missionTeamSelectComponent = Mission.GetMissionBehavior<MultiplayerTeamSelectComponent>();
            _pvcMissionTeamSelectComponent = Mission.GetMissionBehavior<PvCTeamSelectBehavior>();
            _gameModeClient = Mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
            TextObject textObject = GameTexts.FindText("str_multiplayer_game_type", _gameType);
            DataSource = new EscapeMenuVM(null, textObject);
        }

        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            DataSource.Tick(dt);
        }

        public override bool OnEscape()
        {
            bool flag = base.OnEscape();
            if (IsActive)
            {
                if (_gameModeClient.IsGameModeUsingAllowTroopChange)
                {
                    _changeTroopItem.IsDisabled = !_gameModeClient.CanRequestTroopChange();
                }
                if (_gameModeClient.IsGameModeUsingAllowCultureChange)
                {
                    _changeCultureItem.IsDisabled = !_gameModeClient.CanRequestCultureChange();
                }
            }
            return flag;
        }

        protected override List<EscapeMenuItemVM> GetEscapeMenuItems()
        {
            List<EscapeMenuItemVM> list = new List<EscapeMenuItemVM>();
            string gameType = MultiplayerOptions.OptionType.GameType.GetStrValue();

            // Return to the game
            list.Add(new EscapeMenuItemVM(new TextObject("{=e139gKZc}Return to the Game", null), delegate (object o)
            {
                OnEscapeMenuToggled(false);
            }, null, () => new Tuple<bool, TextObject>(false, TextObject.Empty), false));

            // Propositions de vote
            if (GameNetwork.MyPeer.IsAdmin()) // TODO : remove this admin check once the poll feature is completed
            {
                list.Add(new EscapeMenuItemVM(new TextObject("{=lobby_vote_esc_menu}Propositions de vote", null), delegate (object o)
                {
                    OnEscapeMenuToggled(false);
                    Mission.Current.GetMissionBehavior<GameModeMenuView>()?.OpenMenu();
                }, null, () => new Tuple<bool, TextObject>(false, TextObject.Empty), false));
            }

            // Options
            list.Add(new EscapeMenuItemVM(new TextObject("{=NqarFr4P}Options", null), delegate (object o)
            {
                OnEscapeMenuToggled(false);
                MissionOptionsComponent missionOptionsComponent = _missionOptionsComponent;
                if (missionOptionsComponent == null)
                {
                    return;
                }
                missionOptionsComponent.OnAddOptionsUIHandler();
            }, null, () => new Tuple<bool, TextObject>(false, TextObject.Empty), false));

            // Change Team
            if (gameType != "Scenario" || GameNetwork.MyPeer.IsAdmin())
            {
                // TODO : remove pvc custom component, shouldn't have to dup code here
                if (_pvcMissionTeamSelectComponent != null && _pvcMissionTeamSelectComponent.TeamSelectionEnabled)
                {
                    list.Add(new EscapeMenuItemVM(new TextObject("{=2SEofGth}Change Team", null), delegate (object o)
                    {
                        OnEscapeMenuToggled(false);
                        _pvcMissionTeamSelectComponent.SelectTeam();
                    }, null, () => new Tuple<bool, TextObject>(false, TextObject.Empty), false));
                }
                else if (_missionTeamSelectComponent != null && _missionTeamSelectComponent.TeamSelectionEnabled)
                {
                    list.Add(new EscapeMenuItemVM(new TextObject("{=2SEofGth}Change Team", null), delegate (object o)
                    {
                        OnEscapeMenuToggled(false);
                        _missionTeamSelectComponent.SelectTeam();
                    }, null, () => new Tuple<bool, TextObject>(false, TextObject.Empty), false));
                }
            }

            // Change Culture
            //if (_gameModeClient.IsGameModeUsingAllowCultureChange)
            //if (GameNetwork.MyPeer.IsAdmin())
            //{
            //    _changeCultureItem = new EscapeMenuItemVM(new TextObject("{=aGGq9lJT}Change Culture", null), delegate (object o)
            //    {
            //        OnEscapeMenuToggled(false);
            //        _missionLobbyComponent.RequestCultureSelection();
            //    }, null, () => new Tuple<bool, TextObject>(false, TextObject.Empty), false);
            //    list.Add(_changeCultureItem);
            //}

            // Change Troop
            //if (_gameModeClient.IsGameModeUsingAllowTroopChange)
            //if (GameNetwork.MyPeer.IsAdmin())
            //{
            //    _changeTroopItem = new EscapeMenuItemVM(new TextObject("{=Yza0JYJt}Change Troop", null), delegate (object o)
            //    {
            //        OnEscapeMenuToggled(false);
            //        _missionLobbyComponent.RequestTroopSelection();
            //    }, null, () => new Tuple<bool, TextObject>(false, TextObject.Empty), false);
            //    list.Add(_changeTroopItem);
            //}

            // Quit
            list.Add(new EscapeMenuItemVM(new TextObject("{=InGwtrWt}Quit", null), delegate (object o)
            {
                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=InGwtrWt}Quit", null).ToString(), new TextObject("{=lxq6SaQn}Are you sure want to quit?", null).ToString(), true, true, GameTexts.FindText("str_yes", null).ToString(), GameTexts.FindText("str_no", null).ToString(), delegate
                {
                    LobbyClient gameClient = NetworkMain.GameClient;
                    if (gameClient.CurrentState == LobbyClient.State.InCustomGame)
                    {
                        gameClient.QuitFromCustomGame();
                        return;
                    }
                    if (gameClient.CurrentState == LobbyClient.State.HostingCustomGame)
                    {
                        gameClient.EndCustomGame();
                        return;
                    }
                    gameClient.QuitFromMatchmakerGame();
                }, null, "", 0f, null, null, null), false, false);
            }, null, () => new Tuple<bool, TextObject>(false, TextObject.Empty), false));
            return list;
        }

        private MissionOptionsComponent _missionOptionsComponent;

        private MissionLobbyEquipmentNetworkComponent _missionLobbyEquipmentNetworkComponent;

        private MissionLobbyComponent _missionLobbyComponent;

        private MultiplayerTeamSelectComponent _missionTeamSelectComponent;
        private PvCTeamSelectBehavior _pvcMissionTeamSelectComponent;

        private MissionMultiplayerGameModeBaseClient _gameModeClient;

        private readonly string _gameType;

        private EscapeMenuItemVM _changeTroopItem;

        private EscapeMenuItemVM _changeCultureItem;
    }
}
