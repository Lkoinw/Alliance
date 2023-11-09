﻿using Alliance.Common.Core.Security.Extension;
using Alliance.Common.Extensions.SAE.Models;
using Alliance.Common.Extensions.SAE.NetworkMessages.FromClient;
using Alliance.Common.Extensions.SAE.NetworkMessages.FromServer;
using Alliance.Server.Extensions.SAE.Behaviors;
using Alliance.Server.Extensions.SAE.Models;
using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using static Alliance.Common.Utilities.Logger;

namespace Alliance.Server.Extensions.SAE.Handlers
{
    /// <summary>
    /// This handler will handle message send by a client to the server
    /// So this code will only be executed by server
    /// </summary>
    public class SaeCreateMarkerHandler
    {
        SaeBehavior saeBehavior => Mission.Current.GetMissionBehavior<SaeBehavior>();

        public SaeCreateMarkerHandler() { }

        public bool OnSaeCreateMarkerMessageReceived(NetworkCommunicator peer, SaeCreateMarkerNetworkClientMessage message)
        {
            if (saeBehavior == null)
            {
                Debug.Print("Server: SaeBehavior is NULL !", 0, Debug.DebugColor.Red);
                return false;
            }
            else
            {
                Debug.Print("Server: Spawning a strategic area", 0, Debug.DebugColor.Blue);
                List<SaeMarkerServerEntity> markerlist = saeBehavior.AddMarkersToTeam(message.markersPosition, peer.ControlledAgent.Team);

                markerlist.ForEach(m => InitStrategicAreaLogic(peer, m.StrategicArcherPointEntity));

                SendMarkersListToAllPeersOfSameTeam(peer, ConvertServerEntityToIdAndPos(markerlist));

                //DebugInfoCount(peer);

                return true;
            }
        }

        public static List<SaeMarkerWithIdAndPos> ConvertServerEntityToIdAndPos(List<SaeMarkerServerEntity> list)
        {
            List<SaeMarkerWithIdAndPos> myList = new List<SaeMarkerWithIdAndPos>();
            list.ForEach(m => myList.Add(new SaeMarkerWithIdAndPos(m.Id, m.StrategicArcherPointEntity.GetGlobalFrame())));

            return myList;
        }

        public static void SendMarkersListToAllPeersOfSameTeam(NetworkCommunicator peer, List<SaeMarkerWithIdAndPos> markerlist)
        {
            if (markerlist.Count > 0)
            {

                int groupSize = 10;
                int totalElements = markerlist.Count;

                for (int i = 0; i < totalElements; i += groupSize)
                {
                    int startIndex = i;
                    int endIndex = Math.Min(i + groupSize, totalElements);

                    List<SaeMarkerWithIdAndPos> group = markerlist.GetRange(startIndex, endIndex - startIndex);

                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new SaeCreateMarkersNetworkServerMessage(group));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.ExcludeOtherTeamPlayers, peer);
                }
            }
        }

        public static void InitStrategicAreaLogic(NetworkCommunicator peer, GameEntity gameEntity)
        {
            StrategicArea area = gameEntity.GetFirstScriptOfType<StrategicArea>();

            area.InitializeAutogenerated(1, 1, peer.ControlledAgent.Team.Side);

            if (peer.ControlledAgent.Team.TeamAI == null)
            {
                //Create TeamAI
                TeamAIGeneral2 teamAi = new(Mission.Current, peer.ControlledAgent.Team);
                TacticSergeantMPBotTactic2 tacticDefensive = new(peer.ControlledAgent.Team);

                teamAi.AddTacticOption(tacticDefensive);
                teamAi.OnTacticAppliedForFirstTime();

                foreach (Formation formation in peer.ControlledAgent.Team.FormationsIncludingEmpty)
                {
                    if (formation.CountOfUnits > 0 && !(formation.CountOfUnits == 1 && formation.GetFirstUnit() == peer.ControlledAgent))
                    {
                        Debug.Print("Call of OnUnitAddedToFormationForTheFirstTime for formation = " + formation.Index, 0, Debug.DebugColor.Green);
                        teamAi.OnUnitAddedToFormationForTheFirstTime(formation);
                    }
                }
                peer.ControlledAgent.Team.AddTeamAI(teamAi);
            }

            //Add strategicArea
            Debug.Print("Strategic AREA : " + area.Id + " has been added to TeamIA : " + peer.ControlledAgent.Team.TeamAI, 0, Debug.DebugColor.Green);
            peer.ControlledAgent.Team.TeamAI.AddStrategicArea(area);
        }

        private void DebugInfoCount(NetworkCommunicator peer)
        {
            Debug.Print("--- Marker DEBUG SERVER SIDE ---");

            int markerWithStrategicScript = 0;
            int markerWithlogicalMarkerTag = 0;

            List<GameEntity> gameEntities = new List<GameEntity>();
            Mission.Current.Scene.GetEntities(ref gameEntities);
            foreach (GameEntity gameEntity in gameEntities)
            {
                if (gameEntity.HasScriptOfType<StrategicArea>())
                {
                    markerWithStrategicScript += 1;
                }
                if (gameEntity.HasTag(SaeConstants.LOGICAL_STR_POS_TAG))
                {
                    markerWithlogicalMarkerTag += 1;
                }
            }

            Debug.Print("markerWithStrategicScript : " + markerWithStrategicScript);
            Debug.Print("markerWithlogicalMarkerTag : " + markerWithlogicalMarkerTag);
            Debug.Print("MISSION BEHAVIOR NUMBER OF ELEMENTS FOR THIS TEAM : " + saeBehavior.GetMarkerListDependingOnTeam((int)peer.ControlledAgent.Team.Side).Count);
        }

        public bool OnCrouchMessageReceived(NetworkCommunicator peer, SaeCrouchNetworkClientMessage message)
        {
            Log($"{peer.UserName} requesting crouch for {message.Team.Side} - Formation {message.FormationIndex}", LogLevel.Information);
            Formation formation = message.Team.GetFormation((FormationClass)message.FormationIndex);

            // Check if peer is authorized to request animation
            if (peer.ControlledAgent != formation.PlayerOwner && !peer.IsAdmin()) return false;

            foreach (Agent agent in formation.GetUnitsWithoutDetachedOnes())
            {
                agent.SetCrouchMode(message.CrouchMode);
            }

            return true;
        }

        private class TacticSergeantMPBotTactic2 : TacticComponent
        {
            public TacticSergeantMPBotTactic2(Team team)
                : base(team)
            {
            }

            protected override void TickOccasionally()
            {
                foreach (Formation item in FormationsIncludingEmpty)
                {
                    if (item.CountOfUnits > 0)
                    {
                        Debug.Print("ResetBehaviorWeights for formation : " + item.Index, 0, Debug.DebugColor.Green);
                        item.AI.ResetBehaviorWeights();
                    }
                }
            }
        }

        private class TeamAIGeneral2 : TeamAIComponent
        {

            public TeamAIGeneral2(Mission currentMission, Team currentTeam, float thinkTimerTime = 10f, float applyTimerTime = 1f)
                : base(currentMission, currentTeam, thinkTimerTime, applyTimerTime)
            {
            }

            public override void OnUnitAddedToFormationForTheFirstTime(Formation formation)
            {
            }

            private void UpdateVariables()
            {
                TeamQuerySystem querySystem = Team.QuerySystem;
                Vec2 averagePosition = querySystem.AveragePosition;
                foreach (Agent agent in Mission.Agents)
                {
                    if (!agent.IsMount && agent.Team.IsValid && agent.Team.IsEnemyOf(Team))
                    {
                        float num = agent.Position.DistanceSquared(new Vec3(averagePosition.x, averagePosition.y));
                    }
                }
            }
        }
    }
}
