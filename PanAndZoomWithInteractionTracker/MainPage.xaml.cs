using System;
using System.Diagnostics;
using System.Numerics;
using Windows.Devices.Input;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace PanAndZoomWithInteractionTracker
{
    public sealed partial class MainPage : Page, IInteractionTrackerOwner
    {
        #region Fields

        private Visual _rootVisual;
        private Visual _pathContainerVisual;

        private Compositor _compositor;
        private VisualInteractionSource _interactionSource;
        private InteractionTracker _tracker;

        private float _newTrackerScale;

        private static readonly Random Random = new Random();

        #endregion

        public MainPage()
        {
            InitializeComponent();

            Loaded += MainPage_Loaded;
            Root.SizeChanged += Root_SizeChanged;
            Root.PointerPressed += Root_OnPointerPressed;
        }

        #region Handlers

        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_rootVisual == null || _pathContainerVisual == null) return;

            var rootSize = new Vector2((float)Root.ActualWidth, (float)Root.ActualHeight);
            _rootVisual.Size = _pathContainerVisual.Size = rootSize;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _rootVisual = Root.Visual();
            _rootVisual.Size = new Vector2((float)Root.ActualWidth, (float)Root.ActualHeight);

            _pathContainerVisual = PathContainer.Visual();
            _pathContainerVisual.Size = _rootVisual.Size;

            _compositor = _rootVisual.Compositor;

            //CreateMoreShapes();

            _interactionSource = VisualInteractionSource.Create(_rootVisual);
            _interactionSource.PositionXSourceMode = InteractionSourceMode.EnabledWithInertia;
            _interactionSource.PositionYSourceMode = InteractionSourceMode.EnabledWithInertia;
            _interactionSource.IsPositionXRailsEnabled = false;
            _interactionSource.IsPositionYRailsEnabled = false;
            _interactionSource.ScaleSourceMode = InteractionSourceMode.EnabledWithInertia;
            _interactionSource.ManipulationRedirectionMode = VisualInteractionSourceRedirectionMode.CapableTouchpadOnly;

            _tracker = InteractionTracker.CreateWithOwner(_compositor, this);
            _tracker.InteractionSources.Add(_interactionSource);
            _tracker.MaxPosition = new Vector3(_rootVisual.Size, 0.0f) * 3.0f;
            _tracker.MinPosition = _tracker.MaxPosition * 1.0f;
            _tracker.MinScale = 0.9f;
            _tracker.MaxScale = 12.0f;
            _tracker.ScaleInertiaDecayRate = 0.96f;

            var positionAnimation = _compositor.CreateExpressionAnimation("-t.Position");
            positionAnimation.SetReferenceParameter("t", _tracker);
            _pathContainerVisual.StartAnimation("Offset", positionAnimation);

            var scaleAnimation = _compositor.CreateExpressionAnimation("Vector2(t.Scale, t.Scale)");
            scaleAnimation.SetReferenceParameter("t", _tracker);
            _pathContainerVisual.StartAnimation("Scale.XY", scaleAnimation);
        }

        private void Root_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType != PointerDeviceType.Touch) return;

            _interactionSource.TryRedirectForManipulation(e.GetCurrentPoint(Root));
        }

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

        #region IInteractionTrackerOwner Implementation

        public void InteractingStateEntered(InteractionTracker sender, InteractionTrackerInteractingStateEnteredArgs args)
        {
            Debug.WriteLine("Fingers down");
        }

        public void ValuesChanged(InteractionTracker sender, InteractionTrackerValuesChangedArgs args)
        {
            //Debug.WriteLine($"Scale: {args.Scale}");
            //Debug.WriteLine($"Position: {args.Position}");

            _newTrackerScale = args.Scale;
        }

        public void InertiaStateEntered(InteractionTracker sender, InteractionTrackerInertiaStateEnteredArgs args)
        {
        }

        public void IdleStateEntered(InteractionTracker sender, InteractionTrackerIdleStateEnteredArgs args)
        {
            Debug.WriteLine("Fingers up");

            return;

            foreach (var child in PathContainer.Children)
            {
                var shape = child as Path;
                if (shape == null) return;

                // TODO: ALso need to check if the shape is IN VIEW.

                var originalWidth = shape.ActualWidth;
                var originalHeight = shape.ActualHeight;

                var shapeVisual = shape.Visual();
                var currentScale = shapeVisual.Scale.X;

                // When scaling up, we first scale down the shape, and then increase their Width and Height so
                // they will be crystal sharp.
                if (_newTrackerScale > currentScale)
                {
                    var downScale = currentScale / _newTrackerScale;

                    shapeVisual.Scale = new Vector3(downScale, downScale, 0.0f);
                    Debug.WriteLine($"Updated scale: {shapeVisual.Scale}");

                    shape.Height = originalHeight * _newTrackerScale;
                    Debug.WriteLine($"New Width: {shape.Width}");
                }
            }
        }

        public void CustomAnimationStateEntered(InteractionTracker sender, InteractionTrackerCustomAnimationStateEnteredArgs args)
        {
        }

        public void RequestIgnored(InteractionTracker sender, InteractionTrackerRequestIgnoredArgs args)
        {
        }

        #endregion

        #region Methods
        public static int GetRandomNumber(int min, int max) => Random.Next(min, max);

        private void CreateMoreShapes(int numberOfShapesToCreate = 10000)
        {
            for (var i = 0; i < numberOfShapesToCreate; i++)
            {
                var size = GetRandomNumber(4, 32);
                var x = GetRandomNumber(4, (int)_pathContainerVisual.Size.X);
                var y = GetRandomNumber(4, (int)_pathContainerVisual.Size.Y);

                var shape = new Rectangle
                {
                    Fill = new SolidColorBrush(Color.FromArgb(180, (byte)Random.Next(255), (byte)Random.Next(255), (byte)Random.Next(255))),
                    Width = size,
                    Height = size
                };

                Canvas.SetLeft(shape, x);
                Canvas.SetTop(shape, y);

                PathContainer.Children.Add(shape);
            }
        }

        #endregion
    }
}