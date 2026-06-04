using System.Windows;
using System.Windows.Media;

namespace Holiday_Explorer.MVVM.AttachedProperties
{
    public static class RotationAngleAssist
    {
        public static readonly DependencyProperty RotationAngleProperty =
            DependencyProperty.RegisterAttached(
                "RotationAngle",
                typeof(double),
                typeof(RotationAngleAssist),
                new PropertyMetadata(0d, OnRotationPropertyChanged));

        public static readonly DependencyProperty RotationModeProperty =
            DependencyProperty.RegisterAttached(
                "RotationMode",
                typeof(RotationTransformMode),
                typeof(RotationAngleAssist),
                new PropertyMetadata(RotationTransformMode.Render, OnRotationPropertyChanged));

        public static readonly DependencyProperty RotationOriginProperty =
            DependencyProperty.RegisterAttached(
                "RotationOrigin",
                typeof(Point),
                typeof(RotationAngleAssist),
                new PropertyMetadata(new Point(0.5, 0.5), OnRotationPropertyChanged));

        public static double GetRotationAngle(DependencyObject element)
        {
            return (double)element.GetValue(RotationAngleProperty);
        }

        public static void SetRotationAngle(DependencyObject element, double value)
        {
            element.SetValue(RotationAngleProperty, value);
        }

        public static RotationTransformMode GetRotationMode(DependencyObject element)
        {
            return (RotationTransformMode)element.GetValue(RotationModeProperty);
        }

        public static void SetRotationMode(DependencyObject element, RotationTransformMode value)
        {
            element.SetValue(RotationModeProperty, value);
        }

        public static Point GetRotationOrigin(DependencyObject element)
        {
            return (Point)element.GetValue(RotationOriginProperty);
        }

        public static void SetRotationOrigin(DependencyObject element, Point value)
        {
            element.SetValue(RotationOriginProperty, value);
        }

        private static void OnRotationPropertyChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not FrameworkElement element)
                return;

            var angle = GetRotationAngle(element);
            var mode = GetRotationMode(element);
            var origin = GetRotationOrigin(element);

            element.RenderTransformOrigin = origin;

            var rotation = new RotateTransform(angle);

            if (mode == RotationTransformMode.Layout)
            {
                element.LayoutTransform = rotation;
                return;
            }

            element.RenderTransform = rotation;
        }
    }

    public enum RotationTransformMode
    {
        Render,
        Layout
    }
}