using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace LoreCompanion.Views.Effects
{
    public sealed class TintShaderEffect : ShaderEffect
    {
        public static readonly DependencyProperty InputProperty =
            RegisterPixelShaderSamplerProperty(nameof(Input), typeof(TintShaderEffect), 0);

        public static readonly DependencyProperty TintColorProperty = DependencyProperty.Register(
            nameof(TintColor),
            typeof(Color),
            typeof(TintShaderEffect),
            new UIPropertyMetadata(Colors.White, PixelShaderConstantCallback(0)));

        public TintShaderEffect()
        {
            PixelShader = new PixelShader
            {
                UriSource = new Uri(
                    "pack://application:,,,/LoreCompanion;component/Views/Resources/Shaders/TintShader.ps",
                    UriKind.Absolute),
            };

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(TintColorProperty);
        }

        public Brush Input
        {
            get => (Brush)GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }

        public Color TintColor
        {
            get => (Color)GetValue(TintColorProperty);
            set => SetValue(TintColorProperty, value);
        }
    }
}