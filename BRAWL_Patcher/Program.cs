using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.FormKeys.Fallout4;
using Noggog;

namespace BRAWL_Patcher
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<IFallout4Mod, IFallout4ModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.Fallout4, "Broken Weapon System.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<IFallout4Mod, IFallout4ModGetter> state)
        {
            foreach (var armorContext in state.LoadOrder.PriorityOrder.Armor().WinningContextOverrides())
            {
                if (armorContext.Record is null) continue;
                if (armorContext.Record.IsDeleted) continue;
                var record = armorContext.Record;

                if (record.MajorFlags.HasFlag(Armor.MajorFlag.NonPlayable)) continue;
                if (record.Name is null || string.IsNullOrWhiteSpace(record.Name.String)) continue;
                if (record.Race.FormKey != Fallout4.Race.HumanRace.FormKey) continue;
                if (!record.InstanceNaming.IsNull) continue;
                //IArmor newRecord = null;
                var newRecord = record.DeepCopy();
                var modified = false;
                {
                    var needToChange = record.InstanceNaming.IsNull ||
                                       string.IsNullOrEmpty(record.InstanceNaming.TryResolve(state.LinkCache)?.EditorID);
                    if (!record.HasKeyword(Fallout4.Keyword.ArmorTypePower))
                    {
                        if (needToChange)
                        {
                            newRecord.InstanceNaming.SetTo(FormKey.Factory("000092:Broken Weapon System.esp"));
                            modified = true;
                        }
                    }
                    if (modified) state.PatchMod.Armors.Set(newRecord);
                }
            }
            foreach (var weaponContext in state.LoadOrder.PriorityOrder.Weapon().WinningContextOverrides())
            {
                if (weaponContext.Record is null) continue;
                if (weaponContext.Record.IsDeleted) continue;

                if (weaponContext.Record.FormKey.ModKey ==
                    ModKey.FromNameAndExtension("IDEKsLogisticsStation2.esl")) continue;

                var record = weaponContext.Record;
                if (record.MajorFlags.HasFlag(Weapon.MajorFlag.NonPlayable)) continue;
                if (record.Flags.HasFlag(Weapon.Flag.NotPlayable)) continue;
                if (record.Name is null || string.IsNullOrWhiteSpace(record.Name.String)) continue;

                if (record.Keywords is null) continue;

                if (record.Flags.HasFlag(Weapon.Flag.NotUsedInNormalCombat) ||
                    record.Flags.HasFlag(Weapon.Flag.NonHostile) || record.Flags.HasFlag(Weapon.Flag.CannotDrop)
                    || record.Flags.HasFlag(Weapon.Flag.EmbeddedWeapon)) continue;

                if (record.EquipmentType.IsNull ||
                    !(record.EquipmentType.FormKey == Fallout4.EquipType.BothHands.FormKey ||
                        record.EquipmentType.FormKey == Fallout4.EquipType.BothHandsLeftOptional.FormKey ||
                        record.EquipmentType.FormKey == Fallout4.EquipType.GrenadeSlot.FormKey ||
                        record.EquipmentType.FormKey == Fallout4.EquipType.RightHand.FormKey))
                    continue;
                if (record.EquipmentType.FormKey == Fallout4.EquipType.GrenadeSlot.FormKey) continue;
                if (!record.InstanceNaming.IsNull) continue;
                var newRecord = record.DeepCopy();
                var modified = false;
                {
                    var needToChange = record.InstanceNaming.IsNull ||
                                        string.IsNullOrEmpty(record.InstanceNaming.TryResolve(state.LinkCache)?.EditorID);
                    if (needToChange)
                    {
                        if (record.AnimationType == Weapon.AnimationTypes.Gun)
                        {
                            newRecord.InstanceNaming.SetTo(FormKey.Factory("000093:Broken Weapon System.esp"));
                            modified = true;
                        }
                        else if (record.AnimationType is Weapon.AnimationTypes.OneHandAxe
                                    or Weapon.AnimationTypes.OneHandDagger or Weapon.AnimationTypes.OneHandSword
                                    or Weapon.AnimationTypes.TwoHandAxe or Weapon.AnimationTypes.TwoHandSword)
                        {
                            newRecord.InstanceNaming.SetTo(Fallout4.InstanceNamingRules.dn_CommonMelee);
                            modified = true;
                        }
                    }
                }
                if (modified) state.PatchMod.Weapons.Set(newRecord);
            }
        }
    }
}
