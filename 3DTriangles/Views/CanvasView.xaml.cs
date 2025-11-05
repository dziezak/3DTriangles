
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

        private WriteableBitmap _bitmap;

        private bool _useNormalMap;
        private BitmapImage _normalMap;
        private byte[] _normalMapBytes;
        private int _normalMapStride;
        
        private ScaleTransform _scale = new ScaleTransform(1.0, 1.0);

        public CanvasView()
        {
            InitializeComponent();
            Loaded += UserControl_Loaded;
            PreviewMouseWheel += CanvasView_MouseWheel;


            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "john-cena.bmp");
            if (File.Exists(path))
            {
                _normalMap = new BitmapImage(new Uri(path, UriKind.Absolute));
                Console.WriteLine("Wczytano mapę normalnych: John Cena");
            }
            else
            {
                Console.WriteLine("Nie udało się znaleźć John Ceny, bo jest niewidzailny");
                _normalMap = new BitmapImage();
            }
            if (_normalMap != null)
            {
                int stride = _normalMap.PixelWidth * (_normalMap.Format.BitsPerPixel / 8);
                _normalMapBytes = new byte[stride * _normalMap.PixelHeight];
                _normalMap.CopyPixels(_normalMapBytes, stride, 0);
                _normalMapStride = stride;
                Console.WriteLine("Normal map loaded into memory buffer.");
            }

        }
        
        private void CanvasView_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
            _scale.ScaleX *= zoomFactor;
            _scale.ScaleY *= zoomFactor;
        }



        public void SetTriangles(List<Triangle> triangles, bool showBezierPolygon, bool showTriangleMesh,
            bool showFilledTriangles, float kd, float ks, int m, bool UseNormalMap)
        {
            _triangles = triangles;
            _showBezierPolygon = showBezierPolygon;
            _showTriangleMesh = showTriangleMesh;
            _showFilledTriangles = showFilledTriangles;
            _kd = kd;
            _ks = ks;
            _m = m;
            _useNormalMap = UseNormalMap;

            Draw();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _main = Window.GetWindow(this) as MainWindow;
            Draw();
        }

        private void Draw()
{
    if (ActualWidth <= 0 || ActualHeight <= 0 || _triangles == null)
        return;

    double centerX = ActualWidth / 2;
    double centerY = ActualHeight / 2;

    int width = (int)ActualWidth;
    int height = (int)ActualHeight;
    _bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);

    // --- WYPEŁNIANIE TRÓJKĄTÓW ---
    if (_showFilledTriangles)
    {
        _bitmap.Lock();
        unsafe
        {
            IntPtr pBackBuffer = _bitmap.BackBuffer;
            int stride = _bitmap.BackBufferStride;
            byte* pixels = (byte*)pBackBuffer;

            foreach (var tri in _triangles)
            {
                RasterizeTriangle(tri, centerX, centerY, pixels, stride, width, height);
            }
        }
        _bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
        _bitmap.Unlock();
    }

    var image = new Image
    {
        Source = _bitmap,
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Top,
        Stretch = Stretch.None
    };

    var grid = new Grid();
    grid.Children.Add(image);

    // --- RYSOWANIE SIATKI I PUNKTÓW ---
    if (_showTriangleMesh || _showBezierPolygon)
    {
        var overlay = new Canvas();

        if (_showTriangleMesh)
        {
            foreach (var tri in _triangles)
            {
                var poly = new System.Windows.Shapes.Polygon
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
                overlay.Children.Add(poly);
            }
        }

        if (_showBezierPolygon)
        {
            float alfa = (float)_main.AlfaSlider.Value;
            float beta = (float)_main.BetaSlider.Value;

            Matrix4x4 rotX = Matrix4x4.CreateRotationX(MathF.PI * alfa / 180f);
            Matrix4x4 rotZ = Matrix4x4.CreateRotationZ(MathF.PI * beta / 180f);
            Matrix4x4 rot = rotZ * rotX;

            Vector3[,] gridPoints = new Vector3[4, 4];
            var rawPoints = MeshBuilder.GetControlPolygon();
            int index = 0;
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    gridPoints[i, j] = Vector3.Transform(rawPoints[index++], rot);

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    var p = gridPoints[i, j];
                    var ellipse = new System.Windows.Shapes.Ellipse
                    {
                        Width = 4,
                        Height = 4,
                        Fill = Brushes.Red
                    };
                    Canvas.SetLeft(ellipse, centerX + p.X * 100 - 2);
                    Canvas.SetTop(ellipse, centerY - p.Y * 100 - 2);
                    overlay.Children.Add(ellipse);
                }

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 3; j++)
                    overlay.Children.Add(CreateLine(gridPoints[i, j], gridPoints[i, j + 1], centerX, centerY));

            for (int j = 0; j < 4; j++)
                for (int i = 0; i < 3; i++)
                    overlay.Children.Add(CreateLine(gridPoints[i, j], gridPoints[i + 1, j], centerX, centerY));
        }

        grid.Children.Add(overlay);
    }

    // --- OPAKUJ CAŁOŚĆ W TRANSFORMACJĘ SKALUJĄCĄ ---
    var zoomContainer = new Grid
    {
        RenderTransform = _scale,
        RenderTransformOrigin = new Point(0.5, 0.5),
        Children = { grid }
    };

    Content = zoomContainer;
}


        private System.Windows.Shapes.Line CreateLine(Vector3 p1, Vector3 p2, double cx, double cy)
        {
            return new System.Windows.Shapes.Line
            {
                X1 = cx + p1.X * 100,
                Y1 = cy - p1.Y * 100,
                X2 = cx + p2.X * 100,
                Y2 = cy - p2.Y * 100,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };
        }

        private Point ToCanvas(Vector3 p, double cx, double cy)
        {
            return new Point(cx + p.X * 100, cy - p.Y * 100);
        }

        private unsafe void RasterizeTriangle(Triangle tri, double centerX, double centerY, byte* pixels, int stride, int width, int height)
{
    var p0 = ToCanvas(tri.V0.PRot, centerX, centerY);
    var p1 = ToCanvas(tri.V1.PRot, centerX, centerY);
    var p2 = ToCanvas(tri.V2.PRot, centerX, centerY);

    int minY = (int)Math.Floor(Math.Min(p0.Y, Math.Min(p1.Y, p2.Y)));
    int maxY = (int)Math.Ceiling(Math.Max(p0.Y, Math.Max(p1.Y, p2.Y)));

    int bytesPerPixel = 4; // zakładamy BGRA32 — ustawione przy ładowaniu

    for (int y = minY; y <= maxY; y++)
    {
        if (y < 0 || y >= height) continue;

        List<double> xIntersections = new();
        AddEdgeIntersection(p0, p1, y, xIntersections);
        AddEdgeIntersection(p1, p2, y, xIntersections);
        AddEdgeIntersection(p2, p0, y, xIntersections);

        if (xIntersections.Count < 2) continue;
        xIntersections.Sort();
        int xStart = (int)Math.Floor(xIntersections[0]);
        int xEnd = (int)Math.Ceiling(xIntersections[1]);

        for (int x = xStart; x <= xEnd; x++)
        {
            if (x < 0 || x >= width) continue;

            var pixel = new Point(x, y);
            var bary = ComputeBarycentric(pixel, p0, p1, p2);
            if (bary == null) continue;

            float l0 = bary.Value.X;
            float l1 = bary.Value.Y;
            float l2 = bary.Value.Z;

            // interpolowana normalna powierzchni
            Vector3 N = Vector3.Normalize(tri.V0.NRot * l0 + tri.V1.NRot * l1 + tri.V2.NRot * l2);

            // domyślny kolor materiału (biały)
            Vector3 IO = new(1, 1, 1);

            // --- Sampling normal mapy + koloru z diffuse ---
            if (_useNormalMap && _normalMap != null && _normalMapBytes != null && _normalMapStride > 0)
            {
                Vector2 uv =
                    new Vector2(tri.V0.U, tri.V0.V) * l0 +
                    new Vector2(tri.V1.U, tri.V1.V) * l1 +
                    new Vector2(tri.V2.U, tri.V2.V) * l2;

                // konwersja UV -> współrzędne tekstury
                int texX = (int)(uv.X * (_normalMap.PixelWidth - 1));
                int texY = (int)((1 - uv.Y) * (_normalMap.PixelHeight - 1)); // inwersja Y jeśli tekstura odwrócona

                texX = Math.Clamp(texX, 0, _normalMap.PixelWidth - 1);
                texY = Math.Clamp(texY, 0, _normalMap.PixelHeight - 1);

                long texIndexLong = (long)texY * _normalMapStride + (long)texX * bytesPerPixel;
                if (texIndexLong >= 0 && texIndexLong + bytesPerPixel <= _normalMapBytes.Length)
                {
                    int texIndex = (int)texIndexLong;

                    byte bTex = _normalMapBytes[texIndex + 0];
                    byte gTex = _normalMapBytes[texIndex + 1];
                    byte rTex = _normalMapBytes[texIndex + 2];
                    // alpha pomijamy

                    // kolor materiału z bitmapy
                    IO = new Vector3(rTex / 255f, gTex / 255f, bTex / 255f);

                    // normal z normal mapy (zakładamy tangent space)
                    Vector3 Ntex = new(
                        (rTex / 255f) * 2f - 1f,
                        (gTex / 255f) * 2f - 1f,
                        (bTex / 255f) * 2f - 1f
                    );

                    if (Ntex.Z < 0) Ntex.Z = -Ntex.Z;
                    Ntex = Vector3.Normalize(Ntex);

                    // wylicz tangenta i bitangenta z różnic UV (gwarantuje poprawną orientację)
                    Vector3 dp1 = tri.V1.PRot - tri.V0.PRot;
                    Vector3 dp2 = tri.V2.PRot - tri.V0.PRot;
                    Vector2 duv1 = new(tri.V1.U - tri.V0.U, tri.V1.V - tri.V0.V);
                    Vector2 duv2 = new(tri.V2.U - tri.V0.U, tri.V2.V - tri.V0.V);

                    float det = duv1.X * duv2.Y - duv1.Y * duv2.X;
                    if (Math.Abs(det) < 1e-8f) det = 1e-8f; // uniknij dzielenia przez 0

                    float invDet = 1.0f / det;
                    Vector3 Pu = Vector3.Normalize((dp1 * duv2.Y - dp2 * duv1.Y) * invDet);
                    Vector3 Pv = Vector3.Normalize((dp2 * duv1.X - dp1 * duv2.X) * invDet);
                    Vector3 Nsurf = Vector3.Normalize(N);
                    if (Vector3.Dot(Vector3.Cross(Pu, Pv), Nsurf) < 0)
                        Pu = -Pu;



                    Matrix4x4 M = new(
                        Pu.X, Pv.X, Nsurf.X, 0,
                        Pu.Y, Pv.Y, Nsurf.Y, 0,
                        Pu.Z, Pv.Z, Nsurf.Z, 0,
                        0,    0,    0,       1
                    );

                    N = Vector3.TransformNormal(Ntex, M);
                    N = Vector3.Normalize(N);
                }
            }

            // --- Oświetlenie (Phong) ---
            Vector3 L = Vector3.Normalize(new Vector3(0.5f, 0.3f, 1f)); // kierunek światła
            Vector3 V = new(0, 0, 1);
            Vector3 R = Vector3.Normalize(2 * Vector3.Dot(N, L) * N - L);

            float cosNL = MathF.Max(0, Vector3.Dot(N, L));
            float cosVR = MathF.Max(0, Vector3.Dot(V, R));
            float specular = MathF.Pow(cosVR, _m);

            Vector3 IL = new(1, 1, 1); // białe światło
            Vector3 I = _kd * IL * IO * cosNL + _ks * IL * IO * specular;

            byte r = (byte)Math.Min(255, I.X * 255);
            byte g = (byte)Math.Min(255, I.Y * 255);
            byte b = (byte)Math.Min(255, I.Z * 255);

            int index = y * stride + x * 4;
            pixels[index + 0] = b;
            pixels[index + 1] = g;
            pixels[index + 2] = r;
            pixels[index + 3] = 255;
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
