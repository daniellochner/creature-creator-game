// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

namespace DanielLochner.Assets.CreatureCreator
{
    public class WorldMP : World
    {
        #region Properties
        public bool IsPrivate { get; private set; }
        public string JoinCode { get; private set; }
        public string Id { get; private set; }
        public string PasswordHash { get; private set; }
        public bool IsPasswordProtected { get; private set; }

        public string WorldName { get; private set; }
        public string Version { get; private set; }
        public bool AllowProfanity { get; private set; }
        public bool EnablePVP { get; private set; }

        public string HostPlayerId { get; private set; }

        public List<string> KickedPlayers { get; private set; }

        public string InstitutionId { get; private set; }

        public bool IsBeta => Version.EndsWith("-beta");
        public bool IsModded => false;
        #endregion

        #region Methods
        public WorldMP(Lobby lobby)
        {
            Id = lobby.Id;
            IsPrivate = lobby.TryGetValue<bool>("isPrivate");
            JoinCode = lobby.TryGetValue<string>("joinCode");
            PasswordHash = lobby.TryGetValue<string>("passwordHash");
            IsPasswordProtected = !string.IsNullOrEmpty(PasswordHash);

            WorldName = lobby.Name;
            MapName = lobby.TryGetValue<string>("mapName");
            Version = lobby.TryGetValue<string>("version");
            AllowProfanity = lobby.TryGetValue<bool>("allowProfanity");
            EnablePVP = lobby.TryGetValue<bool>("enablePVP");
            EnablePVE = lobby.TryGetValue<bool>("enablePVE");
            SpawnNPC = lobby.TryGetValue<bool>("spawnNPC");
            Mode = (Mode)lobby.TryGetValue<int>("mode");
            MapId = lobby.TryGetValue<string>("mapId");
            SpawnPoint = lobby.TryGetValue<int>("spawnPoint");

            HostPlayerId = lobby.TryGetValue<string>("hostPlayerId");
            KickedPlayers = new List<string>(lobby.TryGetValue("kickedPlayers", "").Split(","));

            InstitutionId = lobby.TryGetValue<string>("institutionId");
        }
        #endregion
    }
}