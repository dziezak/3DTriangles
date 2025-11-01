using System.Printing;

namespace _3DTriangles.Models;
using System.Numerics;

public class Vertex
{
    public Vector3 P;  // pozycja punktu na powierzchni Beziera dla danego (u, v)
    public Vector3 Pu; // wektor styczny w kierunku u ( powierzchnia po u)
    public Vector3 Pv; // wektor styczny w kierunku v 
    public Vector3 N; // wektor normalny N = Pv x Pu ( to do oswietlenia sie przyda ^^)
    
    public Vector3 PRot;
    public Vector3 PuRot;
    public Vector3 PvRot;
    public Vector3 NRot;

    public float U;
    public float V;
    
    public void Rotate(float alfaDeg, float betaDeg)
    {
        
        //Console.WriteLine("Rotating vertex");
        float alfa = MathF.PI * alfaDeg / 180f;
        float beta = MathF.PI * betaDeg / 180f;

        Matrix4x4 rotX = Matrix4x4.CreateRotationX(alfa);
        Matrix4x4 rotZ = Matrix4x4.CreateRotationZ(beta);

        Matrix4x4 rot = rotZ * rotX;

        PRot = Vector3.Transform(P, rot);

        PuRot = Vector3.Transform(Pu, rot);
        PvRot = Vector3.Transform(Pv, rot);
        NRot = Vector3.Normalize(Vector3.Cross(PuRot, PvRot));
    }
}