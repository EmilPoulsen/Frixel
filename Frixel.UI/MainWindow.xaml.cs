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

namespace Frixel.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const double SliderMin = 1;
        const double SliderMax = 30;
        const double CanvasScaleFactor = .8;

        private double _xGridSize;
        private double _yGridSize;
        private bool _isRunning;
        private Tuple<Domain, Domain> _sliderMappingDomains;


        public MainWindow()
        {
            InitializeComponent();
            _sliderMappingDomains = new Tuple<Domain, Domain>(new Domain(0, 1), new Domain(SliderMin, SliderMax));
            _isRunning = false;
            _xGridSize = GridSize(sld_GridX.Value);
            _yGridSize = GridSize(sld_GridY.Value);
            DrawGridSize();
        }

        private double GridSize(double sliderValue)
        {
            // Slider domain is always 0 to 1.
            // Map the value to our domain of Slidermin to SliderMax
            return sliderValue.Map(_sliderMappingDomains.Item1, _sliderMappingDomains.Item2);
        }

        private void DrawGridSize()
        {
            this.tb_GridSize.Text = _xGridSize + "', " + _yGridSize + "'";
        }

        #region Events

        private void btn_RefGeo_Click(object sender, RoutedEventArgs e)
        {

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

        private void Redraw()
        {
            // Get canvas properties
            var canvasWidth = this.canv_Main.ActualWidth;
            var canvasHeight = this.canv_Main.ActualHeight;

            // Get domain of our collection of points in x and Y
            // TODO TODO TODO

            // Scale our lines using the domain


            // Draw a bunch of lines from rhino
            var lin = new Line();
            lin.X
        }

        private void 

        #endregion

        // Pseudocode
        // 1 Reference geometry - Anytime
        // 1 Set up grid spacing - Anytime
        //      Update draw for grid lines
        //      Clear and update solver
    }
}
