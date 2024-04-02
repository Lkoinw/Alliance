﻿using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Alliance.Common.Extensions.ToggleEntities.NetworkMessages.FromClient
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestToggleEntities : GameNetworkMessage
    {
        public string EntitiesTag { get; private set; }
        public bool Show { get; private set; }

        public RequestToggleEntities(string entitiesTag, bool show)
        {
            EntitiesTag = entitiesTag;
            Show = show;
        }

        public RequestToggleEntities()
        {
        }

        protected override bool OnRead()
        {
            bool bufferReadValid = true;
            EntitiesTag = ReadStringFromPacket(ref bufferReadValid);
            Show = ReadBoolFromPacket(ref bufferReadValid);
            return bufferReadValid;
        }

        protected override void OnWrite()
        {
            WriteStringToPacket(EntitiesTag);
            WriteBoolToPacket(Show);
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return string.Concat(Show ? "Show" : "Hide", " entities with tag ", EntitiesTag);
        }
    }
}