using System.Windows;
using LoreCompanion.ViewModels;

namespace LoreCompanion.Views
{
    public partial class MasterDetailControl
    {
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            nameof(ItemTemplate),
            typeof(DataTemplate),
            typeof(MasterDetailControl));

        public static readonly DependencyProperty DetailTemplateProperty = DependencyProperty.Register(
            nameof(DetailTemplate),
            typeof(DataTemplate),
            typeof(MasterDetailControl));

        public static readonly DependencyProperty ReadOnlyDetailTemplateProperty = DependencyProperty.Register(
            nameof(ReadOnlyDetailTemplate),
            typeof(DataTemplate),
            typeof(MasterDetailControl));

        public static readonly DependencyProperty EditDetailTemplateProperty = DependencyProperty.Register(
            nameof(EditDetailTemplate),
            typeof(DataTemplate),
            typeof(MasterDetailControl));

        public static readonly DependencyProperty EditModeProperty = DependencyProperty.Register(
            nameof(EditMode),
            typeof(EditMode),
            typeof(MasterDetailControl),
            new PropertyMetadata(EditMode.ReadOnly, OnEditModeChanged));

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

        public DataTemplate? ReadOnlyDetailTemplate
        {
            get => (DataTemplate?)GetValue(ReadOnlyDetailTemplateProperty);
            set => SetValue(ReadOnlyDetailTemplateProperty, value);
        }

        public DataTemplate? EditDetailTemplate
        {
            get => (DataTemplate?)GetValue(EditDetailTemplateProperty);
            set => SetValue(EditDetailTemplateProperty, value);
        }

        public EditMode EditMode
        {
            get => (EditMode)GetValue(EditModeProperty);
            set => SetValue(EditModeProperty, value);
        }

        private static void OnEditModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not MasterDetailControl masterDetailControl)
            {
                return;
            }

            var editMode = (EditMode)e.NewValue;

            masterDetailControl.DetailTemplate = editMode == EditMode.Edit
                                                     ? masterDetailControl.EditDetailTemplate
                                                     : masterDetailControl.ReadOnlyDetailTemplate;
        }
    }
}