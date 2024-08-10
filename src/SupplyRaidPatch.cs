using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SupplyRaid;
using FistVR;
using HarmonyLib;
using UnityEngine;

namespace Ammo_Pouch
{
    public class SupplyRaidPatch
    {
        [HarmonyPatch(typeof(SR_MagazineDuplicator), "Button_Duplicate")]
        [HarmonyPrefix]
        static bool pouchDuplicate(SR_MagazineDuplicator __instance, bool ___canDuplicate, FVRFireArmMagazine ___m_detectedMag, int ___costDuplicate)
        {
            if (___m_detectedMag is AmmoPouch && ___canDuplicate && ___m_detectedMag != null && ___m_detectedMag.gameObject != null && SR_Manager.EnoughPoints(___costDuplicate))
            {
                AmmoPouch pouch =  ___m_detectedMag as AmmoPouch;
                if (pouch.currentCapacity < pouch.totalCapacity && pouch.container != AmmoPouch.ContainerType.Round)
                {
                    SR_Manager.PlayConfirmSFX();
                    SR_Manager.SpendPoints(___costDuplicate);
                    pouch.duplicateMag();
                } else
                {
                    SR_Manager.PlayFailSFX();
                }
                return false;
                
            }

            return true;
        }


        [HarmonyPatch(typeof(SR_Global), "SpawnLoot")]
        [HarmonyPostfix]
        static void spawnPouch(SR_Global __instance, SR_ItemCategory itemCategory, Transform[] spawns)
        {
            if(itemCategory.type == LootTable.LootTableType.Firearm && Ammo_Pouch_Scripts.AmmoPouch_CanSpawnInSR.Value)
            {
                GameObject obj = UnityEngine.Object.Instantiate<GameObject>(IM.OD["small_ammo_pouch"].GetGameObject(), spawns[0]);
            }
        }
    }
}
