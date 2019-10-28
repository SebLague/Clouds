using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SimplexMaster2D : MonoBehaviour {

    public RenderTexture noiseTex;

    public int width = 8;
    public int height = 8;

    public ComputeShader noiseCompute;

    public SimplexSettings settings;

    void Start () {

    }

    void Update () {
        if (noiseTex == null || !noiseTex.IsCreated () || noiseTex.width != width || noiseTex.height != height) {
            if (noiseTex != null) {
                noiseTex.Release ();
            }

            noiseTex = new RenderTexture (width, height, 0);
            noiseTex.enableRandomWrite = true;
            noiseTex.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;

            noiseTex.Create ();
        }

        var settingsBuffer = new ComputeBuffer (1, SimplexSettings.Size);
        settingsBuffer.SetData (new SimplexSettings[] { settings });
        noiseCompute.SetBuffer (0, "noiseSettings", settingsBuffer);
        noiseCompute.SetInt("width", width);
        noiseCompute.SetInt("height", height);
        noiseCompute.SetTexture (0, "Result", noiseTex);
        noiseCompute.Dispatch (0, width, height, 1);

        settingsBuffer.Release ();

        GetComponent<MeshRenderer> ().sharedMaterial.SetTexture ("_MainTex", noiseTex);
    }
}