using System.Globalization;
using _3DTriangles.Models;

namespace _3DTriangles.Services;
using System.IO;
using System.Numerics;

public static class FileLoader
{
    public static BezierSurface LoadSurface(string path)
    {
        var surface = new BezierSurface();
        var lines = File.ReadAllLines(path);
        for (int i = 0; i < lines.Length; i++)
        {
            var parts = lines[i].Split(' ');
            float x = float.Parse(parts[0], CultureInfo.InvariantCulture);
            float y = float.Parse(parts[1], CultureInfo.InvariantCulture);
            float z = float.Parse(parts[2], CultureInfo.InvariantCulture);
            surface.ControlPoints[i / 4, i % 4] = new Vector3(x, y, z);
        }
        return surface;
    }
}