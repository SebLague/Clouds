using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Save3D : MonoBehaviour {

    const int threadGroupSize = 32;
    public ComputeShader slicer;

    public void Save (RenderTexture volumeTexture, string saveName) {
#if UNITY_EDITOR
        string sceneName = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene ().name;
        saveName = sceneName + "_" + saveName;
        int resolution = volumeTexture.width;
        Texture2D[] slices = new Texture2D[resolution];

        slicer.SetInt ("resolution", resolution);
        slicer.SetTexture (0, "volumeTexture", volumeTexture);

        for (int layer = 0; layer < resolution; layer++) {
            var slice = new RenderTexture (resolution, resolution, 0);
            slice.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            slice.enableRandomWrite = true;
            slice.Create ();

            slicer.SetTexture (0, "slice", slice);
            slicer.SetInt ("layer", layer);
            int numThreadGroups = Mathf.CeilToInt (resolution / (float) threadGroupSize);
            slicer.Dispatch (0, numThreadGroups, numThreadGroups, 1);

            slices[layer] = ConvertFromRenderTexture (slice);

        }

        var x = Tex3DFromTex2DArray (slices, resolution);
        UnityEditor.AssetDatabase.CreateAsset (x, "Assets/Resources/" + saveName + ".asset");
#endif
    }

    Texture3D Tex3DFromTex2DArray (Texture2D[] slices, int resolution) {
        Texture3D tex3D = new Texture3D (resolution, resolution, resolution, TextureFormat.ARGB32, false);
        tex3D.filterMode = FilterMode.Trilinear;
        Color[] outputPixels = tex3D.GetPixels ();

        for (int z = 0; z < resolution; z++) {
            Color c = slices[z].GetPixel (0, 0);
            Color[] layerPixels = slices[z].GetPixels ();
            for (int x = 0; x < resolution; x++)
                for (int y = 0; y < resolution; y++) {
                    outputPixels[x + resolution * (y + z * resolution)] = layerPixels[x + y * resolution];
                }
        }

        tex3D.SetPixels (outputPixels);
        tex3D.Apply ();

        return tex3D;
    }

    Texture2D ConvertFromRenderTexture (RenderTexture rt) {
        Texture2D output = new Texture2D (rt.width, rt.height);
        RenderTexture.active = rt;
        output.ReadPixels (new Rect (0, 0, rt.width, rt.height), 0, 0);
        output.Apply ();
        return output;
    }
}