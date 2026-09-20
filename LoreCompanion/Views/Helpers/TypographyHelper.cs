using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LoreCompanion.Views.Helpers
{
    public static class TypographyHelper
    {
        private static readonly DependencyPropertyDescriptor TextDescriptor =
            DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));

        public static readonly DependencyProperty AllCapsProperty = DependencyProperty.RegisterAttached(
            DependencyPropertyNameHelper.GetName(nameof(AllCapsProperty)),
            typeof(bool),
            typeof(TypographyHelper),
            new PropertyMetadata(false, OnCapitalsChanged));

        [AttachedPropertyBrowsableForType(typeof(TextBlock))]
        public static bool GetAllCaps(TextBlock textBlock) => (bool)textBlock.GetValue(AllCapsProperty);

        public static void SetAllCaps(TextBlock textBlock, bool value) => textBlock.SetValue(AllCapsProperty, value);

        private static void OnCapitalsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBlock textBlock) return;

            if (e.NewValue is true)
            {
                TextDescriptor.AddValueChanged(textBlock, OnTextChanged);
                textBlock.Loaded += OnTextBlockLoaded;
                ApplyCapitals(textBlock);
            }
            else
            {
                TextDescriptor.RemoveValueChanged(textBlock, OnTextChanged);
                textBlock.Loaded -= OnTextBlockLoaded;
            }
        }

        private static void OnTextBlockLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                ApplyCapitals(textBlock);
            }
        }

        private static void OnTextChanged(object? sender, EventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                ApplyCapitals(textBlock);
            }
        }

        private static void ApplyCapitals(TextBlock textBlock)
        {
            if (!GetAllCaps(textBlock)) return;

            var bindingBase = BindingOperations.GetBindingBase(textBlock, TextBlock.TextProperty);

            if (bindingBase is Binding binding)
            {
                if (binding.Converter is UpperCaseValueConverter) return;

                var wrappedConverter = new UpperCaseValueConverter(
                    binding.Converter,
                    binding.ConverterParameter,
                    binding.ConverterCulture);

                var newBinding = CloneBinding(binding, wrappedConverter);
                BindingOperations.SetBinding(textBlock, TextBlock.TextProperty, newBinding);

                return;
            }

            if (bindingBase is MultiBinding multiBinding)
            {
                if (multiBinding.Converter is UpperCaseMultiValueConverter) return;

                var wrappedConverter = new UpperCaseMultiValueConverter(
                    multiBinding.Converter,
                    multiBinding.ConverterParameter,
                    multiBinding.ConverterCulture);

                var newBinding = CloneMultiBinding(multiBinding, wrappedConverter);
                BindingOperations.SetBinding(textBlock, TextBlock.TextProperty, newBinding);

                return;
            }

            var currentText = textBlock.Text;

            if (string.IsNullOrEmpty(currentText)) return;

            var upperText = currentText.ToUpperInvariant();

            if (string.Equals(currentText, upperText, StringComparison.Ordinal)) return;

            // Unhook temporarily to prevent re-entrant event loops
            TextDescriptor.RemoveValueChanged(textBlock, OnTextChanged);

            try
            {
                textBlock.Text = upperText;
            }
            finally
            {
                TextDescriptor.AddValueChanged(textBlock, OnTextChanged);
            }
        }

        private static Binding CloneBinding(Binding source, IValueConverter converter)
        {
            var newBinding = new Binding
            {
                Path = source.Path,
                XPath = source.XPath,
                Mode = source.Mode,
                UpdateSourceTrigger = source.UpdateSourceTrigger,
                Converter = converter,
                ConverterParameter = source.ConverterParameter,
                ConverterCulture = source.ConverterCulture,
                FallbackValue = source.FallbackValue,
                TargetNullValue = source.TargetNullValue,
                StringFormat = source.StringFormat,
                ValidatesOnDataErrors = source.ValidatesOnDataErrors,
                ValidatesOnExceptions = source.ValidatesOnExceptions,
                NotifyOnValidationError = source.NotifyOnValidationError,
                NotifyOnSourceUpdated = source.NotifyOnSourceUpdated,
                NotifyOnTargetUpdated = source.NotifyOnTargetUpdated,
                AsyncState = source.AsyncState,
                BindingGroupName = source.BindingGroupName,
                Delay = source.Delay,
                IsAsync = source.IsAsync,
                UpdateSourceExceptionFilter = source.UpdateSourceExceptionFilter
            };

            if (source.Source != null)
            {
                newBinding.Source = source.Source;
            }
            else if (source.RelativeSource != null)
            {
                newBinding.RelativeSource = source.RelativeSource;
            }
            else if (source.ElementName != null)
            {
                newBinding.ElementName = source.ElementName;
            }

            foreach (var rule in source.ValidationRules)
            {
                newBinding.ValidationRules.Add(rule);
            }

            return newBinding;
        }

        private static MultiBinding CloneMultiBinding(MultiBinding source, IMultiValueConverter converter)
        {
            var newBinding = new MultiBinding
            {
                Mode = source.Mode,
                UpdateSourceTrigger = source.UpdateSourceTrigger,
                Converter = converter,
                ConverterParameter = source.ConverterParameter,
                ConverterCulture = source.ConverterCulture,
                FallbackValue = source.FallbackValue,
                TargetNullValue = source.TargetNullValue,
                StringFormat = source.StringFormat,
                ValidatesOnDataErrors = source.ValidatesOnDataErrors,
                ValidatesOnExceptions = source.ValidatesOnExceptions,
                NotifyOnValidationError = source.NotifyOnValidationError,
                NotifyOnSourceUpdated = source.NotifyOnSourceUpdated,
                NotifyOnTargetUpdated = source.NotifyOnTargetUpdated,
                BindingGroupName = source.BindingGroupName,
                Delay = source.Delay,
                UpdateSourceExceptionFilter = source.UpdateSourceExceptionFilter
            };

            foreach (var b in source.Bindings)
            {
                newBinding.Bindings.Add(b);
            }

            foreach (var rule in source.ValidationRules)
            {
                newBinding.ValidationRules.Add(rule);
            }

            return newBinding;
        }

        private sealed class UpperCaseValueConverter(
            IValueConverter? innerConverter,
            object? innerParameter,
            CultureInfo? innerCulture) : IValueConverter
        {
            public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                if (innerConverter != null)
                {
                    value = innerConverter.Convert(
                        value,
                        targetType,
                        innerParameter ?? parameter,
                        innerCulture ?? culture);
                }

                if (value is string str)
                {
                    return str.ToUpperInvariant();
                }

                return value?.ToString()?.ToUpperInvariant();
            }

            public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                if (innerConverter != null)
                {
                    return innerConverter.ConvertBack(
                        value,
                        targetType,
                        innerParameter ?? parameter,
                        innerCulture ?? culture);
                }

                return value;
            }
        }

        private sealed class UpperCaseMultiValueConverter(
            IMultiValueConverter? innerConverter,
            object? innerParameter,
            CultureInfo? innerCulture) : IMultiValueConverter
        {
            public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
            {
                object? result;

                if (innerConverter != null)
                {
                    result = innerConverter.Convert(
                        values,
                        targetType,
                        innerParameter ?? parameter,
                        innerCulture ?? culture);
                }
                else
                {
                    result = values.Length > 0 ? values[0] : null;
                }

                if (result is string str)
                {
                    return str.ToUpperInvariant();
                }

                return result?.ToString()?.ToUpperInvariant();
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                if (innerConverter != null)
                {
                    return innerConverter.ConvertBack(value, targetTypes, parameter, culture);
                }

                return [value];
            }
        }
    }
}