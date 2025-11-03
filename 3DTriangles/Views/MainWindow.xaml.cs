using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using _3DTriangles.Models;
using _3DTriangles.Services;
using BezierVisualizer.Views;

namespace BezierVisualizer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateSliderLabels();
            RedrawScene();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsLoaded) return;
            UpdateSliderLabels();
            RedrawScene();
        }

        private void Checkbox_Changed(object sender, RoutedEventArgs e)
        {
            RedrawScene();
        }

        private void UpdateSliderLabels()
        {
            AlfaValue.Text = ((int)AlfaSlider.Value).ToString();
            BetaValue.Text = ((int)BetaSlider.Value).ToString();
            ResolutionValue.Text = ((int)ResolutionSlider.Value).ToString();

            KdValue.Text = KdSlider.Value.ToString("0.00");
            KsValue.Text = KsSlider.Value.ToString("0.00");
            MValue.Text = MSlider.Value.ToString("0.00");
        }

        private void RedrawScene()
        {
            float alfa = (float)AlfaSlider.Value;
            float beta = (float)BetaSlider.Value;
            int resolution = (int)ResolutionSlider.Value;

            var surface = FileLoader.LoadSurface("Resources/surface.txt");
            var triangles = MeshBuilder.GenerateMesh(surface, resolution);

            foreach (var tri in triangles)
            {
                tri.V0.Rotate(alfa, beta);
                tri.V1.Rotate(alfa, beta);
                tri.V2.Rotate(alfa, beta);
            }

            CanvasArea.SetTriangles(
                triangles,
                showBezierPolygon: ShowBezierPolygon.IsChecked == true,
                showTriangleMesh: ShowTriangleMesh.IsChecked == true,
                showFilledTriangles: ShowFilledTriangles.IsChecked == true,
                kd: (float)KdSlider.Value,
                ks: (float)KsSlider.Value,
                m: (int)MSlider.Value
            );
        }
    }
}