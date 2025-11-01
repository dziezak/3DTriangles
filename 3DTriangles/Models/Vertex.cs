namespace _3DTriangles.Models;
using System.Numerics;

public class Vertex
{
    // punkt przed obrotem
    public Vector3 P;  // pozycja punktu na powierzchni Beziera dla danego (u, v)
    public Vector3 Pu; // wektor styczny w kierunku u ( powierzchnia po u)
    public Vector3 Pv; // wektor styczny w kierunku v 
    public Vector3 N; // wektor normalny N = Pv x Pu
    
    //punt po obrocie
    public Vector3 PRot;
    public Vector3 PuRot;
    public Vector3 PvRot;
    public Vector3 NRot;

    public float U;
    public float V;
}