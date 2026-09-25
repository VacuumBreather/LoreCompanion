using System.ComponentModel;

namespace LoreCompanion.Models
{
    [TypeConverter(typeof(ItemTypeTypeConverter))]
    public enum ItemType
    {
        Tool,
        Ash,
        CraftingMaterial,
        BolsteringMaterial,
        KeyItem,
        Sorcery,
        Incantation,
        AshOfWar,
        MeleeWeapon,
        RangedWeapon,
        ArrowBolt,
        Shield,
        Torch,
        Head,
        Chest,
        Arms,
        Legs,
        Talisman,
        Info,
    }
}