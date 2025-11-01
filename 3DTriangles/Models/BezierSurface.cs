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
   
   public List<Vector3> GetControlPoints()
   {
      var list = new List<Vector3>();
      for (int i = 0; i < 4; i++)
      {
         for (int j = 0; j < 4; j++)
         {
            list.Add(ControlPoints[i, j]);
         }
      }
      return list;
   }
   
   public Vector3 EvaluateDerivativeU(float u, float v)
   {
      Vector3 result = Vector3.Zero;
      int n = 3; // stopień w kierunku u
      int m = 3; // stopień w kierunku v

      for (int i = 0; i < n; i++)
      {
         for (int j = 0; j <= m; j++)
         {
            Vector3 diff = ControlPoints[i + 1, j] - ControlPoints[i, j];
            float bu = Bernstein(i, n - 1, u);
            float bv = Bernstein(j, m, v);
            result += n * diff * bu * bv;
         }
      }

      return result;
   }

   public Vector3 EvaluateDerivativeV(float u, float v)
   {
      Vector3 result = Vector3.Zero;
      int n = 3; // stopień w kierunku u
      int m = 3; // stopień w kierunku v

      for (int i = 0; i <= n; i++)
      {
         for (int j = 0; j < m; j++)
         {
            Vector3 diff = ControlPoints[i, j + 1] - ControlPoints[i, j];
            float bu = Bernstein(i, n, u);
            float bv = Bernstein(j, m - 1, v);
            result += m * diff * bu * bv;
         }
      }

      return result;
   }



}