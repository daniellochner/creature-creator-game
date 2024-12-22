// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

namespace DanielLochner.Assets.CreatureCreator
{
    public class WorldSP : World
    {
        public bool Unlimited { get; private set; }

        public WorldSP(Map map, Mode mode, bool spawnNPC, bool enablePVE, bool unlimited, string customMapId = "")
        {
            MapName = map.ToString();
            Mode = mode;
            SpawnNPC = spawnNPC;
            EnablePVE = enablePVE;
            Unlimited = unlimited;
            CustomMapId = customMapId;
        }
    }
}