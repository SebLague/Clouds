using UnityEngine;

[System.Serializable]
public struct WorleySettings {
    public int seed;
    public int numPoints;
    public int numLayers;
    public float lacunarity;
    public float persistence;
    public float densityOffset;
    public float densityMultiplier;

    public static int Size {
        get {
            int num32BitVars = 7;
            return sizeof (float) * num32BitVars;
        }
    }
}