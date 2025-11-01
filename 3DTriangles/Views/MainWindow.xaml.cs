using System.Windows;
using _3DTriangles.Models;
using BezierVisualizer.Views;
using System.Collections.Generic;
using _3DTriangles.Services;

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
            var surface = FileLoader.LoadSurface("Resources/surface.txt");
            var triangles = MeshBuilder.GenerateMesh(surface, resolution: 10);
            CanvasArea.SetTriangles(triangles);
        }
        
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsLoaded) return;

            float alfa = (float)AlfaSlider.Value;
            float beta = (float)BetaSlider.Value;
            int resolution = (int)ResolutionSlider.Value;

            var surface = FileLoader.LoadSurface("Resources/surface.txt");
            var triangles = MeshBuilder.GenerateMesh(surface, resolution);

            // Obrót powierzchni
            foreach (var tri in triangles)
            {
                tri.V0.Rotate(alfa, beta);
                tri.V1.Rotate(alfa, beta);
                tri.V2.Rotate(alfa, beta);
            }


            CanvasArea.SetTriangles(triangles);
        }

    }
}