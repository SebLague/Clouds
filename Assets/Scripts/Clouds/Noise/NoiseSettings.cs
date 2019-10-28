using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NoiseSettings : ScriptableObject {

    public event System.Action onValueChanged;

    public abstract System.Array GetDataArray ();

    public abstract int Stride { get; }

    void OnValidate () {
        if (onValueChanged != null) {
            onValueChanged ();
        }
    }
}