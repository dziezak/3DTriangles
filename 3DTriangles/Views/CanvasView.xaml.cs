using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Numerics;
using System.Collections.Generic;
using _3DTriangles.Models;
using _3DTriangles.Services;

namespace BezierVisualizer.Views
{
    public partial class CanvasView : UserControl
    {
        private List<Triangle> _triangles;
        private bool _showBezierPolygon;
        private bool _showTriangleMesh;
        private bool _showFilledTriangles;
        private MainWindow _main;

        public CanvasView()
        {
            InitializeComponent();
            Loaded += UserControl_Loaded;
        }

        public void SetTriangles(List<Triangle> triangles, bool showBezierPolygon, bool showTriangleMesh, bool showFilledTriangles)
        {
            _triangles = triangles;
            _showBezierPolygon = showBezierPolygon;
            _showTriangleMesh = showTriangleMesh;
            _showFilledTriangles = showFilledTriangles;

            Draw();
        }

        private void Draw()
        {
            DrawCanvas.Children.Clear();
            double centerX = DrawCanvas.ActualWidth / 2;
            double centerY = DrawCanvas.ActualHeight / 2;

            if (_showTriangleMesh)
            {
                foreach (var tri in _triangles)
                {
                    var poly = new Polygon
                    {
                        Stroke = Brushes.White,
                        StrokeThickness = 0.5,
                        Fill = Brushes.Transparent,
                        Points = new PointCollection
                        {
                            ToCanvas(tri.V0.PRot, centerX, centerY),
                            ToCanvas(tri.V1.PRot, centerX, centerY),
                            ToCanvas(tri.V2.PRot, centerX, centerY)
                        }
                    };
                    DrawCanvas.Children.Add(poly);
                }
            }

            if (_showBezierPolygon)
            {
                var controlPoints = MeshBuilder.GetControlPolygon();

                // Obrót punktów kontrolnych
                float alfa = (float)_main.AlfaSlider.Value;
                float beta = (float)_main.BetaSlider.Value;

                Matrix4x4 rotX = Matrix4x4.CreateRotationX(MathF.PI * alfa / 180f);
                Matrix4x4 rotZ = Matrix4x4.CreateRotationZ(MathF.PI * beta / 180f);
                Matrix4x4 rot = rotZ * rotX;

                // Zakładamy, że punkty są w kolejności [i,j] → 4x4 = 16
                Vector3[,] grid = new Vector3[4, 4];
                var rawPoints = MeshBuilder.GetControlPolygon();
                int index = 0;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        grid[i, j] = Vector3.Transform(rawPoints[index++], rot);
                    }
                }

// Rysuj punkty
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        var p = grid[i, j];
                        var ellipse = new Ellipse
                        {
                            Width = 4,
                            Height = 4,
                            Fill = Brushes.Red
                        };
                        Canvas.SetLeft(ellipse, centerX + p.X * 100 - 2);
                        Canvas.SetTop(ellipse, centerY - p.Y * 100 - 2);
                        DrawCanvas.Children.Add(ellipse);
                    }
                }

// Rysuj linie wzdłuż wierszy
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var p1 = grid[i, j];
                        var p2 = grid[i, j + 1];
                        var line = new Line
                        {
                            X1 = centerX + p1.X * 100,
                            Y1 = centerY - p1.Y * 100,
                            X2 = centerX + p2.X * 100,
                            Y2 = centerY - p2.Y * 100,
                            Stroke = Brushes.Gray,
                            StrokeThickness = 1
                        };
                        DrawCanvas.Children.Add(line);
                    }
                }

// Rysuj linie wzdłuż kolumn
                for (int j = 0; j < 4; j++)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var p1 = grid[i, j];
                        var p2 = grid[i + 1, j];
                        var line = new Line
                        {
                            X1 = centerX + p1.X * 100,
                            Y1 = centerY - p1.Y * 100,
                            X2 = centerX + p2.X * 100,
                            Y2 = centerY - p2.Y * 100,
                            Stroke = Brushes.Gray,
                            StrokeThickness = 1
                        };
                        DrawCanvas.Children.Add(line);
                    }
                }

            }

            if (_showFilledTriangles)
            {
                // ⏳ Rasteryzacja trójkątów — do zaimplementowania później
            }
        }

        private Point ToCanvas(Vector3 p, double cx, double cy)
        {
            return new Point(cx + p.X * 100, cy - p.Y * 100);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _main = Window.GetWindow(this) as MainWindow;
            Draw();
        }
    }
}
