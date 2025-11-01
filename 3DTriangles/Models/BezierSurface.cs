namespace _3DTriangles.Models;
using System;
using System.Collections.Generic;
using System.Numerics;

public class BezierSurface
{
   public Vector3[,] ControlPoints = new Vector3[4, 4];

   public Vector3 Evaluate(float u, float v)
   {
      Vector3 result = Vector3.Zero;
      for (int i = 0; i <= 3; i++)
      {
         for (int j = 0; j <= 3; j++)
         {
            float bu = Bernstein(i, 3, u);
            float bv = Bernstein(j, 3, v);
            result += ControlPoints[i, j] * bu * bv;
         }
      }
      return result;
   }

   private float Bernstein(int i, int n, float t)
   {
      return Binomail(n, i) * (float)Math.Pow(t, i)*(float)Math.Pow(1-t, n-i);
   }

   private int Binomail(int n, int k)
   {
      int result = 1;
      for (int i = 1; i <= k; i++)
      {
         result *= n - (k-i);
         result /= i;
      }
      return result;
   }
}