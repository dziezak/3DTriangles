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
        
        private float _kd;
        private float _ks;
        private int _m;


        public CanvasView()
        {
            InitializeComponent();
            Loaded += UserControl_Loaded;
        }

        public void SetTriangles(List<Triangle> triangles, bool showBezierPolygon, bool showTriangleMesh, bool showFilledTriangles, float kd, float ks, int m)
        {
            _triangles = triangles;
            _showBezierPolygon = showBezierPolygon;
            _showTriangleMesh = showTriangleMesh;
            _showFilledTriangles = showFilledTriangles;
            _kd = kd;
            _ks = ks;
            _m = m;


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
                foreach (var tri in _triangles)
                {
                    RasterizeTriangle(tri, centerX, centerY);
                }
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
        
        private void RasterizeTriangle(Triangle tri, double centerX, double centerY)
        {
            // Rzutuj wierzchołki na 2D
            var p0 = ToCanvas(tri.V0.PRot, centerX, centerY);
            var p1 = ToCanvas(tri.V1.PRot, centerX, centerY);
            var p2 = ToCanvas(tri.V2.PRot, centerX, centerY);

            // Znajdź bounding box
            int minY = (int)Math.Floor(Math.Min(p0.Y, Math.Min(p1.Y, p2.Y)));
            int maxY = (int)Math.Ceiling(Math.Max(p0.Y, Math.Max(p1.Y, p2.Y)));

            for (int y = minY; y <= maxY; y++)
            {
                List<double> xIntersections = new();

                // Sprawdź przecięcia z każdą krawędzią
                AddEdgeIntersection(p0, p1, y, xIntersections);
                AddEdgeIntersection(p1, p2, y, xIntersections);
                AddEdgeIntersection(p2, p0, y, xIntersections);

                if (xIntersections.Count < 2) continue;

                xIntersections.Sort();
                int xStart = (int)Math.Floor(xIntersections[0]);
                int xEnd = (int)Math.Ceiling(xIntersections[1]);

                for (int x = xStart; x <= xEnd; x++)
                {
                    var pixel = new Point(x, y);
                    var bary = ComputeBarycentric(pixel, p0, p1, p2);
                    if (bary == null) continue;

                    float l0 = bary.Value.X;
                    float l1 = bary.Value.Y;
                    float l2 = bary.Value.Z;

                    // Interpoluj normalną
                    Vector3 N = Vector3.Normalize(
                        tri.V0.NRot * l0 + tri.V1.NRot * l1 + tri.V2.NRot * l2);

                    // Interpoluj kolor obiektu (IO) — na razie stały
                    Vector3 IO = new(1, 1, 1); // biały

                    // Światło
                    Vector3 L = Vector3.Normalize(new Vector3(0, 0, 1)); // kierunek do światła
                    Vector3 V = new(0, 0, 1);
                    Vector3 R = Vector3.Normalize(2 * Vector3.Dot(N, L) * N - L);

                    float kd = _kd;
                    float ks = _ks;
                    int m = _m;
                    Vector3 IL = new(1, 1, 1); // kolor światła

                    float cosNL = MathF.Max(0, Vector3.Dot(N, L));
                    float cosVR = MathF.Max(0, Vector3.Dot(V, R));
                    float specular = MathF.Pow(cosVR, m);

                    Vector3 I = kd * IL * IO * cosNL + ks * IL * IO * specular;

                    byte r = (byte)Math.Min(255, I.X * 255);
                    byte g = (byte)Math.Min(255, I.Y * 255);
                    byte b = (byte)Math.Min(255, I.Z * 255);

                    var rect = new Rectangle
                    {
                        Width = 1,
                        Height = 1,
                        Fill = new SolidColorBrush(Color.FromRgb(r, g, b))
                    };
                    Canvas.SetLeft(rect, x);
                    Canvas.SetTop(rect, y);
                    DrawCanvas.Children.Add(rect);
                }
            }
        }
        private void AddEdgeIntersection(Point a, Point b, int y, List<double> list)
        {
            if ((y < a.Y && y < b.Y) || (y > a.Y && y > b.Y) || (a.Y == b.Y)) return;

            double t = (y - a.Y) / (b.Y - a.Y);
            double x = a.X + t * (b.X - a.X);
            list.Add(x);
        }

        private Vector3? ComputeBarycentric(Point p, Point a, Point b, Point c)
        {
            float denom = (float)((b.Y - c.Y) * (a.X - c.X) + (c.X - b.X) * (a.Y - c.Y));
            if (Math.Abs(denom) < 1e-5) return null;

            float l0 = (float)((b.Y - c.Y) * (p.X - c.X) + (c.X - b.X) * (p.Y - c.Y)) / denom;
            float l1 = (float)((c.Y - a.Y) * (p.X - c.X) + (a.X - c.X) * (p.Y - c.Y)) / denom;
            float l2 = 1 - l0 - l1;

            if (l0 < 0 || l1 < 0 || l2 < 0) return null;

            return new Vector3(l0, l1, l2);
        }


    }
}
