using UnityEngine;

[System.Serializable]
public struct SimplexSettings {
    public int seed;
    public float noiseScale;
    public int numLayers;
    public float persistence;
    public float lacunarity;
    public float densityOffset;
    public Vector3 offset;

    public static int Size {
        get {
            int num32BitVars = 9;
            return sizeof (float) * num32BitVars;
        }
    }
}