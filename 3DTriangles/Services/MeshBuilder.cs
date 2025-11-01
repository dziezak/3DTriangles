using _3DTriangles.Models;

namespace _3DTriangles.Services;
using System.Collections.Generic;
using System.Numerics;

public class MeshBuilder
{
    public static List<Triangle> GenerateMesh(BezierSurface surface, int resolution)
    {
        var triangles = new List<Triangle>();
        float step = 1f / resolution;
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                float u0 = i * step;
                float v0 = j * step;
                float u1 = (i + 1) * step;
                float v1 = (j + 1) * step;
                
                var v00 = CreateVertex(surface, u0, v0);
                var v10 = CreateVertex(surface, u1, v0);
                var v01 = CreateVertex(surface, u0, v1);
                var v11 = CreateVertex(surface, u1, v1);
                
                triangles.Add(new Triangle(v00, v10, v11));
                triangles.Add(new Triangle(v00, v11, v01));
            }
        }
        return triangles;
    }

    private static Vertex CreateVertex(BezierSurface surface, float u, float v)
    {
        var p = surface.Evaluate(u, v);
        return new Vertex {P = p, U = u, V = v};
    }
    
}