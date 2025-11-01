using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Numerics;
using System.Collections.Generic;
using _3DTriangles.Models;

namespace BezierVisualizer.Views;

public partial class CanvasView : UserControl
{
    private List<Triangle> _triangles;

    public CanvasView()
    {
        InitializeComponent();
    }

    public void SetTriangles(List<Triangle> triangles)
    {
        _triangles = triangles;
        Draw();
    }

    private void Draw()
    {
        DrawCanvas.Children.Clear();
        double centerX = DrawCanvas.ActualWidth / 2;
        double centerY = DrawCanvas.ActualHeight / 2;

        foreach (var tri in _triangles)
        {
            var poly = new Polygon
            {
                Stroke = Brushes.White,
                StrokeThickness = 0.5,
                Fill = Brushes.Gray,
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

    private Point ToCanvas(Vector3 p, double cx, double cy)
    {
        return new Point(cx + p.X * 100, cy - p.Y * 100); // skalowanie i odwrócenie Y
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        Draw();
    }
}