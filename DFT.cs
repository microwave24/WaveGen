using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System;
using Unity.Mathematics;
using UnityEngine.UI;

using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
public class DFT : MonoBehaviour
{

    [SerializeField] MeshGenerator waveScript; // to access the grid generated
    public bool ready = false; // this is for waiting untill the grid is generated
    private Mesh meshPlane;

    // Wave Sim Settings //
    public float amplitude = 1f;
    public float size = 1f;
    public float speed = 1f;
    public float minAngle = 0;
    public float maxAngle = 360;
    // ----------------- //

    // user input stuff
    [SerializeField] Transform user_Constants;
    [SerializeField] Slider user_Amp;
    [SerializeField] Slider user_Speed;
    [SerializeField] Slider user_MinAngle, user_MaxAngle;

    private List<Complex> fourierConstants = new List<Complex>();
    //
    public List<Vector2> directionInputs = new List<Vector2>();
    private Vector3[] directions;

    private List<Vector3> verts = new List<Vector3>();

    void Start()
    {
        StartCoroutine(InitializeMeshAndCoefficients());
    }

    private IEnumerator InitializeMeshAndCoefficients()
    {
        // Wait until waveScript.mesh is not null
        while (waveScript.mesh == null)
        {
            yield return new WaitForSeconds(.05f); // Wait for a bit
        }

        int c = user_Constants.childCount;

        for (int i = 0; i < c; i++)
        {
            var current = user_Constants.GetChild(i).GetComponent<Slider>();

            fourierConstants.Add(new Complex(current.value,0));
        }

        directions = new Vector3[c];
        // if no direction inputted just generate one
        for (int i = 0; i < c; i++)
        {
            if (directionInputs[i] == Vector2.zero)
            {
                // create a random set of direction for no-input
                float angle = Random.Range(minAngle, maxAngle) * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                directions[i] = dir;
            }
            else
            {
                directions[i] = new Vector3(directionInputs[i].x, 0, directionInputs[i].y);
            }
        }
        verts = waveScript.vertices;

        meshPlane = waveScript.mesh;
        ready = true;
    }


    void generateWave(float t)
    {
        Vector3[] newPos = new Vector3[verts.Count];
        for (int i = 0; i < verts.Count; i++)
        {
            Vector3 sum = Vector3.zero;
            for(int j = 1; j < fourierConstants.Count; j++)
            {
                var v = new Vector3(verts[i].x, 0, verts[i].z); // prepare the vertex for dot product


                float phase = Vector3.Dot(v, directions[j].normalized) * j * size - (t * speed); // apply direction to current wave

                Complex complexPhase = Complex.Exp(Complex.ImaginaryOne * phase) * fourierConstants[j]; // get wave
                Complex y = amplitude * complexPhase.Real;
                Complex z = verts[i].z - amplitude * complexPhase.Imaginary;
                // -- since this is a gerstner wave, there is horizontal displacement 

                // add to total position
                Vector3 pos = new Vector3(verts[i].x, (float)y.Real, (float)z.Real);
                sum += pos;
            }
            newPos[i] = sum;
            

        }
        meshPlane.vertices = newPos;
        RecalculateMesh(meshPlane);
        
    }
    void RecalculateMesh(Mesh mesh)
    {
        // Recalculate various attributes of the mesh
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
    }

    private void Update()
    {
        if(ready == true)
        {
            double currentMinAngle = minAngle;
            double currentMaxAngle = maxAngle;


            amplitude = user_Amp.value;
            speed = user_Speed.value;
            minAngle = user_MinAngle.value;
            maxAngle = user_MaxAngle.value;

            for (int i = 0; i < fourierConstants.Count; i++) {
                var current = user_Constants.GetChild(i).GetComponent<Slider>();
                fourierConstants[i] = new Complex(current.value, 0);

            }

            if (minAngle != currentMinAngle || maxAngle != currentMaxAngle) {
                for (int i = 0; i < fourierConstants.Count; i++)
                {
                    float angle = Random.Range(minAngle, maxAngle) * Mathf.Deg2Rad;
                    Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                    directions[i] = dir;

                }
            }

            



            generateWave(Time.time * speed);
        }
    }
}

// ----------------------- NOTES
// next time ill finally do FFT >:)