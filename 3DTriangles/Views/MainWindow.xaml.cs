using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Threading;
using _3DTriangles.Models;
using _3DTriangles.Services;
using BezierVisualizer.Views;

namespace BezierVisualizer
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _lightTimer;
        private double _lightAngle = 0;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateSliderLabels();
            RedrawScene();
            
            LightZSlider.ValueChanged += LightZSlider_Changed;
            _lightTimer = new DispatcherTimer();
            _lightTimer.Interval = TimeSpan.FromMilliseconds(30);
            _lightTimer.Tick += UpdateLightPosition;
            _lightTimer.Start();
        }
        
        private void UpdateLightPosition(object sender, EventArgs e)
        {
            if (CanvasArea == null || AnimateLight.IsChecked != true)
                return;

            _lightAngle += 0.05;
            float radius = 1.5f;
            float x = radius * MathF.Cos((float)_lightAngle);
            float y = radius * MathF.Sin((float)_lightAngle);
            float z = (float)LightZSlider.Value;

            CanvasArea.SetLightPosition(new Vector3(x, y, z));
            RedrawScene();
        }

        private void LightZSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsLoaded || CanvasArea == null || AnimateLight == null || LightZSlider == null)
            {
                Console.WriteLine("LightZSlider_Changed something is null");
                return;
            }

            if (AnimateLight.IsChecked == true)
            {
                Console.WriteLine("LightZSlider_Changed: Animation in session");
                return;
            }

            float z = (float)LightZSlider.Value;

            Vector3 current = CanvasArea.GetLightPosition();
            CanvasArea.SetLightPosition(new Vector3(current.X, current.Y, z));
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
            if (CanvasArea == null || !IsLoaded) return;
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
                m: (int)MSlider.Value,
                UseNormalMap: UseNormalMap.IsChecked == true
            );
        }
    }
}