using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PerlinTileTest : MonoBehaviour {

    public int seed;
    public int res = 20;
    Texture2D texture;

    public float scale = 2;
    public int layers = 3;
    public float persistence = .5f;
    public float lacunarity = 2;
    public Vector2 offset;

    void Update () {
        Update3D ();
    }

    void Update4D () {
        SimplexNoise noise = new SimplexNoise (seed);
        var prng = new System.Random (seed);
        float[] noiseMap = new float[res * res];
        float maxVal = float.MinValue;
        float minVal = float.MaxValue;

        var offsets = new Vector4[layers];
        for (int i = 0; i < layers; i++) {
            offsets[i] = new Vector4 ((float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1) * 1000;
        }

        // circumference should = 1 (length of cylinder)
        // 2*pi*r = 1 therefore r = 1/(2pi)
        float radius = 1 / (2 * Mathf.PI);

        for (int x = 0; x < res; x++) {
            float px = x / (res - 1f);
            float angleZW = px * Mathf.PI * 2;
            float circleZ = Mathf.Cos (angleZW) * radius + offset.x;
            float circleW = Mathf.Sin (angleZW) * radius + offset.y;
            for (int y = 0; y < res; y++) {

                float py = y / (res - 1f);
                float angle = py * Mathf.PI * 2;

                float circleX = Mathf.Cos (angle) * radius;
                float circleY = Mathf.Sin (angle) * radius;

                float amplitude = 1;
                float frequency = scale;
                float noiseVal = 0;
                for (int i = 0; i < layers; i++) {
                    Vector4 p = new Vector4 (circleX, circleY, circleZ, circleW) * frequency + offsets[i];
                    noiseVal += (float) noise.Evaluate (p.x, p.y, p.z, p.w) * amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                noiseMap[x * res + y] = noiseVal;
                maxVal = Mathf.Max (maxVal, noiseVal);
                minVal = Mathf.Min (minVal, noiseVal);
            }
        }

        if (texture == null || texture.width != res) {
            texture = new Texture2D (res, res);
        }
        texture.wrapMode = TextureWrapMode.Repeat;

        var cols = new Color[noiseMap.Length];
        for (int i = 0; i < cols.Length; i++) {
            float v = Mathf.InverseLerp (minVal, maxVal, noiseMap[i]);
            cols[i] = new Color (v, v, v);
        }
        texture.SetPixels (cols);
        texture.Apply ();
        GetComponent<MeshRenderer> ().sharedMaterial.mainTexture = texture;

    }

    void Update3D () {
        SimplexNoise noise = new SimplexNoise (seed);
        float[] noiseMap = new float[res * res];
        float maxVal = float.MinValue;
        float minVal = float.MaxValue;

        for (int x = 0; x < res; x++) {
            float px = x / (res - 1f);
            for (int y = 0; y < res; y++) {

                float py = (y + offset.x) / (res - 1f);
                float angle = py * Mathf.PI * 2;

                // circumference should = 1 (length of cylinder)
                // 2*pi*r = 1 therefore r = 1/(2pi)
                float radius = 1 / (2 * Mathf.PI);
                float circleX = Mathf.Cos (angle) * radius;
                float circleY = Mathf.Sin (angle) * radius;

                float amplitude = 1;
                float frequency = scale;
                float noiseVal = 0;
                for (int i = 0; i < layers; i++) {
                    noiseVal += (float) noise.Evaluate (circleX * frequency+offset.x, circleY * frequency+offset.x, px * frequency) * amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                noiseMap[x * res + y] = noiseVal;
                maxVal = Mathf.Max (maxVal, noiseVal);
                minVal = Mathf.Min (minVal, noiseVal);
            }
        }

        if (texture == null || texture.width != res) {
            texture = new Texture2D (res, res);
        }

        var cols = new Color[noiseMap.Length];
        for (int i = 0; i < cols.Length; i++) {
            float v = Mathf.InverseLerp (minVal, maxVal, noiseMap[i]);
            cols[i] = new Color (v, v, v);
        }
        texture.SetPixels (cols);
        texture.Apply ();
        GetComponent<MeshRenderer> ().sharedMaterial.mainTexture = texture;

    }
}