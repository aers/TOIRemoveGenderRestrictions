using System;
using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using MelonLoader;

namespace TOIRemoveGenderRestrictions
{
    public class RemoveGenderRestrictionsMod : MelonMod
    {
        private MelonPreferences_Category _configConf;
        private MelonPreferences_Category _configAsmPatch;
        private MelonPreferences_Category _configOptional;

        public override void OnApplicationLateStart()
        {
            LoggerInstance.Msg("Loading config");
            LoadConfig();
            CommitMemoryPatches();
            LoggerInstance.Msg("mod loaded");
        }

        private void LoadConfig()
        {
            _configConf = MelonPreferences.CreateCategory("RemoveGenderRestrictions Conf Patches");
            _configConf.CreateEntry("PatchRoleRelations", true,
                description: "Allow selecting Partner and Spouse roles from the Partner menu.");
            _configConf.CreateEntry("PatchNpcActionFilter", true,
                description: "Allow NPCs to request actions.");
            _configConf.CreateEntry("PatchNpcSocialRelation", true,
                description: "Allow relationships to generate if the woman is older than the man.");
            _configConf.CreateEntry("PatchTrueLove", 4,
                description:
                "Change how True Love destiny works. 0 = random, 1 = male, 2 = female, 3 = same sex, 4 (DEFAULT) = opposite sex");

            _configAsmPatch = MelonPreferences.CreateCategory("RemoveGenderRestriction ASM Patches");
            _configAsmPatch.CreateEntry("PatchInitMeum", true, description: "Allows selecting Invite in NPC window.");
            _configAsmPatch.CreateEntry("PatchCreateMeumItem", true,
                description: "Allows selecting Partner and Dual Cultivation in NPC window.");
            _configAsmPatch.CreateEntry("PatchIsCreate", true,
                description: "Allows selecting Partner in the Partner window.");
            _configAsmPatch.CreateEntry("PatchScoreForceMin", true,
                description: "Stops forcing minimum relationship score on partner/lover related checks.");
            _configAsmPatch.CreateEntry("PatchGetRelationUnits", true,
                description: "Allow marriage when the game generates potential relations between NPCs.");
            _configAsmPatch.CreateEntry("PatchVariousChildNameChecks", true,
                description: "Patches checks for a male parent when assigning child name.");

            _configOptional = MelonPreferences.CreateCategory("RemoveGenderRestriction Optional");
            _configOptional.CreateEntry("SingleGenderWorldGeneration", 0,
                description:
                "Change how random NPC gender is generated. 0 (DEFAULT) = random, 1 = male, 2 = female. Only affects randomly generated NPCs.");
        }

        private void CommitMemoryPatches()
        {
            if (_configAsmPatch.GetEntry<bool>("PatchInitMeum").Value)
                // NOP a sex equality -> jmp out check
                if (!AsmPatch.ApplyPatch(
                        "PatchInitMeum",
                        typeof(UINPCInfoUnitInfo),
                        "NativeMethodInfoPtr_InitMeum_Private_Void_0",
                        "0F 84 ?? ?? ?? ?? 33 C9 E8 ?? ?? ?? ??",
                        0x3000,
                        new byte[] {0x90, 0x90, 0x90, 0x90, 0x90, 0x90}))
                {
                    LoggerInstance.Error($"PatchInitMeum failed.");
                }

            if (_configAsmPatch.GetEntry<bool>("PatchCreateMeumItem").Value)
            {
                // NOP a sex equality -> jmp out check
                if (!AsmPatch.ApplyPatch(
                        "PatchCreateMeumItem_1",
                        typeof(UINPCInfoUnitInfo),
                        "NativeMethodInfoPtr_CreateMeumItem_Private_Void_List_1_DataStruct_6_String_Action_ReturnAction_1_String_GameObject_GameObject_Action_1_GameObject_0",
                        "0F 84 ?? ?? ?? ?? 33 C9 E8 ?? ?? ?? ??",
                        0x1500,
                        new byte[] {0x90, 0x90, 0x90, 0x90, 0x90, 0x90}))
                {
                    LoggerInstance.Error($"PatchCreateMeumItem_1 failed.");
                }
                // NOP a sex equality -> jmp out check
                if (!AsmPatch.ApplyPatch(
                        "PatchCreateMeumItem_2",
                        typeof(UINPCInfoUnitInfo),
                        "NativeMethodInfoPtr_CreateMeumItem_Private_Void_List_1_DataStruct_6_String_Action_ReturnAction_1_String_GameObject_GameObject_Action_1_GameObject_0",
                        "0F 84 ?? ?? ?? ?? 33 C9 89 75 10",
                        0x1500,
                        new byte[] {0x90, 0x90, 0x90, 0x90, 0x90, 0x90}))
                {
                    LoggerInstance.Error($"PatchCreateMeumItem_2 failed.");
                }
            }
            
            if (_configAsmPatch.GetEntry<bool>("PatchIsCreate").Value)
                // jmp over sex check
                if (!AsmPatch.ApplyPatch(
                        "PatchIsCreate",
                        typeof(UnitActionRelationSet),
                        "NativeMethodInfoPtr_IsCreate_Public_Virtual_Int32_Boolean_0",
                        "77 ?? 48 8B 43 10 48 85 C0",
                        0x500,
                        new byte[] {0xEB}))
                {
                    LoggerInstance.Error($"PatchIsCreate failed.");
                }

            if (_configAsmPatch.GetEntry<bool>("PatchScoreForceMin").Value)
                // jmp over sex check
                if (!AsmPatch.ApplyPatch(
                        "PatchScoreForceMin",
                        typeof(FormulaTool.Relation),
                        "NativeMethodInfoPtr_ScoreForceMin_Public_Static_Int32_ConfNpcPartFittingItem_WorldUnitBase_WorldUnitBase_0",
                        "77 ?? 48 8B 47 18 48 85 C0",
                        0x400,
                        new byte[] {0xEB}))
                {
                    LoggerInstance.Error($"PatchScoreForceMin failed.");
                }
            if (_configAsmPatch.GetEntry<bool>("PatchGetRelationUnits").Value)
                // NOP a sex equality -> jmp out check
                if (!AsmPatch.ApplyPatch(
                        "PatchGetRelationUnits",
                        typeof(WorldInitNPCTool),
                        "NativeMethodInfoPtr_GetRelationUnits_Public_Static_List_1_WorldUnitBase_WorldUnitBase_List_1_WorldUnitBase_ConfNpcPartFittingItem_UnitRelationType_0",
                        "0F 84 ?? ?? ?? ?? 4C 8B 05 ?? ?? ?? ?? 8B D3 48 8B CF E8 ?? ?? ?? ?? 48 85 C0 0F 84 ?? ?? ?? ?? 48 8B 40 18 48 85 C0 0F 84 ?? ?? ?? ?? 48 8B 48 18",
                        0x1000,
                        new byte[] {0x90, 0x90, 0x90, 0x90, 0x90, 0x90}))
                {
                    LoggerInstance.Error($"PatchGetRelationUnits failed.");
                }

            if (_configAsmPatch.GetEntry<bool>("PatchVariousChildNameChecks").Value)
            {
                // NOP a sex equality -> jmp out check
                if (!AsmPatch.ApplyPatch(
                        "PatchVariousChildNameChecks_1",
                        typeof(UnitActionRelationSetParent),
                        "NativeMethodInfoPtr_OnCreate_Protected_Virtual_Void_0",
                        "75 ?? 49 8B 4D 10 48 85 C9 0F 84 ?? ?? ?? ??",
                        0x1000,
                        new byte[] {0x90, 0x90}))
                {
                    LoggerInstance.Error($"PatchVariousChildNameChecks_1 failed.");
                }

                // NOP a sex equality -> jmp out check
                if (!AsmPatch.ApplyPatch(
                        "PatchVariousChildNameChecks_2",
                        typeof(UnitActionRelationSetParent.__c__DisplayClass8_0),
                        "NativeMethodInfoPtr__OnCreate_b__0_Internal_Void_WorldUnitBase_String_0",
                        "0F 85 ?? ?? ?? ?? 33 C9 E8 ?? ?? ?? ??",
                        0x300,
                        new byte[] {0x90, 0x90, 0x90, 0x90, 0x90, 0x90}))
                {
                    LoggerInstance.Error($"PatchVariousChildNameChecks_2 failed.");
                }
            }
        }

        private void PatchVariousChildNameChecks()
        {
            IntPtr address =
                GetAddressForMethod(typeof(UnitActionRelationSetParent), "NativeMethodInfoPtr_OnCreate_Protected_Virtual_Void_0");
            // nop sex check
            PatchMemory(address,0x8A8, new byte[] { 0x90, 0x90 });
            MelonLogger.Msg("Patched UnitActionRelationSetParent::OnCreate");
            address =
                GetAddressForMethod(typeof(UnitActionRelationSetParent.__c__DisplayClass8_0), "NativeMethodInfoPtr__OnCreate_b__0_Internal_Void_WorldUnitBase_String_0");
            // nop sex check
            PatchMemory(address, 0xF4, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 } );
            MelonLogger.Msg("Patched UnitActionRelationSetParent::Lambda8::OnCreate");
        }

        private void PatchMemory(IntPtr address, int offset, byte[] bytes)
        {
            IntPtr writeAddress = address + offset;

            VirtualProtect(writeAddress, (uint) bytes.Length, Protection.PAGE_EXECUTE_READWRITE, out Protection old);
            for (int i = 0; i < bytes.Length; i++)
            {
                Marshal.WriteByte(writeAddress + i, bytes[i]);
            }
            VirtualProtect(writeAddress, (uint) bytes.Length, old, out Protection _);
        }

        private static IntPtr GetAddressForMethod(Type type, string fieldName)
        {
            FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            if (fieldInfo != null)
            {
                IntPtr pointer = (IntPtr) fieldInfo.GetValue(null);
                return Marshal.ReadIntPtr(pointer);
            }

            return IntPtr.Zero;
        }
        
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize,
            Protection flNewProtect, out Protection lpflOldProtect);

        private enum Protection {
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400
        }

        //[HarmonyPatch]
        //[HarmonyPatch(typeof(WorldInitNPCTool), nameof(WorldInitNPCTool.GetRelationUnits))]
        public class TempPatch
        {
            // static IEnumerable<MethodBase> TargetMethods()
            // {
            //     return AccessTools.GetTypesFromAssembly(System.Reflection.Assembly.GetAssembly(typeof(UICreatePlayer)))
            //         .SelectMany(type => type.GetMethods())
            //         .Where(method => method.ReturnType == typeof(void) && method.Name.Contains("CreateGameInfo"))
            //         .Cast<MethodBase>();
            // }
            
            //static void Prefix(UnitActionRelationSetParent __instance)
            //{
            //    MelonLogger.Msg("{0} {1} {2}", __instance.unit.data.unitData.unitID, __instance.manUnit.data.unitData.unitID, __instance.womanUnit.data.unitData.unitID);
            //}

            static void Postfix(ref List<WorldUnitBase> __result, ref int relationType)
            {
                if (relationType == 8)
                    MelonLogger.Msg(__result.Count);
            }
        }
    }
}