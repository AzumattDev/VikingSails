using System;
using HarmonyLib;

namespace VikingSails.Patches
{
    [HarmonyPatch(typeof(Ship),nameof(Ship.Awake))]
    static class ShipAwakePatch
    {
        static void Postfix(Ship __instance)
        {
            if(!__instance.gameObject.GetComponent<VikingShipURL>())
                __instance.gameObject.AddComponent<VikingShipURL>();
        }
    }   
}