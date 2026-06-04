using MaterialDesignThemes.Wpf;
using System.Windows;

namespace Holiday_Explorer.MVVM.AttachedProperties
{
    public sealed class PackIconKindAssist
    {
        #region IconKind 
        public static readonly DependencyProperty IconKindProperty =
            DependencyProperty.RegisterAttached(
                "IconKind",
                typeof(PackIconKind),
                typeof(PackIconKindAssist),
                new PropertyMetadata(default(PackIconKind)));

        public static void SetIconKind(UIElement element, PackIconKind value) =>
            element.SetValue(IconKindProperty, value);

        public static PackIconKind GetIconKind(UIElement element) =>
            (PackIconKind)element.GetValue(IconKindProperty);
        #endregion
    }
}
