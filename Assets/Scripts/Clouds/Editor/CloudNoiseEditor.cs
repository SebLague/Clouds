using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof (CloudNoise))]
public class CloudNoiseEditor : Editor {

    CloudNoise noise;

    const string calculateWorley = "#define CALCULATE_WORLEY";

    public override void OnInspectorGUI () {
        DrawDefaultInspector ();

        using (var check = new EditorGUI.ChangeCheckScope()) {
            noise.calculateWorley = EditorGUILayout.Toggle("Calculate Worley", noise.calculateWorley);

            if (check.changed) {
                ShaderEditorUtility.SetShaderConst(noise.noiseCompute, calculateWorley, noise.calculateWorley);
            }
        }

        if (GUILayout.Button ("Update")) {
            noise.ManualUpdate ();
            EditorApplication.QueuePlayerLoopUpdate ();
        }

    }

    void OnEnable () {
        noise = (CloudNoise) target;
    }


}