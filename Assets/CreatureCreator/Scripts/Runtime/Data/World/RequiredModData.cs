using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    [Serializable]
    public class RequiredModData
    {
        public ulong id;
        public uint version;

        public RequiredModData()
        {
        }
        public RequiredModData(ulong id)
        {
            this.id = id;
        }
    }
}