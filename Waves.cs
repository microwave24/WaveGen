using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.IntegralTransforms;
using UnityEngine.UIElements;
using System.Numerics;
using System;
using Unity.Mathematics;

public class Waves : MonoBehaviour
{

    [SerializeField] private float scale = 1f;
    [SerializeField] private int quality = 1;
    public float height = 0;


    [SerializeField] private float BaseFreq = 1;
    [SerializeField] private float BaseAmp = 1;
    [SerializeField] private float BaseSpeed = 1;

    [SerializeField] private float GerstnerAmp = 1;
    [SerializeField] private float GerstnerFreq = 1;
    [SerializeField] private float GerstnerSpeed = 1;
    [SerializeField] private float GerstnerWavelength = 1;
    [SerializeField] private float FFTSpeed = 1;
    private Mesh mesh;

    public List<int> triangles = new List<int>();

    private List<UnityEngine.Vector3> vertices = new List<UnityEngine.Vector3>();
    private List<UnityEngine.Vector2> uvs = new List<UnityEngine.Vector2>();

    private int resolution = 1;
    public float waveAmplitude = 1.0f;
    public float windSpeed = 10.0f;
    public UnityEngine.Vector2 windDirection = new UnityEngine.Vector2(1, 1);

    private Complex[] spectrum;
    [SerializeField] private float[] heightMap;
    private float time;


    // Start is called before the first frame update
    void Start()
    {
        resolution = quality*2;
        spectrum = new Complex[resolution * resolution];
        heightMap = new float[resolution * resolution];

        InitializeSpectrum();
        ApplyInverseFFT();
        generateMesh();
    }

    void InitializeSpectrum()
    {
        System.Random random = new System.Random();
        for (int i = 0; i < spectrum.Length; i++)
        {
            float randomAmplitude = (float)random.NextDouble();
            float randomPhase = (float)(random.NextDouble() * 2 * Mathf.PI);
            spectrum[i] = new Complex(randomAmplitude * Mathf.Cos(randomPhase), randomAmplitude * Mathf.Sin(randomPhase));
        }
    }

    

    

    void generateMesh()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        mesh.name = "waveMesh";

        // create verticies



        // in the following, this loop goes through each level of the colomn and creates verticies for its row, all the way untill it reaches the top (quality)
        // z1------>
        // z0------>
        // ext
        // each colomn
        for (int z = 0; z < quality; z++)
        {
            for (int x = 0; x < quality; x++)
            {
                vertices.Add(new UnityEngine.Vector3((float)x / quality, 0, (float)z / quality));
                uvs.Add(new UnityEngine.Vector2((float)x / quality, (float)z / quality));
            }
        }

        // Define triangles
        
        for (int z = 0; z < quality - 1; z++)
        {
            for (int x = 0; x < quality - 1; x++)
            {
                int start = z * (quality) + x;
                triangles.Add(start);
                triangles.Add(start + quality);
                triangles.Add(start + 1);

                triangles.Add(start + quality);
                triangles.Add(start + quality + 1);
                triangles.Add(start + 1);
            }
        }
        
        print(vertices.Count);

        

        // Assign vertices, triangles, and UVs to the mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        // Recalculate normals for proper lighting
        mesh.RecalculateNormals();

        // Assign the mesh to the MeshFilter component
        meshFilter.mesh = mesh;


        // set scale
        gameObject.transform.localScale = new UnityEngine.Vector3(scale, scale, scale);

    }

    void RecalculateMesh(Mesh mesh)
    {
        // Recalculate various attributes of the mesh
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
    }

    void ApplyInverseFFT()
    {
        Fourier.Inverse(spectrum, FourierOptions.Default);
        for (int i = 0; i < spectrum.Length; i++)
        {
            heightMap[i] = (float)spectrum[i].Real * waveAmplitude;

        }

    }

    void applyWave(List<UnityEngine.Vector3> vs)
    {
        for (int i = 0; i < vs.Count; i++)
        {
            float BaseWave = Mathf.PerlinNoise(((vs[i].x * BaseFreq) + (Time.time * BaseSpeed)) , vs[i].z * BaseFreq) * BaseAmp;
            UnityEngine.Vector2 dir = new UnityEngine.Vector2(1, 1);
            
            float VertexHeight = BaseWave + gerstnerWave(vs[i], dir) + heightMap[i];
            
            
            vs[i] = new UnityEngine.Vector3(vs[i].x, VertexHeight, vs[i].z);
        }
    }

    private float gerstnerWave(UnityEngine.Vector3 point, UnityEngine.Vector2 direction)
    {
        float k = 2 * Mathf.PI / GerstnerWavelength; // Wave number
        float c = Mathf.Sqrt(9.81f / k); // Wave speed, assuming deep water

        UnityEngine.Vector2 d = direction.normalized; // Normalize direction

        float f = k * (UnityEngine.Vector2.Dot(d, new UnityEngine.Vector2(point.x, point.z)) - c * Time.time * GerstnerSpeed); // Phase

        float y = GerstnerAmp * Mathf.Sin(f); // Vertical displacement
        float x = d.x * GerstnerAmp * Mathf.Cos(f); // Horizontal displacement (x direction)
        float z = d.y * GerstnerAmp * Mathf.Cos(f); // Horizontal displacement (z direction)

        return y;
    }


    // Update is called once per frame
    void Update()
    {

        time = Time.time;
        //UpdateSpectrum();
        //ApplyInverseFFT();

        applyWave(vertices);
        mesh.vertices = vertices.ToArray();
        RecalculateMesh(mesh);
    }
}
