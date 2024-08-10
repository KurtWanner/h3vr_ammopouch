using BepInEx;
using BepInEx.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using HarmonyLib;
using BepInEx.Configuration;
using System;

namespace Ammo_Pouch
{
    [BepInAutoPlugin]
    [BepInProcess("h3vr.exe")]
    public partial class Ammo_Pouch_Scripts : BaseUnityPlugin
    {

        // Config entries
        public static ConfigEntry<bool> AmmoPouch_CanSpawnInTnH;
        public static ConfigEntry<bool> AmmoPouch_CanSpawnInSR;

        private void Awake()
        {

            AmmoPouch_CanSpawnInTnH = Config.Bind("Ammo Pouch", "Can Spawn in TnH?", true, "If true, will spawn ammo pouches with firearms.");
            AmmoPouch_CanSpawnInSR = Config.Bind("Ammo Pouch", "Can Spawn in Supply Raid?", true, "If true, will spawn ammo pouches with firearms.");
            Logger = base.Logger;

            Harmony.CreateAndPatchAll(typeof(AmmoPouch_Hooks.AmmoReloader));
            Harmony.CreateAndPatchAll(typeof(AmmoPouch_Hooks.PouchSpawning));

            try
            {
                Harmony.CreateAndPatchAll(typeof(SupplyRaidPatch));
            }
            catch 
            {
                Logger.LogInfo("Supply raid not found.");
            }

        }
        
        internal new static ManualLogSource Logger { get; private set; }
    }
}
