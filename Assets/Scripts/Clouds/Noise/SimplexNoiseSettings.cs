using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SimplexNoiseSettings : NoiseSettings {

    public int seed;
    [Range(1,6)]
    public int numLayers = 1;
    public float scale = 1;
    public float lacunarity = 2;
    public float persistence = .5f;
    public Vector2 offset;

    public override System.Array GetDataArray () {
        var data = new DataStruct () {
            seed = seed,
            numLayers = Mathf.Max (1, numLayers),
            scale = scale,
            lacunarity = lacunarity,
            persistence = persistence,
            offset = offset
        };

        return new DataStruct[] { data };
    }

    public struct DataStruct {
        public int seed;
        public int numLayers;
        public float scale;
        public float lacunarity;
        public float persistence;
        public Vector2 offset;
    }

    public override int Stride {
        get {
            return sizeof (float) * 7;
        }
    }
}