using System.Windows;
using LoreCompanion.Utilities;
using LoreCompanion.ViewModels;

namespace LoreCompanion.Views
{
    public partial class MasterDetailControl
    {
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            nameof(ItemTemplate),
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

        private static readonly DependencyProperty DetailTemplateProperty = DependencyProperty.Register(
            nameof(DetailTemplate),
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

            Loaded += OnLoaded;
        }

        public DataTemplate? ItemTemplate
        {
            get => (DataTemplate?)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
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

        private DataTemplate? DetailTemplate
        {
            get => (DataTemplate?)GetValue(DetailTemplateProperty);
            set => SetValue(DetailTemplateProperty, value);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetDetailTemplate();
        }

        private static void OnEditModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not MasterDetailControl masterDetailControl)
            {
                return;
            }

            masterDetailControl.SetDetailTemplate();
        }

        private void SetDetailTemplate()
        {
            if (!AppHelper.IsAdminMode)
            {
                DetailTemplate = ReadOnlyDetailTemplate;

                return;
            }

            DetailTemplate = EditMode == EditMode.Edit ? EditDetailTemplate : ReadOnlyDetailTemplate;
        }
    }
}