using System.Numerics;
using Windows.Devices.Input;
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;

namespace PanAndZoomWithInteractionTracker
{
    public sealed partial class MainPage : Page
    {
        #region Fields

        private Visual _rootVisual;
        private Visual _pathContainerVisual;

        private Compositor _compositor;
        private VisualInteractionSource _interactionSource;
        private InteractionTracker _tracker;

        //private IDictionary<uint, Vector2> _points = new Dictionary<uint, Vector2>();

        #endregion

        public MainPage()
        {
            InitializeComponent();

            Loaded += MainPage_Loaded;
            Root.SizeChanged += Root_SizeChanged;
            Root.PointerPressed += Root_OnPointerPressed;
            //Root.PointerReleased += Root_PointerReleased;
        }

        #region Handlers

        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_rootVisual == null || _pathContainerVisual == null) return;

            _rootVisual.Size = new Vector2((float)Root.ActualWidth, (float)Root.ActualHeight);
            _pathContainerVisual.Size = new Vector2(_rootVisual.Size.X, _rootVisual.Size.Y);
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _rootVisual = ElementCompositionPreview.GetElementVisual(Root);
            _rootVisual.Size = new Vector2((float)Root.ActualWidth, (float)Root.ActualHeight);

            _pathContainerVisual = ElementCompositionPreview.GetElementVisual(PathContainer);
            _pathContainerVisual.Size = new Vector2(_rootVisual.Size.X, _rootVisual.Size.Y);

            _compositor = _rootVisual.Compositor;

            _interactionSource = VisualInteractionSource.Create(_rootVisual);
            _interactionSource.PositionXSourceMode = InteractionSourceMode.EnabledWithInertia;
            _interactionSource.PositionYSourceMode = InteractionSourceMode.EnabledWithInertia;
            _interactionSource.IsPositionXRailsEnabled = false;
            _interactionSource.IsPositionYRailsEnabled = false;
            _interactionSource.ScaleSourceMode = InteractionSourceMode.EnabledWithInertia;
            _interactionSource.ManipulationRedirectionMode = VisualInteractionSourceRedirectionMode.CapableTouchpadOnly;

            _tracker = InteractionTracker.Create(_compositor);
            _tracker.InteractionSources.Add(_interactionSource);
            _tracker.MaxPosition = new Vector3(_rootVisual.Size.X, _rootVisual.Size.Y, 0.0f) * 10.0f;
            _tracker.MinPosition = _tracker.MaxPosition * -1.0f;
            _tracker.MinScale = 0.9f;
            _tracker.MaxScale = 12.0f;
            _tracker.ScaleInertiaDecayRate = 0.95f;

            var positionAnimation = _compositor.CreateExpressionAnimation("-t.Position");
            positionAnimation.SetReferenceParameter("t", _tracker);
            _pathContainerVisual.StartAnimation("Offset", positionAnimation);

            var scaleAnimation = _compositor.CreateExpressionAnimation("Vector2(t.Scale, t.Scale)");
            scaleAnimation.SetReferenceParameter("t", _tracker);
            _pathContainerVisual.StartAnimation("Scale.XY", scaleAnimation);

            //var centerAnimation = _compositor.CreateExpressionAnimation("-t.Position.XY + v.Size * 0.5f");
            //centerAnimation.SetReferenceParameter("t", _tracker);
            //centerAnimation.SetReferenceParameter("v", _pathContainerVisual);
            //_pathContainerVisual.StartAnimation("CenterPoint.XY", centerAnimation);
        }

        private void Root_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType != PointerDeviceType.Touch) return;

            // This won't work since Root_PointerReleased is not getting fired.

            //var id = e.GetCurrentPoint(Root).PointerId;
            //var position = e.GetCurrentPoint(Root).Position;

            //if (_points.All(p => p.Key != id))
            //{
            //    _points[id] = new Vector2((float)position.X, (float)position.Y);
            //}

            //if (_points.Any())
            //{
            //    var center = new Vector3(_points.Sum(p => p.Value.X) / _points.Count, _points.Sum(p => p.Value.Y) / _points.Count, 0.0f);
            //    _pathContainerVisual.CenterPoint = center;
            //}

            _interactionSource.TryRedirectForManipulation(e.GetCurrentPoint(Root));
        }

        //private void Root_PointerReleased(object sender, PointerRoutedEventArgs e)
        //{
        //    // Because all manipulations are redirected to the tracker, this won't be fired.

        //    //var id = e.GetCurrentPoint(Root).PointerId;
        //    //_points.Remove(id);
        //}

        //private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        //{
        //    await Task.Delay(1000);

        //    //var root = Root.ContainerVisual();
        //    //var compositor = root.Compositor;

        //    var visual = Logo.Visual();
        //    //var visual = compositor.CreateSpriteVisual();
        //    //visual.Size = new Vector2(40, 40);
        //    visual.CenterPoint = Vector3.Zero;
        //    visual.Offset = Vector3.Zero;
        //    //visual.Brush = compositor.CreateColorBrush(Colors.CadetBlue);

        //    //root.Children.InsertAtTop(visual);

        //    visual.StartScaleAnimation(null, new Vector2((float)1 * 20, (float)1 * 20), 5000.0d);

        //    await Task.Delay(5001);
        //    //visual.Scale = Vector3.One;
        //    //BullclipLogo.Width = BullclipLogo.Width / 12;
        //    //BullclipLogo.Height = BullclipLogo.Height / 12;

        //    //visual.Opacity = 0.1f;

        //    //Logo.InvalidateMeasure();
        //    //Logo.InvalidateArrange();
        //    //Logo.UpdateLayout();
        //}

        #endregion
    }
}
