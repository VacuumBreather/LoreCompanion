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
            new PropertyMetadata(false, OnTypographyChanged));

        public static readonly DependencyProperty IncreaseLetterSpacingProperty = DependencyProperty.RegisterAttached(
            DependencyPropertyNameHelper.GetName(nameof(IncreaseLetterSpacingProperty)),
            typeof(bool),
            typeof(TypographyHelper),
            new PropertyMetadata(false, OnTypographyChanged));

        [AttachedPropertyBrowsableForType(typeof(TextBlock))]
        public static bool GetAllCaps(TextBlock textBlock) => (bool)textBlock.GetValue(AllCapsProperty);

        public static void SetAllCaps(TextBlock textBlock, bool value) => textBlock.SetValue(AllCapsProperty, value);

        [AttachedPropertyBrowsableForType(typeof(TextBlock))]
        public static bool GetIncreaseLetterSpacing(TextBlock textBlock) => (bool)textBlock.GetValue(IncreaseLetterSpacingProperty);

        public static void SetIncreaseLetterSpacing(TextBlock textBlock, bool value) => textBlock.SetValue(IncreaseLetterSpacingProperty, value);

        private static void OnTypographyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBlock textBlock) return;

            var isEnabled = GetAllCaps(textBlock) || GetIncreaseLetterSpacing(textBlock);

            TextDescriptor.RemoveValueChanged(textBlock, OnTextChanged);
            textBlock.Loaded -= OnTextBlockLoaded;

            if (isEnabled)
            {
                TextDescriptor.AddValueChanged(textBlock, OnTextChanged);
                textBlock.Loaded += OnTextBlockLoaded;
                ApplyTypography(textBlock);
            }
            else
            {
                var bindingBase = BindingOperations.GetBindingBase(textBlock, TextBlock.TextProperty);

                if (bindingBase is Binding)
                {
                    BindingOperations.GetBindingExpression(textBlock, TextBlock.TextProperty)?.UpdateTarget();
                }
                else if (bindingBase is MultiBinding)
                {
                    BindingOperations.GetMultiBindingExpression(textBlock, TextBlock.TextProperty)?.UpdateTarget();
                }
            }
        }

        private static void OnTextBlockLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                ApplyTypography(textBlock);
            }
        }

        private static void OnTextChanged(object? sender, EventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                ApplyTypography(textBlock);
            }
        }

        private static string TransformText(string? text, bool allCaps, bool increaseLetterSpacing)
        {
            if (string.IsNullOrEmpty(text)) return text ?? string.Empty;

            var result = text;

            if (allCaps)
            {
                result = result.ToUpperInvariant();
            }

            if (increaseLetterSpacing)
            {
                var clean = result.Replace("\u200A", "");
                result = string.Join("\u200A", clean.ToCharArray());
            }

            return result;
        }

        private static void ApplyTypography(TextBlock textBlock)
        {
            var allCaps = GetAllCaps(textBlock);
            var letterSpacing = GetIncreaseLetterSpacing(textBlock);

            if (!allCaps && !letterSpacing) return;

            var bindingBase = BindingOperations.GetBindingBase(textBlock, TextBlock.TextProperty);

            if (bindingBase is Binding binding)
            {
                if (binding.Converter is TypographyValueConverter)
                {
                    BindingOperations.GetBindingExpression(textBlock, TextBlock.TextProperty)?.UpdateTarget();
                    return;
                }

                var wrappedConverter = new TypographyValueConverter(
                    binding.Converter,
                    binding.ConverterParameter,
                    binding.ConverterCulture,
                    new WeakReference<TextBlock>(textBlock));

                var newBinding = CloneBinding(binding, wrappedConverter);
                BindingOperations.SetBinding(textBlock, TextBlock.TextProperty, newBinding);

                return;
            }

            if (bindingBase is MultiBinding multiBinding)
            {
                if (multiBinding.Converter is TypographyMultiValueConverter)
                {
                    BindingOperations.GetMultiBindingExpression(textBlock, TextBlock.TextProperty)?.UpdateTarget();
                    return;
                }

                var wrappedConverter = new TypographyMultiValueConverter(
                    multiBinding.Converter,
                    multiBinding.ConverterParameter,
                    multiBinding.ConverterCulture,
                    new WeakReference<TextBlock>(textBlock));

                var newBinding = CloneMultiBinding(multiBinding, wrappedConverter);
                BindingOperations.SetBinding(textBlock, TextBlock.TextProperty, newBinding);

                return;
            }

            var currentText = textBlock.Text;

            if (string.IsNullOrEmpty(currentText)) return;

            var transformedText = TransformText(currentText, allCaps, letterSpacing);

            if (string.Equals(currentText, transformedText, StringComparison.Ordinal)) return;

            // Unhook temporarily to prevent re-entrant event loops
            TextDescriptor.RemoveValueChanged(textBlock, OnTextChanged);

            try
            {
                textBlock.Text = transformedText;
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

        private sealed class TypographyValueConverter(
            IValueConverter? innerConverter,
            object? innerParameter,
            CultureInfo? innerCulture,
            WeakReference<TextBlock> textBlockRef) : IValueConverter
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

                if (value == null) return null;

                var str = value as string ?? value.ToString();
                if (str == null) return null;

                var allCaps = textBlockRef.TryGetTarget(out var tb) && GetAllCaps(tb);
                var letterSpacing = textBlockRef.TryGetTarget(out var tb2) && GetIncreaseLetterSpacing(tb2);

                return TransformText(str, allCaps, letterSpacing);
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

        private sealed class TypographyMultiValueConverter(
            IMultiValueConverter? innerConverter,
            object? innerParameter,
            CultureInfo? innerCulture,
            WeakReference<TextBlock> textBlockRef) : IMultiValueConverter
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

                if (result == null) return null;

                var str = result as string ?? result.ToString();
                if (str == null) return null;

                var allCaps = textBlockRef.TryGetTarget(out var tb) && GetAllCaps(tb);
                var letterSpacing = textBlockRef.TryGetTarget(out var tb2) && GetIncreaseLetterSpacing(tb2);

                return TransformText(str, allCaps, letterSpacing);
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