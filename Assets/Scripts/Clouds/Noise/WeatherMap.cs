using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherMap : MonoBehaviour {

    public bool logTime;
    public ComputeShader noiseCompute;
    public SimplexNoiseSettings noiseSettings;
    public int resolution = 512;
    public RenderTexture weatherMap;
    public Vector4 testParams;
    public Transform container;

    public bool viewerEnabled;
    [HideInInspector]
    public bool showSettingsEditor = true;

    List<ComputeBuffer> buffersToRelease;
    public Vector2 minMax = new Vector2 (0, 1);
    //public int[] minMaxTest;

    public void UpdateMap () {
        var sw = System.Diagnostics.Stopwatch.StartNew ();

        CreateTexture (ref weatherMap, resolution);

        if (noiseCompute == null) {
            return;
        }
        buffersToRelease = new List<ComputeBuffer> ();

        var prng = new System.Random (noiseSettings.seed);
        var offsets = new Vector4[noiseSettings.numLayers];
        for (int i = 0; i < offsets.Length; i++) {
            var o = new Vector4 ((float) prng.NextDouble (), (float) prng.NextDouble (), (float) prng.NextDouble (), (float) prng.NextDouble ());
            offsets[i] = (o * 2 - Vector4.one) * 1000 + (Vector4)container.position;
        }
        CreateBuffer (offsets, sizeof (float) * 4, "offsets");

        var settings = (SimplexNoiseSettings.DataStruct) noiseSettings.GetDataArray ().GetValue (0);
        settings.offset += FindObjectOfType<CloudMaster> ().heightOffset;
        CreateBuffer (new SimplexNoiseSettings.DataStruct[] { settings }, noiseSettings.Stride, "noiseSettings", 0);
        noiseCompute.SetTexture (0, "Result", weatherMap);
        noiseCompute.SetInt ("resolution", resolution);
        var minMaxBuffer = CreateBuffer (new int[] { int.MaxValue, 0 }, sizeof (int), "minMaxBuffer", 0);
        noiseCompute.SetBuffer (1, "minMaxBuffer", minMaxBuffer);
        noiseCompute.SetVector ("minMax", minMax);
        noiseCompute.SetVector("params", testParams);

        int threadGroupSize = 16;
        int numThreadGroups = Mathf.CeilToInt (resolution / (float) threadGroupSize);
        noiseCompute.Dispatch (0, numThreadGroups, numThreadGroups, 1);

        noiseCompute.SetTexture (1, "Result", weatherMap);
        //noiseCompute.Dispatch (1, numThreadGroups, numThreadGroups, 1);

        //minMaxTest = new int[2];
        //minMaxBuffer.GetData (minMaxTest);

        // Release buffers
        foreach (var buffer in buffersToRelease) {
            buffer.Release ();
        }

        if (logTime) {
            Debug.Log ("Weather gen: " + sw.ElapsedMilliseconds + " ms.");
        }
    }

    // Create buffer with some data, and set in shader. Also add to list of buffers to be released
    ComputeBuffer CreateBuffer (System.Array data, int stride, string bufferName, int kernel = 0) {
        var buffer = new ComputeBuffer (data.Length, stride, ComputeBufferType.Raw);
        buffersToRelease.Add (buffer);
        buffer.SetData (data);
        noiseCompute.SetBuffer (kernel, bufferName, buffer);
        return buffer;
    }

    void CreateTexture (ref RenderTexture texture, int resolution) {
        var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_UNorm;
        if (texture == null || !texture.IsCreated () || texture.width != resolution || texture.height != resolution || texture.graphicsFormat != format) {
            if (texture != null) {
                texture.Release ();
            }
            texture = new RenderTexture (resolution, resolution, 0);
            texture.graphicsFormat = format;
            texture.volumeDepth = resolution;
            texture.enableRandomWrite = true;
            texture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            texture.name = name;

            texture.Create ();
        }
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
    }
}