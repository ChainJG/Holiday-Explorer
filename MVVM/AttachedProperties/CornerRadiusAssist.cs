using System.Windows;

namespace Holiday_Explorer.MVVM.AttachedProperties
{
    public static class CornerRadiusAssist
    {
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.RegisterAttached(
                "CornerRadius",
                typeof(CornerRadius),
                typeof(CornerRadiusAssist),
                new FrameworkPropertyMetadata(new CornerRadius(0)));

        public static CornerRadius GetCornerRadius(DependencyObject element)
        {
            return (CornerRadius)element.GetValue(CornerRadiusProperty);
        }

        public static void SetCornerRadius(
            DependencyObject element,
            CornerRadius value)
        {
            element.SetValue(CornerRadiusProperty, value);
        }
    }
}
