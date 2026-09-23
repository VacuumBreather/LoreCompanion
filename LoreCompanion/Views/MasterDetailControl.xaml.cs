using System.Windows;
using System.Windows.Controls;

namespace LoreCompanion.Views
{
    public partial class MasterDetailControl : UserControl
    {
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            nameof(ItemTemplate),
            typeof(DataTemplate),
            typeof(MasterDetailControl));

        public static readonly DependencyProperty DetailTemplateProperty = DependencyProperty.Register(
            nameof(DetailTemplate),
            typeof(DataTemplate),
            typeof(MasterDetailControl));

        public MasterDetailControl()
        {
            InitializeComponent();
        }

        public DataTemplate? ItemTemplate
        {
            get => (DataTemplate?)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public DataTemplate? DetailTemplate
        {
            get => (DataTemplate?)GetValue(DetailTemplateProperty);
            set => SetValue(DetailTemplateProperty, value);
        }
    }
}