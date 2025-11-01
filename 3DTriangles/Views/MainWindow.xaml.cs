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
    }
}