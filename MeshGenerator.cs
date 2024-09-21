using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;

public class MeshGenerator : MonoBehaviour
{
    public Mesh mesh;
    public int quality = 1;

    private List<int> triangles = new List<int>();
    [HideInInspector] public List<Vector3> vertices = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();

    void generateMesh()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        mesh.name = "waveMesh";

        // vertex creation
        for (int z = 0; z < quality; z++)
        {
            for (int x = 0; x < quality; x++)
            {
                // Normalize the vertex positions to ensure the mesh size stays the same
                float normalizedX = (float)x / (quality - 1);
                float normalizedZ = (float)z / (quality - 1);

                vertices.Add(new Vector3(normalizedX, 0, normalizedZ));
                uvs.Add(new Vector2(normalizedX, normalizedZ));
            }
        }
        // Assign Triangles to the mesh

        for (int z = 0; z < (quality) - 1; z++)
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
        // Assign vertices, triangles, and UVs to the mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        // Recalculate normals for proper lighting
        mesh.RecalculateNormals();

        // Assign the mesh to the MeshFilter component
        meshFilter.mesh = mesh;
    }

    private void Start()
    {
        generateMesh();
    }
}
