using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;
using FistVR;
using System;


namespace Ammo_Pouch
{

    public class AmmoPouch_Hooks
    {

        
        public class AmmoReloader
        {

            [HarmonyPatch(typeof(FVRFireArmMagazine), "ReloadMagWithType")]
            [HarmonyPrefix]
            static bool ReloadWithMagFix(FVRFireArmMagazine __instance, FireArmRoundClass rClass)
            {
                if(__instance is AmmoPouch)
                {
                    ((AmmoPouch)__instance).refillWithType(rClass);
                    return false;
                }
                
                return true;
            }
        }

        public class PouchSpawning
        {
            [HarmonyPatch(typeof(TNH_ObjectConstructor), "ButtonClicked")]
            [HarmonyPrefix]
            static void SpawnObjectFix(TNH_ObjectConstructor __instance, int i)
            {

                if(i == 3 && __instance.State == TNH_ObjectConstructor.ConstructorState.Confirm)
                {
                    Console.WriteLine("i = 3 and Confirm state");
                    ObjectTable table = __instance.M.GetObjectTable(__instance.m_poolEntries[__instance.m_selectedEntry].TableDef);

                    if(table.GetRandomObject().Category == FVRObject.ObjectCategory.Firearm && Ammo_Pouch_Scripts.AmmoPouch_CanSpawnInTnH.Value)
                    {
                        Console.WriteLine("Is firearm and valid");
                        int cost = __instance.m_poolEntries[__instance.m_selectedEntry].GetCost(__instance.M.EquipmentMode) + __instance.m_poolAddedCost[__instance.m_selectedEntry];
                        if (__instance.M.GetNumTokens() >= cost)
                        {
                            Console.WriteLine("Enough cost, attempting to spawn");
                            GameObject obj = UnityEngine.Object.Instantiate<GameObject>(IM.OD["small_ammo_pouch"].GetGameObject(), __instance.SpawnPoint_Mag);
                            __instance.M.AddObjectToTrackedList(obj);
                        }
                    }
                }
            }


            [HarmonyPatch(typeof(TNH_SupplyPoint), "ConfigureAtBeginning")]
            [HarmonyPostfix]
            static void SpawnInitPouch(TNH_SupplyPoint __instance)
            {
                if (Ammo_Pouch_Scripts.AmmoPouch_CanSpawnInTnH.Value)
                {
                    GameObject obj = UnityEngine.Object.Instantiate<GameObject>(IM.OD["small_ammo_pouch"].GetGameObject(), __instance.SpawnPoints_SmallItem[0]);
                    __instance.M.AddObjectToTrackedList(obj);
                }
        
            }

        }
    }
}