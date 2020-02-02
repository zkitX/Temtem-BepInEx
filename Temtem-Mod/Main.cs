using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using Temtem.Players;
using TemTemMod.Mods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace TemTemMod
{
    [BepInPlugin("com.zkitx.plugin.mod", "Temtem Mod", "1.0.0.0")]
    [BepInProcess("Temtem.exe")]
    public class TemTemMod : BaseUnityPlugin
    {
        public static GameObject BaseObject;
        private static TemTemMod instance;
        internal static List<string> log = new List<string>();

        UserInterface updateCallbacks = new UserInterface();
        Hacks hacks = new Hacks();

        public void Awake()
        {
            instance = this;
            Invoke(nameof(Init), 1f);
        }

        private void Init()
        {
            BaseObject = new GameObject("TemtemMod");
            DontDestroyOnLoad(BaseObject);
            BaseObject.SetActive(false);

            // Credit to shalzuth https://github.com/shalzuth/TemSharp for a efficient way to load mods
            var types = Assembly.GetExecutingAssembly().GetTypes().ToList().Where(t => t.BaseType == typeof(MonoBehaviour) && !t.IsNested);
            foreach (var type in types)
            {
                var component = (MonoBehaviour)BaseObject.AddComponent(type);
                component.enabled = false;
            }
            BaseObject.GetComponent<UserInterface>().enabled = true;
            BaseObject.SetActive(true);
            // new GameObject(typeof(UserInterface).FullName, typeof(UserInterface));
            DestroyAntiCheat();
        }

        internal static ConfigFile GetConfig()
        {
            return instance.Config;
        }

        public void Update()
        {
            try
            {
                this.TeleportRoutine();
                this.EncounterFunc();
                this.ShinyHuntRoutine();
                this.MonsterInfoRoutine();
            }
            catch (NullReferenceException)
            {
            }
        }

        private void MonsterInfoRoutine()
        {
            bool monsterToggle = updateCallbacks.istoggleMonsterInfo;
            if (monsterToggle)
            {
                BaseObject.GetComponent<MonsterInfo>().enabled = true;
            }
            if (!monsterToggle)
            {
                BaseObject.GetComponent<MonsterInfo>().enabled = false;
            }
        }

        private void TeleportRoutine()
        {
            bool teleportToggle = updateCallbacks.isToggleTP;
            if (teleportToggle)
            {
                BaseObject.GetComponent<Teleport>().enabled = true;
            }
            if (!teleportToggle)
            {
                BaseObject.GetComponent<Teleport>().enabled = false;
            }
        }

        private void EncounterFunc()
        {
            bool encounterToggle = updateCallbacks.isToggleEncounter;
            if (encounterToggle)
            {
                BaseObject.GetComponent<EncounterMod>().enabled = true;
            }
            if (!encounterToggle)
            {
                BaseObject.GetComponent<EncounterMod>().enabled = false;
            }
        }

        private void ShinyHuntRoutine()
        {
            bool shinyhuntToggle = updateCallbacks.isToggleShinyHunter;
            if (shinyhuntToggle)
            {
                BaseObject.GetComponent<ShinyHunter>().enabled = true;
            }
            if (!shinyhuntToggle)
            {
                BaseObject.GetComponent<ShinyHunter>().enabled = false;
            }

        }

        public static void DestroyAntiCheat()
        {
            DestroyImmediate(FindObjectOfType<CodeStage.AntiCheat.Detectors.ActDetectorBase>());
            DestroyImmediate(FindObjectOfType<CodeStage.AntiCheat.Detectors.InjectionDetector>());
            DestroyImmediate(FindObjectOfType<CodeStage.AntiCheat.Detectors.ObscuredCheatingDetector>());
            DestroyImmediate(FindObjectOfType<CodeStage.AntiCheat.Detectors.SpeedHackDetector>());
            DestroyImmediate(FindObjectOfType<CodeStage.AntiCheat.Detectors.TimeCheatingDetector>());
        }

        public static void Log(string owner, LogLevel level, string message)
        {
            if(log.Count > 200)
            {
                log.RemoveAt(0);
            }
            string line = "[" + owner + "]: ";
            line += message;
            log.Add(line);
        }
    }
}
