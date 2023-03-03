using System;
using System.IO;
using System.Media;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using Newtonsoft.Json;

namespace TOIRemoveGenderRestrictions
{
    [HarmonyPatch(typeof(ConfMgr), nameof(ConfMgr.Init), new Type[]{})]
    public class ConfMgrPatch
    {
        static void Postfix()
        {
            MelonLogger.Msg("ConfMgr Inited");
            var config = MelonPreferences.GetCategory("RemoveGenderRestrictions Conf Patches");
            if (config.GetEntry<bool>("PatchRoleRelations").Value)
            {
                var roleRelation = g.conf.roleRelation;
                foreach (var item in roleRelation._allConfList)
                {
                    if (item.gender != 3) continue;
                    item.gender = 0;
                }
                MelonLogger.Msg("Patched ConfRoleRelation gender lock");
            }

            if (config.GetEntry<bool>("PatchNpcActionFilter").Value)
            {
                var npcActionFilter = g.conf.npcActionFilter;
                npcActionFilter.GetItem(14).gender = 0;
                MelonLogger.Msg("Patched ConfNpcActionFilter gender lock");
            }

            if (config.GetEntry<bool>("PatchNpcSocialRelation").Value)
            {
                var npcSocialRelation = g.conf.npcSocialRelation;
                npcSocialRelation.GetItem(20).value = 101;
                MelonLogger.Msg("Patched NpcSocialRelation age discrimination");
            }

            var npcSpecial = g.conf.npcSpecial;
            var npcSpecialItem = npcSpecial.GetItem(1001);
            npcSpecialItem.gender = config.GetEntry<int>("PatchTrueLove").Value;
            MelonLogger.Msg($"Patched ConfNpcSpecial True Love entry to gender {npcSpecialItem.gender}");
            
            //DumpConf(g.conf.npcAction1044._allConfList, "NpcAction1044");
            //DumpConf(g.conf.roleAttributeLimit._allConfList, "RoleAttributeLimit");
            //DumpConf(g.conf.roleAttributePower._allConfList, "RoleAttributePower");
            //DumpConf(g.conf.roleAttributeCoefficient._allConfList, "RoleAttributeCoefficient");
        }

        private static void DumpConf<T>(List<T> conf, string fileName)
        {
            var tempList = new System.Collections.Generic.List<T>();
            
            foreach(var item in conf)
                tempList.Add(item);

            using (StreamWriter file = File.CreateText($"json/{fileName}.json"))
            {
                file.Write(JsonConvert.SerializeObject(tempList, Formatting.Indented));
            }
        }
    }
}