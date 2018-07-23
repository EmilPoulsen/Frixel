using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Frixel.Core;
using Frixel.Core.Extensions;
using Frixel.Core.Geometry;

namespace Frixel.UI
{

    enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Real world value of our sliders min (m)
        /// </summary>
        const double SliderMin = 5;
        /// <summary>
        /// Real world value of our sliders max (m)
        /// </summary>
        const double SliderMax = 30;
        /// <summary>
        /// Margin of canvas in pixels
        /// </summary>
        const double CanvasMargin = 60;

        public delegate FrixelReferenceData PixelStructurePass(double xSize, double ySize);
        public static event PixelStructurePass ReferenceFromClient;

        public delegate FrixelReferenceData PixelStructureUpdate(double xSize, double ySize);
        public static event PixelStructureUpdate UpdateClient;

        public delegate void PixelStructureBake(PixelStructure pixelStructure);
        public static event PixelStructureBake BakeStructure;

        private double _xGridSize;
        private double _yGridSize;
        private bool _isRunning;
        private bool _isRedrawing;
        private Tuple<Domain, Domain> _sliderMappingDomains;
        private Optimizer.FrixelOptimizer _optimizer;

        private PixelStructure _pixelStructure;
        private List<Line2d> _actualOutline;
        private Domain2d _massingDomain;

        public static AnalysisResults AnalysisResults;
        private AnalysisSummary _analysisSummary;
        private double _windLoad;
        private List<double> _dispMap = new List<double>();

        private BackgroundWorker _bw = new BackgroundWorker();
        private bool _bwComplete = true;
        private int _completeGens = 0;
        private DisplayState _displayState = DisplayState.Default;
        private Direction _windDirection = Direction.Right;

        #region CTOR

        public MainWindow(PixelStructure pixelStructure)
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            _pixelStructure = pixelStructure;
            _sliderMappingDomains = new Tuple<Domain, Domain>(new Domain(0, 1), new Domain(SliderMin, SliderMax));
            _isRunning = false;
            _xGridSize = GridSize(sld_GridX.Value);
            _yGridSize = GridSize(sld_GridY.Value);
            _isRedrawing = false;
            DrawGridSize();
            Redraw();
            Subscribe();
        }

        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            this.Opacity = 0;
            this.grd_FrixelSplash.Visibility = Visibility.Visible;
            this.grd_FrixelSplash.Opacity = 1;
            _sliderMappingDomains = new Tuple<Domain, Domain>(new Domain(0, 1), new Domain(SliderMin, SliderMax));
            _isRunning = false;
            _xGridSize = GridSize(sld_GridX.Value);
            _yGridSize = GridSize(sld_GridY.Value);
            _isRedrawing = false;
            DrawGridSize();
            Subscribe();
        }

        private void Subscribe()
        {
            _optimizer = new Optimizer.FrixelOptimizer();
            _optimizer.RanIteration += _optimizer_RanIteration;
            _bw.DoWork += _bw_DoWork;
            _bw.WorkerSupportsCancellation = true;
            _bw.RunWorkerCompleted += _bw_RunWorkerCompleted;
        }

        private void Unsubscribe()
        {
            _optimizer.RanIteration -= _optimizer_RanIteration;
            _bw.DoWork -= _bw_DoWork;
            _bw.RunWorkerCompleted -= _bw_RunWorkerCompleted;
        }

        #endregion

        #region Overrides

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Unsubscribe();
        }

        #endregion

        #region Background Worker

        private void _bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Get a beer
        }

        private void _bw_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!_bwComplete)
            {
                _optimizer.Optimize(_pixelStructure);
                _bwComplete = true;
            }
        }

        #endregion

        #region Optimizer

        private void _optimizer_RanIteration(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // Ran iteration
                var args = e as Optimizer.FrixelEventArgs;
                if (args.AnalysisResults == null) { return; }
                MainWindow.AnalysisResults = args.AnalysisResults;
                UpdateBracing();
                Redisplace();
                Redraw();
                _completeGens++;
                tb_Generations.Text = "completed " + _completeGens + " generations";
            });

        }

        private void UpdateBracing()
        {
            var shouldbebraced = MainWindow.AnalysisResults.PixelResults.Select(p => p.Value.IsBraced).ToList();
            for (int i = 0; i < this._pixelStructure.Pixels.Count; i++)
            {
                if (shouldbebraced[i])
                {
                    this._pixelStructure.Pixels[i].ChangeStateTo(PixelState.Moment);
                }
            }
        }

        #endregion

        #region UI Effects

        public void ShowWindow()
        {
            this.Opacity = 0;
            this.Visibility = Visibility.Visible;
            this.WindowFadeIn(this, 400);
        }

        private void ShowSplash()
        {
            this.grd_FrixelSplash.Visibility = Visibility.Visible;
            this.SplashControlFade(this.grd_FrixelSplash, 400, 3);
        }

        private void ShowWarningMessage()
        {
            this.grd_Message.Visibility = Visibility.Visible;
            this.grd_Message.Opacity = 1;
        }

        private void HideWarningMessage()
        {
            this.HideWarningMessageFade(this.grd_Message, 1000, 3);
        }

        private void WindowFadeIn(FrameworkElement control, int speed = 300, int lingerTime = 2)
        {
            Storyboard storyboard = new Storyboard();
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, speed);
            storyboard.Completed += Window_Completed_Fade; ;

            var fadeInAnimation = new DoubleAnimation { From = 0, To = 1, Duration = new Duration(new TimeSpan(0, 0, 0, 0, 300)) };
            fadeInAnimation.BeginTime = new TimeSpan(0, 0, 0);
            Storyboard.SetTargetName(fadeInAnimation, control.Name);
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath("Opacity", 0));
            storyboard.Children.Add(fadeInAnimation);

            storyboard.Begin(control);
        }

        private void ShowWarningMessageFade(FrameworkElement control, int speed = 300)
        {
            Storyboard storyboard = new Storyboard();
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, speed);

            var fadeInAnimation = new DoubleAnimation { From = 0, To = 1, Duration = new Duration(new TimeSpan(0, 0, 0, 0, 300)) };
            fadeInAnimation.BeginTime = new TimeSpan(0, 0, 0);
            Storyboard.SetTargetName(fadeInAnimation, control.Name);
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath("Opacity", 0));
            storyboard.Children.Add(fadeInAnimation);

            storyboard.Begin(control);
        }

        private void HideWarningMessageFade(FrameworkElement control, int speed = 300, int lingerTime = 2)
        {
            Storyboard storyboard = new Storyboard();
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, speed);
            storyboard.Completed += WarningMessageCompleted; ; ;

            var fadeOutAnimation = new DoubleAnimation { From = 1.0, To = 0, Duration = new Duration(duration) };
            fadeOutAnimation.BeginTime = new TimeSpan(0, 0, lingerTime);
            Storyboard.SetTargetName(fadeOutAnimation, control.Name);
            Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath("Opacity", 0));
            storyboard.Children.Add(fadeOutAnimation);

            storyboard.Begin(control);
        }

        private void WarningMessageCompleted(object sender, EventArgs e)
        {
            this.grd_Message.Visibility = Visibility.Hidden;
        }

        private void Window_Completed_Fade(object sender, EventArgs e)
        {
            ShowSplash();
        }

        private void SplashControlFade(FrameworkElement control, int speed = 300, int lingerTime = 2)
        {
            Storyboard storyboard = new Storyboard();
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, speed);
            storyboard.Completed += Storyboard_Completed;

            var fadeOutAnimation = new DoubleAnimation { From = 1.0, To = 0, Duration = new Duration(duration) };
            fadeOutAnimation.BeginTime = new TimeSpan(0, 0, lingerTime);
            Storyboard.SetTargetName(fadeOutAnimation, control.Name);
            Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath("Opacity", 0));
            storyboard.Children.Add(fadeOutAnimation);

            storyboard.Begin(control);
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            this.grd_FrixelSplash.Visibility = Visibility.Hidden;
        }

        #endregion

        #region Events

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Redraw();
        }

        private void btn_RefGeo_Click(object sender, RoutedEventArgs e)
        {
            var refData = ReferenceFromClient(_xGridSize, _yGridSize);
            if (refData == null) { return; }

            SetUpdated(refData);
            this.Redraw();
        }

        private void sld_GridX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _xGridSize = GridSize(sld_GridX.Value);
            this.ChangeDisplayState(DisplayState.Default);
            var updated = UpdateClient(_xGridSize, _yGridSize);
            SetUpdated(updated);
            this.Redraw();
            DrawGridSize();
        }

        private void sld_GridY_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _yGridSize = GridSize(sld_GridY.Value);
            this.ChangeDisplayState(DisplayState.Default);
            var updated = UpdateClient(_xGridSize, _yGridSize);
            SetUpdated(updated);
            this.Redraw();
            DrawGridSize();
        }

        private void sld_WindLoad_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            tb_WindLoad.Text = Math.Round(sld_WindLoad.Value.Map(new Domain(0, 1), new Domain(0, 10))).ToString();
            if (AnalysisResults == null) { return; }
            else { AnalyzeAndRedraw(); }
        }

        private void sld_GravLoad_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            tb_GravLoad.Text = Math.Round(sld_GravLoad.Value.Map(new Domain(0, 1), new Domain(0, 10))).ToString();
            if (AnalysisResults == null) { return; }
            Redisplace();
            Redraw();

        }

        private void btn_Run_Click(object sender, RoutedEventArgs e)
        {
            AnalyzeAndRedraw();
        }

        private void btn_Reset_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.AnalysisResults = null;
            var update = UpdateClient(_xGridSize, _yGridSize);
            SetUpdated(update);
            Redraw();
        }

        private void btn_BraceAll_Click(object sender, RoutedEventArgs e)
        {
            BraceAll();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void tb_Close_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void btn_Optimize_Click(object sender, RoutedEventArgs e)
        {
            if (_bw.IsBusy)
            {
                _bw.CancelAsync();
                this.btn_Optimize.Content = "Optimize";
                _bwComplete = true;
            }
            else
            {
                // First analye
                AnalyzeAndRedraw();
                tb_Generations.Text = "";
                _completeGens = 0;
                _bwComplete = false;
                _bw.RunWorkerAsync();
                this.btn_Optimize.Content = "Optimize";
            }
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            // Emit event to client to bake with current structure
            BakeStructure(this._pixelStructure);
        }

        private void canv_Main_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // If there is nothing loaded into the window, return early
            if(this._pixelStructure == null) { return; }

            // Get the pixel space coordinates of the click
            var location = ScreenSpaceToPixelSpace(e.GetPosition(canv_Main));
            switch (this._displayState)
            {
                case DisplayState.Analytical:
                    {
                        if (!this._pixelStructure.ChangeBracingAtLocation(location, true)) return;
                        AnalyzeAndRedraw();
                        break;
                    }
                case DisplayState.Default:
                    {
                        if (!this._pixelStructure.ChangeBracingAtLocation(location, false)) return;
                        break;
                    }
            }
            // If there hasn't been an early return, then there is a change to be rendered
            Redraw();
        }

        private void tb_WindDir_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_windDirection == Direction.Right)
            {
                _windDirection = Direction.Left;
                tb_WindDir.Text = "←";
            }
            else if (_windDirection == Direction.Left)
            {
                _windDirection = Direction.Right;
                tb_WindDir.Text = "→";
            }
            if(_displayState == DisplayState.Analytical) { AnalyzeAndRedraw(); }
        }

        private void Rb_View_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)this.rb_DefaultView.IsChecked) { ChangeDisplayState(DisplayState.Default, true); }
            if ((bool)this.rb_AnalyticalView.IsChecked) {
                ChangeDisplayState(DisplayState.Analytical, true);
                if(this._pixelStructure != null) { AnalyzeAndRedraw(); }
            }
        }

        #endregion

        private double GridSize(double sliderValue)
        {
            // Slider domain is always 0 to 1.
            // Map the value to our domain of Slidermin to SliderMax
            return sliderValue.MapRound(_sliderMappingDomains.Item1, _sliderMappingDomains.Item2);
        }

        private void DrawGridSize()
        {
            this.tb_gridX.Text = _xGridSize + "'";
            this.tb_gridY.Text = _yGridSize + "'";
        }

        private void SetUpdated(FrixelReferenceData refData)
        {
            if (refData == null) { return; }
            this._pixelStructure = refData.Structure;
            this._actualOutline = refData.ActualShape;
            this._massingDomain = refData.BoundingBox;
            this.tb_GridSize.Text = refData.ActualXSize.RoundTo(2) + "', " + refData.ActuveYSize.RoundTo(2) + "'";
        }

        private Domain2d GetCanvasDomain()
        {
            // Get canvas properties
            var canvasWidth = this.canv_Main.ActualWidth;
            var canvasHeight = this.canv_Main.ActualHeight; // Test height

            // Get canvas ready lines
            var canvasDomain = new Domain2d(
                new Domain(0 + CanvasMargin, canvasWidth - CanvasMargin),
                new Domain(canvasHeight - CanvasMargin, 0 + CanvasMargin)
                );

            // Scale the canvas domain to the aspect ratio of the input domain
            if (_massingDomain.X.IsLargerThan(_massingDomain.Y))
            {
                double YscaleFactor = _massingDomain.AspectRatioY;
                canvasDomain.Y.ScaleMid(YscaleFactor);
            }
            else
            {
                double XscaleFactor = _massingDomain.AspectRatioX;
                canvasDomain.Y.ScaleMid(XscaleFactor);
            }

            return canvasDomain;
        }

        private void Redraw()
        {
            // Early returns if canvas is too small or no structure loaded
            if (CanvasIsSmall()) { return; }
            if (this._pixelStructure == null | this._actualOutline == null) { return; }

            // Set state
            this._isRedrawing = true;

            // Render analysis colors
            bool renderAnalColors = this._displayState == DisplayState.Analytical;

            // Render displacement?
            bool renderDisplacement = this._pixelStructure.HasAnalysisValues();

            // Clear canvas
            ClearCanvas();

            // Get domains for canvas and point collection
            Domain2d pxlSDomain = this._massingDomain;
            Domain2d canvasDomain = GetCanvasDomain();

            List<Line> pxsLines = _pixelStructure.GetAllLines(renderDisplacement).Select(l =>
            {
                return l.Map(pxlSDomain, canvasDomain).ToCanvasLine(Brushes.Gray);
            }).ToList();

            if (renderDisplacement)
            {
                // Add analysis colors
                List<double> edgeMap = new List<double>();
                _pixelStructure.GetAllEdges().ForEach(e =>
                {
                    edgeMap.Add(_dispMap[e.Start] + _dispMap[e.End]);
                });
                var startingDomain = new Domain(
                    edgeMap.Min(),
                    edgeMap.Max()
                    );
                var redDomain = new Domain(0, 255);
                int i = 0;
                _pixelStructure.ClearAllEdgeColors();
                pxsLines.ForEach(l =>
                {
                    try
                    {
                        var edgeColor = new Core.Display.Color(255, System.Convert.ToInt32(edgeMap[i].Map(startingDomain, redDomain)), 0, 0);
                        _pixelStructure.AllEdgeColors.Add(edgeColor);
                        if (renderAnalColors) { l.Stroke = new SolidColorBrush(edgeColor.ToMediaColor()); }
                        else { l.Stroke = Brushes.Black; }
                    } catch
                    {
                        l.Stroke = Brushes.Black;
                        _pixelStructure.AllEdgeColors.Add(new Core.Display.Color(255, 0, 0, 0));
                    }

                    i++;
                });
            }

            List<Line> actualMassingLInes = _actualOutline.Select(l =>
            {
                return l.Map(pxlSDomain, canvasDomain).ToCanvasLine(Brushes.LightBlue);
            }).ToList();

            // Add lines to canvas 
            pxsLines.ForEach(l => canv_Main.Children.Add(l));
            actualMassingLInes.ForEach(l => canv_Main.Children.Add(l));

            // Render support points
            List<Rectangle> supports = _pixelStructure.Nodes.Where(n => n.IsLocked).ToList().Select(n =>
            {
                return n.Map(pxlSDomain, canvasDomain).ToCanvasRect(20, Brushes.Blue);
            }).ToList();
            supports.ForEach(r =>
            {
                int index = canv_Main.Children.Add(r);
            });

            // Update summary
            if(this._pixelStructure != null && MainWindow.AnalysisResults != null)
            {
                RenderAnalysisSummary();
            }
            else { }

            // Set state
            this._isRedrawing = false;
        }

        private void RenderAnalysisSummary()
        {
            if(this._analysisSummary == null)
            {
                this._analysisSummary = new AnalysisSummary(MainWindow.AnalysisResults, this._pixelStructure);
            }
            tb_DispMin.Text = "Displacement Min: " + _analysisSummary.MinDisplacement.RoundTo(2);
            tb_DispMax.Text = "Displacement Max: " + _analysisSummary.MaxDisplacement.RoundTo(2);
            tb_ElementCt.Text = "Elements: " + _analysisSummary.Elements;
            tb_ConnCt.Text = "Connections: " + _analysisSummary.Connections;
            tb_SuppCt.Text = "Supports: " + _analysisSummary.Supports;
            tb_NetLen.Text = "Net Length: " + _analysisSummary.NetLength.RoundTo(2);
        }

        private bool CanvasIsSmall()
        {
            return (this.canv_Main.ActualWidth < CanvasMargin * 4
               | this.canv_Main.ActualHeight < CanvasMargin * 4);
        }

        private void ClearCanvas()
        {
            this.canv_Main.Children.Clear();
        }

        private void AnalyzeAndRedraw()
        {
            // Return if fucked
            if (_pixelStructure == null) { return; }

            // Update state
            this.ChangeDisplayState(DisplayState.Analytical);

            // Set stuff
            _pixelStructure.GravityLoad.Activated = true;
            _pixelStructure.GravityLoad.Amplification = 5000;

            if (this.sld_WindLoad.Value != null)
            {
                _pixelStructure.WindLoad.Activated = true;
                double windDir = (_windDirection == Direction.Left ? -1 : 1);
                _pixelStructure.WindLoad.Direction = new Point2d(windDir * 500000 * sld_WindLoad.Value, 0);
                _pixelStructure.WindLoad.NodeIndices = _pixelStructure.Nodes.Select(n => _pixelStructure.Nodes.IndexOf(n)).ToList();
            }

            // Run an analysis
            try
            {
                var anal = new Optimizer.FrixelAnalyzer();
                MainWindow.AnalysisResults = anal.Analyze(_pixelStructure);
                Redisplace();
                Redraw();
            }
            catch (Exception asd)
            {
                // Show warning message
                ShowWarningMessage();

                try
                {
                    BraceAll();
                    var anal = new Optimizer.FrixelAnalyzer();
                    MainWindow.AnalysisResults = anal.Analyze(_pixelStructure);
                    Redisplace();
                    Redraw();
                }
                catch (Exception fuckit)
                {
                    this.ChangeDisplayState(DisplayState.Default,true);
                    MainWindow.AnalysisResults = null;
                }
                HideWarningMessage();

                // LJ PRINT ANALYTICAL VALUES 
                //if (MainWindow.AnalysisResults == null) { return; }
                //lb_Results.Items.Clear();
                //MainWindow.AnalysisResults.NodeResults.Select(r => r.Value.DispX.ToString() + "," + r.Value.DispY.ToString()).ToList().ForEach(r => lb_Results.Items.Add(r));
            }
        }

        private void Redisplace()
        {
            _pixelStructure.ResetDisp();
            _dispMap.Clear();
            for (int i = 0; i < _pixelStructure.Nodes.Count; i++)
            {
                _pixelStructure.DispNodes[i].X += AnalysisResults.NodeResults[i].DispX * sld_GravLoad.Value;
                _pixelStructure.DispNodes[i].Y += AnalysisResults.NodeResults[i].DispY * sld_GravLoad.Value;
                _dispMap.Add(AnalysisResults.NodeResults[i].DispX + AnalysisResults.NodeResults[i].DispY);
            }
        }

        private void BraceAll()
        {
            this._pixelStructure.Pixels.ForEach(p => p.ChangeStateTo(PixelState.Moment));
            Redraw();
        }

        private Point2d ScreenSpaceToPixelSpace(Point clickLocation)
        {
            Domain2d canvasDomain = GetCanvasDomain();
            var mappedPoint = clickLocation.ToPoint2d().Map(canvasDomain, this._massingDomain);
            return mappedPoint;            
        }

        private void ChangeDisplayState(DisplayState newState, bool redraw = false)
        {
            // Make sure the radio buttons refelct the current state
            switch (newState)
            {
                case DisplayState.Analytical:
                    if (rb_AnalyticalView.IsChecked != true) { rb_AnalyticalView.IsChecked = true; }
                    break;
                case DisplayState.Default:
                    if (rb_DefaultView.IsChecked != true) { rb_DefaultView.IsChecked = true; }
                    break;
            }
            // If the state hasn't changed (fluke) early return
            if (this._displayState == newState) { return; }
            this._displayState = newState;
            if (redraw) { Redraw(); }
        }

        private void DisplayAnalysisReuslts()
        {
            // LJ TODO
        }


    }

    enum DisplayState
    {
        Default,
        Analytical,
        Annotated
    }
}
