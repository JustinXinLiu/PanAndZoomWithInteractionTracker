using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

namespace PanAndZoomWithInteractionTracker
{
    public static class Extensions
    {
        public static Visual Visual(this UIElement element)
        {
            return ElementCompositionPreview.GetElementVisual(element);
        }

        public static ContainerVisual ContainerVisual(this UIElement element)
        {
            var hostVisual = ElementCompositionPreview.GetElementVisual(element);
            var root = hostVisual.Compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(element, root);
            return root;
        }
    }
}
