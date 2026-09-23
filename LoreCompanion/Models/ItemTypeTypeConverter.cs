using System.ComponentModel;
using System.Globalization;

namespace LoreCompanion.Models
{
    public class ItemTypeTypeConverter() : EnumConverter(typeof(ItemType))
    {
        public override object? ConvertTo(
            ITypeDescriptorContext? context,
            CultureInfo? culture,
            object? value,
            Type destinationType)
        {
            if ((destinationType == typeof(string)) && value is ItemType itemType)
            {
                return itemType switch
                {
                    ItemType.Tool => "Tool",
                    ItemType.Ash => "Ash",
                    ItemType.CraftingMaterial => "Crafting Material",
                    ItemType.BolsteringMaterial => "Bolstering Material",
                    ItemType.KeyItem => "Key Item",
                    ItemType.Sorcery => "Sorcery",
                    ItemType.Incantation => "Incantation",
                    ItemType.AshOfWar => "Ash of War",
                    ItemType.MeleeWeapon => "Melee Weapon",
                    ItemType.RangedWeapon => "Ranged Weapon",
                    ItemType.ArrowBolt => "Arrow / Bolt",
                    ItemType.Shield => "Shield",
                    ItemType.Torch => "Torch",
                    ItemType.Head => "Head Armor",
                    ItemType.Chest => "Chest Armor",
                    ItemType.Arms => "Arms Armor",
                    ItemType.Legs => "Legs Armor",
                    ItemType.Talisman => "Talisman",
                    ItemType.Info => "Info",
                    var _ => base.ConvertTo(context, culture, value, destinationType),
                };
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}