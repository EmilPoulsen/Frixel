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


        private double _xGridSize;
        private double _yGridSize;
        private bool _isRunning;
        private bool _isRedrawing;
        private Tuple<Domain, Domain> _sliderMappingDomains;

        private PixelStructure _pixelStructure;
        private List<Line2d> _actualMassingOutline;


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
            SetReferenceData(refData);
            this.Redraw();
        }

        private void sld_GridX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _xGridSize = GridSize(sld_GridX.Value);
            DrawGridSize();
        }

        private void sld_GridY_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _yGridSize = GridSize(sld_GridY.Value);
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

        private void SetReferenceData(FrixelReferenceData refData)
        {
            this._pixelStructure = refData.Structure;
            this._actualMassingOutline = refData.ActualShape;
        }

        private void Redraw()
        {
            // Set state
            this._isRedrawing = true;

            // Clear canvas
            ClearCanvas();

            // If canvas is too small fuck off
            if (CanvasIsSmall()) { this._isRedrawing = false; return; }

            // Get canvas properties
            var canvasWidth = this.canv_Main.ActualWidth;
            var canvasHeight = this.canv_Main.ActualHeight; // Test height

            // Get domain of our collection of points in x and Y
            Domain2d pxlSDomain = _pixelStructure.Nodes.GetBoundingBox();

            // Get canvas ready lines
            var canvasDomain = new Domain2d(
                new Domain(canvasWidth - CanvasMargin, 0 + CanvasMargin),
                new Domain(canvasHeight - CanvasMargin, 0 + CanvasMargin)
                );

            List<Line> pxsLines = _pixelStructure.GetAllLInes().Select(l =>
            {
                return l.Map(pxlSDomain, canvasDomain).ToCanvasLine(Brushes.Black);
            }).ToList();
            List<Line> actualMassingLInes = _actualMassingOutline.Select(l =>
            {
                return l.Map(pxlSDomain, canvasDomain).ToCanvasLine(Brushes.Red);
            }).ToList();

            // Add lines to canvas 
            pxsLines.ForEach(l => canv_Main.Children.Add(l));
            actualMassingLInes.ForEach(l => canv_Main.Children.Add(l));

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
