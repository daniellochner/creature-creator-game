// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using System.Collections.Generic;

namespace DanielLochner.Assets.CreatureCreator
{
    public class World
    {
        public Mode Mode { get; protected set; }
        public bool EnablePVE { get; protected set; }
        public bool SpawnNPC { get; protected set; }
        public string MapName { get; protected set; }
        public string MapId { get; protected set; }
        public int SpawnPoint { get; protected set; }
        public string CustomMapId { get; protected set; }
        public List<string> CustomBodyPartIds { get; protected set; }
        public List<string> CustomPatternIds { get; protected set; }

        public bool IsCustom => MapName.Equals("Custom");
    }
}