using System.Collections;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Caliburn.Micro;
using LoreCompanion.Views.Extensions;

namespace LoreCompanion.Views.Controls
{
    /// <summary>Represents a selectable dialog item inside a <see cref="NotificationHost"/>.</summary>
    /// <seealso cref="TransitioningContentControl"/>
    [TemplatePart(Name = LayoutScaleTransformPartName, Type = typeof(ScaleTransform))]
    public class NotificationItem : TransitioningContentControl
    {
        /// <summary>The name of the layout scale transform template part.</summary>
        public const string LayoutScaleTransformPartName = "PART_LayoutScaleTransform";

        /// <summary>Gets the command to close a <see cref="NotificationItem"/>.</summary>
        public static readonly RoutedCommand CloseCommand = new(nameof(CloseCommand), typeof(NotificationItem));

        /// <summary>Identifies the <see cref="CloseTransitionEffect"/> dependency property.</summary>
        public static readonly DependencyProperty CloseTransitionEffectProperty = DependencyProperty.Register(
            nameof(CloseTransitionEffect),
            typeof(ITransition),
            typeof(NotificationItem),
            new PropertyMetadata(default(ITransition)));

        private readonly NotificationHost _host;

        private CancellationTokenSource? _cts;
        private ScaleTransform? _layoutScaleTransform;

        /// <summary>Initializes static members of the <see cref="NotificationItem"/> class.</summary>
        static NotificationItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(NotificationItem),
                new FrameworkPropertyMetadata(typeof(NotificationItem)));
        }

        /// <summary>Initializes a new instance of the <see cref="NotificationItem"/> class.</summary>
        /// <param name="host">The <see cref="NotificationHost"/> hosting this item.</param>
        public NotificationItem(NotificationHost host)
        {
            _host = host;
            CommandBindings.Add(new CommandBinding(CloseCommand, OnCloseExecuted));
        }

        /// <summary>Gets or sets the effect to run when closing this item.</summary>
        public ITransition? CloseTransitionEffect
        {
            get => (ITransition?)GetValue(CloseTransitionEffectProperty);
            set => SetValue(CloseTransitionEffectProperty, value);
        }

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            var nameScopeRoot = this.GetNameScopeRoot();

            _layoutScaleTransform = GetTemplateChild(LayoutScaleTransformPartName) as ScaleTransform;

            if (FindName(LayoutScaleTransformPartName) != null)
            {
                UnregisterName(LayoutScaleTransformPartName);
            }

            if (_layoutScaleTransform is not null)
            {
                nameScopeRoot.RegisterName(LayoutScaleTransformPartName, _layoutScaleTransform);
            }

            base.OnApplyTemplate();
        }

        /// <inheritdoc/>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            if (oldContent is IDeactivate oldDeactivate)
            {
                oldDeactivate.Deactivated -= OnContentDeactivatedAsync;
            }

            if (newContent is IDeactivate deactivate)
            {
                deactivate.Deactivated += OnContentDeactivatedAsync;
            }
            else
            {
                _ = Execute.OnUIThreadAsync(async () =>
                {
                    _cts = new CancellationTokenSource();

                    _host.Unloaded += OnHostUnloaded;

                    try
                    {
                        await Task.Delay(5000, _cts.Token);
                    }
                    finally
                    {
                        await RunClosingTransitionAsync();

                        if (_host.ItemsSource is IList itemsList)
                        {
                            itemsList.Remove(DataContext);
                        }

                        _host.Unloaded -= OnHostUnloaded;
                    }
                });
            }
        }

        private void OnCloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!ReferenceEquals(sender, this))
            {
                return;
            }

            if (Content is IClose closable)
            {
                _ = Execute.OnUIThreadAsync(() => closable.TryCloseAsync());
            }
            else
            {
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;
            }
        }

        private async Task OnContentDeactivatedAsync(object sender, DeactivationEventArgs e)
        {
            if (!e.WasClosed)
            {
                return;
            }

            await RunClosingTransitionAsync();
        }

        private void OnHostUnloaded(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _host.Unloaded -= OnHostUnloaded;
        }

        private Timeline? BuildScaleTimeline(TimeSpan duration)
        {
            if (_layoutScaleTransform is null)
            {
                return null;
            }

            const double StartScale = 1.0;
            const double EndScale = 0.0;

            var startFrame = new DiscreteDoubleKeyFrame(StartScale, TimeSpan.Zero);
            var endFrame = new EasingDoubleKeyFrame(EndScale, duration) { EasingFunction = new CubicEase() };

            var timeline = new DoubleAnimationUsingKeyFrames();
            timeline.KeyFrames.Add(startFrame);
            timeline.KeyFrames.Add(endFrame);
            timeline.Duration = duration;
            timeline.Completed += (_, _) => CancelScaling();

            _layoutScaleTransform.SetCurrentValue(ScaleTransform.ScaleYProperty, StartScale);

            Storyboard.SetTargetName(timeline, LayoutScaleTransformPartName);
            Storyboard.SetTargetProperty(timeline, new PropertyPath(ScaleTransform.ScaleYProperty));

            return timeline;
        }

        private void CancelScaling()
        {
            if (_layoutScaleTransform is null)
            {
                return;
            }

            _layoutScaleTransform.SetCurrentValue(ScaleTransform.ScaleYProperty, 0.0);
            _layoutScaleTransform = null;
        }

        private async ValueTask RunClosingTransitionAsync()
        {
            if (!CanPerformTransition || (CloseTransitionEffect is null && _layoutScaleTransform is null))
            {
                return;
            }

            CancelTransition();

            var storyboard = new Storyboard();
            var transitionEffect = CloseTransitionEffect?.Build(this);

            var layoutScaleEffect =
                BuildScaleTimeline(CloseTransitionEffect?.Duration ?? TimeSpan.FromMilliseconds(value: 500));

            if (transitionEffect != null)
            {
                storyboard.Children.Add(transitionEffect);
            }

            if (layoutScaleEffect != null)
            {
                storyboard.Children.Add(layoutScaleEffect);
            }

            var tcs = new TaskCompletionSource();

            storyboard.Completed += (_, _) => tcs.TrySetResult();

            storyboard.Begin(this.GetNameScopeRoot(), true);

            await tcs.Task;
        }
    }
}