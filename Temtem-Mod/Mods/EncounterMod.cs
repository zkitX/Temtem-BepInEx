﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Temtem.Core;
using Temtem.World;
using Temtem.Players;
using Temtem.Network;

namespace TemTemMod.Mods
{
    public class EncounterMod : MonoBehaviour
    {
        public Dictionary<String, SpawnZoneDefinition.mjoookiqrfh> InitialZoneValues = new Dictionary<string, SpawnZoneDefinition.mjoookiqrfh>();
        void OnEnable()
        {
            var spawnZoneDefList = typeof(WildMonstersLogic).GetField<WildMonstersLogic>().GetField<HashSet<SpawnZoneDefinition>>();
            foreach (var spawnZoneDef in spawnZoneDefList)
            {
                InitialZoneValues[spawnZoneDef.GetField<Int16>("id") + spawnZoneDef.GetField<Int16>("sceneId") + spawnZoneDef.GetField<String>("battleZone")] = spawnZoneDef.GetField<SpawnZoneDefinition.mjoookiqrfh>();
                spawnZoneDef.SetField("triggerType", SpawnZoneDefinition.mjoookiqrfh.jccgopkhifm);
            }
        }
        void OnDisable()
        {
            var spawnZoneDefList = typeof(WildMonstersLogic).GetField<WildMonstersLogic>().GetField<HashSet<SpawnZoneDefinition>>();
            foreach (var spawnZoneDef in spawnZoneDefList)
            {
                var key = spawnZoneDef.GetField<Int16>("id") + spawnZoneDef.GetField<Int16>("sceneId") + spawnZoneDef.GetField<String>("battleZone");
                if (InitialZoneValues.ContainsKey(key))
                    spawnZoneDef.SetField("triggerType", InitialZoneValues[spawnZoneDef.GetField<Int16>("id") + spawnZoneDef.GetField<Int16>("sceneId") + spawnZoneDef.GetField<String>("battleZone")]);
            }
        }
    }
}
