using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LightConeTest : MonoBehaviour {

    public int seed;
    public int numStepsLight = 8;
    public float lightConeRadius = 1;

    public Transform target;

    void Update () {

        Debug.DrawLine (Vector3.zero, target.position, Color.yellow);
        Vector3 dir = target.position.normalized;
        Vector3 localX = Vector3.Cross (Vector3.up, dir).normalized;
        Vector3 localZ = Vector3.Cross (localX, dir).normalized;
        Debug.DrawRay (Vector3.zero, localX, Color.red);
        Debug.DrawRay (Vector3.zero, localZ, Color.cyan);

        Vector2[] lightConeOffsets = new Vector2[numStepsLight];
        var prng = new System.Random (seed);
        for (int i = 0; i < numStepsLight; i++) {
            float p = i / (numStepsLight - 1f);
            var offset = new Vector2 ((float) prng.NextDouble (), (float) prng.NextDouble ()) * 2 - Vector2.one;
            lightConeOffsets[i] = offset.normalized * p * lightConeRadius;

            Vector3 localPos = target.position * p + lightConeOffsets[i].x * localX + lightConeOffsets[i].y * localZ;
            Debug.DrawLine(target.position * p, localPos, Color.yellow);
        }
    }
}