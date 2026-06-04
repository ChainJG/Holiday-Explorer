using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace Holiday_Explorer.MVVM.Behaviors
{
    public static class PulsePopBehavior
    {
        private class PulseState
        {
            public Stopwatch Stopwatch { get; } = Stopwatch.StartNew();
            public ScaleTransform Transform { get; } = new();
        }

        private static readonly Dictionary<UIElement, PulseState> States = new();

        #region ReplayOnChange

        public static readonly DependencyProperty ReplayOnChangeProperty =
            DependencyProperty.RegisterAttached(
                "ReplayOnChange",
                typeof(object),
                typeof(PulsePopBehavior),
                new PropertyMetadata(null, OnReplayOnChangeChanged));

        public static object? GetReplayOnChange(DependencyObject obj) =>
            obj.GetValue(ReplayOnChangeProperty);

        public static void SetReplayOnChange(DependencyObject obj, object? value) =>
            obj.SetValue(ReplayOnChangeProperty, value);

        #endregion

        #region StartScale

        public static readonly DependencyProperty StartScaleProperty =
            DependencyProperty.RegisterAttached(
                "StartScale",
                typeof(double),
                typeof(PulsePopBehavior),
                new PropertyMetadata(0.7));

        public static double GetStartScale(DependencyObject obj) =>
            (double)obj.GetValue(StartScaleProperty);

        public static void SetStartScale(DependencyObject obj, double value) =>
            obj.SetValue(StartScaleProperty, value);

        #endregion

        #region Overshoot

        public static readonly DependencyProperty OvershootProperty =
            DependencyProperty.RegisterAttached(
                "Overshoot",
                typeof(double),
                typeof(PulsePopBehavior),
                new PropertyMetadata(1.15));

        public static double GetOvershoot(DependencyObject obj) =>
            (double)obj.GetValue(OvershootProperty);

        public static void SetOvershoot(DependencyObject obj, double value) =>
            obj.SetValue(OvershootProperty, value);

        #endregion

        #region Duration

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.RegisterAttached(
                "Duration",
                typeof(double),
                typeof(PulsePopBehavior),
                new PropertyMetadata(0.35));

        public static double GetDuration(DependencyObject obj) =>
            (double)obj.GetValue(DurationProperty);

        public static void SetDuration(DependencyObject obj, double value) =>
            obj.SetValue(DurationProperty, value);

        #endregion

        private static void OnReplayOnChangeChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not FrameworkElement element)
                return;

            if (e.OldValue is null)
                return;

            if (!element.IsLoaded)
                return;

            RestartAnimation(element);
        }

        private static void RestartAnimation(UIElement element)
        {
            Stop(element);

            var state = CreateState(element);

            States[element] = state;

            CompositionTarget.Rendering -= OnRender;
            CompositionTarget.Rendering += OnRender;
        }

        private static PulseState CreateState(UIElement element)
        {
            var state = new PulseState();

            element.RenderTransform = state.Transform;
            element.RenderTransformOrigin = new Point(0.5, 0.5);

            var startScale = GetStartScale(element);

            state.Transform.ScaleX = startScale;
            state.Transform.ScaleY = startScale;

            return state;
        }

        private static void Stop(UIElement element)
        {
            if (!States.Remove(element))
                return;

            if (States.Count == 0)
                CompositionTarget.Rendering -= OnRender;
        }

        private static void OnRender(object? sender, EventArgs e)
        {
            var completedElements = new List<UIElement>();

            foreach (var pair in States)
            {
                var element = pair.Key;
                var state = pair.Value;

                var duration = GetDuration(element);
                var overshoot = GetOvershoot(element);
                var startScale = GetStartScale(element);

                var t = state.Stopwatch.Elapsed.TotalSeconds / duration;

                if (t >= 1.0)
                {
                    state.Transform.ScaleX = 1.0;
                    state.Transform.ScaleY = 1.0;

                    completedElements.Add(element);
                    continue;
                }

                var value = EaseOutBack(t, startScale, overshoot);

                state.Transform.ScaleX = value;
                state.Transform.ScaleY = value;
            }

            foreach (var element in completedElements)
                States.Remove(element);

            if (States.Count == 0)
                CompositionTarget.Rendering -= OnRender;
        }

        private static double EaseOutBack(
            double t,
            double start,
            double overshoot)
        {
            var c1 = overshoot;
            var c3 = c1 + 1;

            var x = 1 - Math.Pow(1 - t, 3);

            return start + (1 - start) *
                (1 + c3 * Math.Pow(x - 1, 3) + c1 * Math.Pow(x - 1, 2));
        }
    }
}