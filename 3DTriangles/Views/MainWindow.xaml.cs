using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using _3DTriangles.Models;
using _3DTriangles.Services;
using BezierVisualizer.Views;
using YourAppNamespace.Rendering;
using Color = System.Drawing.Color;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace BezierVisualizer
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _lightTimer;
        private double _lightAngle = 0;
        private Vector3 _lightColor = new Vector3(1f, 1f, 1f);


        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateSliderLabels();
            RedrawScene();
            
            LightAngleSlider.ValueChanged += LightAngleSlider_Changed;
            LightZSlider.ValueChanged += LightZSlider_Changed;
            _lightTimer = new DispatcherTimer();
            _lightTimer.Interval = TimeSpan.FromMilliseconds(30);
            _lightTimer.Tick += UpdateLightPosition;
            _lightTimer.Start();
        }
        
        private void PickLightColorButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ColorDialog();
            dialog.FullOpen = true;
            dialog.Color = System.Drawing.Color.White;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var c = dialog.Color;

                _lightColor = new Vector3(
                    c.R / 255f,
                    c.G / 255f,
                    c.B / 255f
                );

                CanvasArea.SetLightColor(_lightColor);
                LightColorPreview.Background = new SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B)
                );
                
                CanvasArea.Draw();
            }
        }
        
        private void UseNormalMap_Checked(object sender, RoutedEventArgs e)
        {
            if (CanvasArea != null)
            {
                CanvasArea.SetNormalMapUsage(true);
                RedrawScene();
            }
        }
        
        private void UseNormalMap_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CanvasArea != null)
            {
                CanvasArea.SetNormalMapUsage(false);
                RedrawScene();
            }
        }

        private void UseNormalBitMat_Checked(object sender, RoutedEventArgs e)
        {
            if (CanvasArea != null)
            {
                CanvasArea.SetNormalBitMapUsage(true);
                RedrawScene();
            }
        }

        private void UseNormalBitMap_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CanvasArea != null)
            {
                CanvasArea.SetNormalBitMapUsage(false);
                RedrawScene();
            }
        }

        
        private void LoadNormalBitMap_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Obrazy (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Wszystkie pliki|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var bitmap = new BitmapImage(new Uri(dialog.FileName));
                    var writable = new WriteableBitmap(bitmap);
                    CanvasArea.LoadNormalBitMap(writable);

                    MessageBox.Show("Mapa wektorów normalnych została wczytana poprawnie.",
                        "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd przy wczytywaniu mapy normalnych:\n{ex.Message}",
                        "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        
        private void LoadNormalMap_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Obrazy (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|Wszystkie pliki|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var bitmap = new BitmapImage(new Uri(dialog.FileName));
                    var writable = new WriteableBitmap(bitmap);

                    CanvasArea.LoadNormalMap(writable);

                    MessageBox.Show("Mapa normalnych została wczytana poprawnie.", 
                        "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd przy wczytywaniu mapy normalnych:\n{ex.Message}",
                        "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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

        private void LightAngleSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsLoaded || CanvasArea == null || AnimateLight == null || LightAngleValue == null)
            {
                Console.WriteLine("LightAngleSlider_Changed something is null");
                return;
            }

            if (AnimateLight.IsChecked == true)
            {
                Console.WriteLine("LightAngleSlider_Changed: Animation in session");
                return;
            }

            float angle = MathF.PI * (float)LightAngleSlider.Value / 180f;
            float radious = 1.5f;
            float x = radious * MathF.Cos(angle);
            float y = radious * MathF.Sin(angle);
            float z = CanvasArea.GetLightPosition().Z;

            CanvasArea.SetLightPosition(new Vector3(x, y, z));
            UpdateSliderLabels();
            RedrawScene();
        }

        private void LightZSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsLoaded || CanvasArea == null) return;

            float angleRad = (float)(LightAngleSlider.Value * Math.PI / 180.0);
            float radius = 1.5f;

            float x = radius * MathF.Cos(angleRad);
            float y = radius * MathF.Sin(angleRad);
            float z = (float)LightZSlider.Value;

            CanvasArea.SetLightPosition(new Vector3(x, y, z));

            LightZValue.Text = z.ToString("0.00");

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
            LightAngleValue.Text = $"{(int)LightAngleSlider.Value}°";
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
                UseNormalMap: UseNormalMap.IsChecked == true,
                UseNormalBitMap: UseNormalBitMap.IsChecked == true
            );
        }
    }
}