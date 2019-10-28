using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WorleyNoiseSettings : ScriptableObject {

    public int seed;
    [Range (1, 50)]
    public int numDivisionsA = 5;
    [Range (1, 50)]
    public int numDivisionsB = 10;
    [Range (1, 50)]
    public int numDivisionsC = 15;

    public float persistence = .5f;
    public int tile = 1;
    public bool invert = true;

}