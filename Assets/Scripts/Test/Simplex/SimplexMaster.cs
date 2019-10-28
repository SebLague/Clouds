using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SimplexMaster : MonoBehaviour {

    public RenderTexture noiseTex;

    public int width = 8;
    public int height = 8;
    public int depth = 8;
    public float depthDisplay;

    public ComputeShader noiseCompute;

    public SimplexSettings settings;
    bool needsUpdate = true;

    void Start () {

    }

    void Update () {
        if (noiseTex == null || !noiseTex.IsCreated () || noiseTex.width != width || noiseTex.height != height || noiseTex.volumeDepth != depth) {
            if (noiseTex != null) {
                noiseTex.Release ();
            }

            noiseTex = new RenderTexture (width, height, 0);
            noiseTex.volumeDepth = depth;
            noiseTex.enableRandomWrite = true;
            noiseTex.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;

            noiseTex.Create ();
        }

        if (needsUpdate) {
            needsUpdate = false;

            float[] densityGradient = new float[height];
            for (int i = 0; i < height; i++)
            {
                densityGradient[i] = 1;
            }
            ComputeBuffer densityGradientBuffer = new ComputeBuffer(densityGradient.Length, sizeof(float));
            densityGradientBuffer.SetData(densityGradient);
            noiseCompute.SetBuffer(0, "densityGradient", densityGradientBuffer);


            var settingsBuffer = new ComputeBuffer (1, SimplexSettings.Size);
            settingsBuffer.SetData (new SimplexSettings[] { settings });
            noiseCompute.SetBuffer (0, "noiseSettings", settingsBuffer);
            noiseCompute.SetInt ("width", width);
            noiseCompute.SetInt ("height", height);
            noiseCompute.SetInt ("depth", depth);
            noiseCompute.SetTexture (0, "Result", noiseTex);

            float groupSize = 8;
            noiseCompute.Dispatch (0, Mathf.CeilToInt (width / groupSize), Mathf.CeilToInt (height / groupSize), Mathf.CeilToInt (depth / groupSize));

            settingsBuffer.Release ();
            densityGradientBuffer.Release();

        }
        GetComponent<MeshRenderer> ().sharedMaterial.SetTexture ("_MainTex", noiseTex);
        GetComponent<MeshRenderer> ().sharedMaterial.SetFloat ("depth", depthDisplay);
    }

    void OnValidate () {
        needsUpdate = true;
    }
}