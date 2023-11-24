﻿using Alliance.Common.Extensions;
using Alliance.Common.Extensions.TroopSpawner.Models;
using Alliance.Common.Extensions.TroopSpawner.NetworkMessages.FromServer;
using TaleWorlds.MountAndBlade;

namespace Alliance.Client.Extensions.TroopSpawner.Handlers
{
    public class AgentsInfoHandler : IHandlerRegister
    {
        public void Register(GameNetwork.NetworkMessageHandlerRegisterer reg)
        {
            reg.Register<AgentsInfoMessage>(HandleServerEventAgentsInfoMessage);
        }

        public void HandleServerEventAgentsInfoMessage(AgentsInfoMessage message)
        {
            switch (message.DataType)
            {
                case DataType.All:
                    AgentsInfoModel.Instance.AddAgentInfo(message.Agent, message.Difficulty, message.Experience, message.Lives);
                    break;
                case DataType.Difficulty:
                    AgentsInfoModel.Instance.UpdateAgentDifficulty(message.Agent, message.Difficulty);
                    break;
                case DataType.Experience:
                    AgentsInfoModel.Instance.UpdateAgentExperience(message.Agent, message.Experience);
                    break;
                case DataType.Lives:
                    AgentsInfoModel.Instance.UpdateAgentLives(message.Agent, message.Lives);
                    break;
            }
            message.Agent.UpdateAgentProperties();
        }
    }
}