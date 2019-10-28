using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Worley2D : MonoBehaviour {

    const int threadGroupSize = 8;

    public bool logTimer;
    public int numPoints = 5;
    public int numLayers = 1;
    public float lacunarity = 2;
    public float persistence = .5f;
    public int resolution = 10;
    public float multiplier = 1;
    public float multiplier2 = 1;
    public float offset;
    public int seed;

    public ComputeShader worleyCompute2D;
    public RenderTexture tex2D;
    bool needsUpdate = true;

    public RenderTexture worleyTex;

    void OnValidate () {
        needsUpdate = true;
    }

    void Update () {
        Debug.Log(worleyTex.enableRandomWrite);
        worleyTex.enableRandomWrite = true;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // Validate
        resolution = Mathf.Max (1, resolution);
        numPoints = Mathf.Max (1, numPoints);
        
        if (tex2D == null || !tex2D.IsCreated() || tex2D.width != resolution) {
            if (tex2D != null) {
                tex2D.Release ();
            }
            //threeD.volum
            tex2D = new RenderTexture (resolution, resolution, 0);
            tex2D.enableRandomWrite = true;
            tex2D.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            tex2D.Create ();
            needsUpdate = true;
        }

        if (needsUpdate) {
            needsUpdate = false;
            // Run
            Vector2[] points = new Vector2[numPoints];

             worleyCompute2D.SetInt ("numLayers", numLayers);
             worleyCompute2D.SetFloat ("persistence", persistence);
             worleyCompute2D.SetFloat ("lacunarity", lacunarity);


            worleyCompute2D.SetInt ("numPoints", numPoints);
            worleyCompute2D.SetInt ("resolution", resolution);
            worleyCompute2D.SetFloat("multiplier", multiplier);
            worleyCompute2D.SetFloat("multiplier2", multiplier2);
            worleyCompute2D.SetFloat("offset", offset);
            worleyCompute2D.SetInt("seed", seed);

            worleyCompute2D.SetTexture (0, "values", tex2D);
            int numThreadGroups = Mathf.CeilToInt (resolution / (float)threadGroupSize);
            worleyCompute2D.Dispatch (0, numThreadGroups, numThreadGroups, 1);

            GetComponent<MeshRenderer> ().sharedMaterial.SetTexture ("_MainTex", tex2D);

            if (logTimer) {
                print("Completed: " + sw.ElapsedMilliseconds + " ms.");
            }
        }

    }
}