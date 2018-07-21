using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Frixel.Core;
using Frixel.Core.Extensions;
using Frixel.Core.Geometry;

namespace Frixel.UI
{
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
        public static event PixelStructurePass ReferenceFromRhino;

        public delegate FrixelReferenceData PixelStructureUpdate(double xSize, double ySize);
        public static event PixelStructureUpdate UpdateRhino;


        private double _xGridSize;
        private double _yGridSize;
        private bool _isRunning;
        private bool _isRedrawing;
        private Tuple<Domain, Domain> _sliderMappingDomains;

        private PixelStructure _pixelStructure;
        private List<Line2d> _actualMassingOutline;
        private Domain2d _massingDomain;


        public MainWindow(PixelStructure pixelStructure)
        {
            InitializeComponent();
            _pixelStructure = pixelStructure;
            _sliderMappingDomains = new Tuple<Domain, Domain>(new Domain(0, 1), new Domain(SliderMin, SliderMax));
            _isRunning = false;
            _xGridSize = GridSize(sld_GridX.Value);
            _yGridSize = GridSize(sld_GridY.Value);
            _isRedrawing = false;
            DrawGridSize();
            Redraw();
        }

        public MainWindow()
        {
            InitializeComponent();
            _sliderMappingDomains = new Tuple<Domain, Domain>(new Domain(0, 1), new Domain(SliderMin, SliderMax));
            _isRunning = false;
            _xGridSize = GridSize(sld_GridX.Value);
            _yGridSize = GridSize(sld_GridY.Value);
            _isRedrawing = false;
            DrawGridSize();
        }

        private double GridSize(double sliderValue)
        {
            // Slider domain is always 0 to 1.
            // Map the value to our domain of Slidermin to SliderMax
            return sliderValue.MapRound(_sliderMappingDomains.Item1, _sliderMappingDomains.Item2);
        }

        private void DrawGridSize()
        {
            this.tb_GridSize.Text = _xGridSize + "', " + _yGridSize + "'";
        }

        #region Events

        private void btn_RefGeo_Click(object sender, RoutedEventArgs e)
        {
            var refData = ReferenceFromRhino(_xGridSize,_yGridSize);
            if(refData == null) { return; }

            SetUpdated(refData);
            this.Redraw();
        }

        private void sld_GridX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _xGridSize = GridSize(sld_GridX.Value);
            var updated = UpdateRhino(_xGridSize, _yGridSize);
            SetUpdated(updated);
            this.Redraw();
            DrawGridSize();
        }

        private void sld_GridY_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _yGridSize = GridSize(sld_GridY.Value);
            var updated = UpdateRhino(_xGridSize, _yGridSize);
            SetUpdated(updated);
            this.Redraw();
            DrawGridSize();
        }

        private void sld_WindLoad_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //throw new NotImplementedException();
        }

        private void sld_GravLoad_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //throw new NotImplementedException();
        }


        #endregion

        private void SetUpdated(FrixelReferenceData refData)
        {
            this._pixelStructure = refData.Structure;
            this._actualMassingOutline = refData.ActualShape;
            this._massingDomain = refData.BoundingBox;
        }

        private void Redraw()
        {

            // Set state
            this._isRedrawing = true;

            // Clear canvas
            ClearCanvas();

            // If canvas is too small fuck off
            if (CanvasIsSmall()) { return; }
            if (this._pixelStructure == null | this._actualMassingOutline == null) { return; }

            // Get canvas properties
            var canvasWidth = this.canv_Main.ActualWidth;
            var canvasHeight = this.canv_Main.ActualHeight; // Test height

            // Get domain of our collection of points in x and Y
            //Domain2d pxlSDomain = _pixelStructure.Nodes.GetBoundingBox();
            Domain2d pxlSDomain = this._massingDomain;

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

            List<Line> pxsLines = _pixelStructure.GetAllLInes().Select(l =>
            {
                return l.Map(pxlSDomain, canvasDomain).ToCanvasLine(Brushes.Gray);
            }).ToList();
            List<Line> actualMassingLInes = _actualMassingOutline.Select(l =>
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

            // Set state
            this._isRedrawing = false;
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

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Redraw();
        }


        // Pseudocode
        // 1 Reference geometry - Anytime
        // 1 Set up grid spacing - Anytime
        //      Update draw for grid lines
        //      Clear and update solver
    }
}
