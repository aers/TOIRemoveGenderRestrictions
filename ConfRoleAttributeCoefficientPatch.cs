using HarmonyLib;
using MelonLoader;

namespace TOIRemoveGenderRestrictions
{
    [HarmonyPatch(typeof(ConfRoleAttributeCoefficient), nameof(ConfRoleAttributeCoefficient.RandomInitNPCUnit))]
    public class ConfRoleAttributeCoefficientPatch
    {
        static void Prefix(ref int sex)
        {
            if (sex == 0)
            {
                sex = MelonPreferences.GetCategory("RemoveGenderRestriction Optional")
                    .GetEntry<int>("SingleGenderWorldGeneration").Value;
            }
        }
    }
}