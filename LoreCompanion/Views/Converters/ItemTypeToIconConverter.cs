using System.Globalization;
using System.Windows;
using System.Windows.Data;
using LoreCompanion.Models;
using MahApps.Metro.IconPacks;

namespace LoreCompanion.Views.Converters
{
    public class ItemTypeToIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not ItemType itemType)
            {
                return DependencyProperty.UnsetValue;
            }

            return itemType switch
            {
                ItemType.Tool => PackIconRPGAwesomeKind.FireBomb,
                ItemType.Ash => PackIconRPGAwesomeKind.RingingBell,
                ItemType.CraftingMaterial => PackIconRPGAwesomeKind.Leaf,
                ItemType.BolsteringMaterial => PackIconRPGAwesomeKind.MuscleUp,
                ItemType.KeyItem => PackIconRPGAwesomeKind.Key,
                ItemType.Sorcery => PackIconRPGAwesomeKind.FairyWand,
                ItemType.Incantation => PackIconRPGAwesomeKind.LightningTrio,
                ItemType.AshOfWar => PackIconRPGAwesomeKind.SpinningSword,
                ItemType.MeleeWeapon => PackIconRPGAwesomeKind.Broadsword,
                ItemType.RangedWeapon => PackIconRPGAwesomeKind.Crossbow,
                ItemType.ArrowBolt => PackIconRPGAwesomeKind.ArrowFlights,
                ItemType.Shield => PackIconRPGAwesomeKind.Shield,
                ItemType.Torch => PackIconRPGAwesomeKind.Torch,
                ItemType.Head => PackIconRPGAwesomeKind.KnightHelmet,
                ItemType.Chest => PackIconRPGAwesomeKind.Vest,
                ItemType.Arms => PackIconRPGAwesomeKind.Hand,
                ItemType.Legs => PackIconRPGAwesomeKind.ShoePrints,
                ItemType.Talisman => PackIconRPGAwesomeKind.GemPendant,
                ItemType.Info => PackIconRPGAwesomeKind.Book,
                var _ => DependencyProperty.UnsetValue,
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}