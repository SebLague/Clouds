using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Visualisation.MeshGeneration {
    public static class CylinderMesh {

        const int resolution = 20;

        public static void GenerateMesh (Mesh mesh) {

            float radius = .5f;

            var bottomVerts = new List<Vector3> ();
            var bottomTris = new List<int> ();

            var topVerts = new List<Vector3> ();
            var topTris = new List<int> ();

            var sideVerts = new List<Vector3> ();
            var sideTris = new List<int> ();

            // Top/bottom face
            Vector3 bottomCentre = Vector3.down * .5f;
            Vector3 topCentre = Vector3.up * .5f;
            bottomVerts.Add (bottomCentre);
            topVerts.Add (topCentre);

            for (int i = 0; i < resolution; i++) {
                float angle = (i / (resolution - 2f)) * Mathf.PI * 2;
                Vector3 offset = new Vector3 (Mathf.Sin (angle), 0, Mathf.Cos (angle)) * radius;
                bottomVerts.Add (bottomCentre + offset);
                bottomTris.AddRange (new int[] { 0, (i + 2) % resolution, (i + 1) % resolution });

                topVerts.Add (topCentre + offset);
                topTris.AddRange (new int[] { 0, (i + 1) % resolution, (i + 2) % resolution });
            }

            sideVerts.AddRange (bottomVerts.GetRange (1, bottomVerts.Count - 1));
            sideVerts.AddRange (topVerts.GetRange (1, topVerts.Count - 1));

            // Sides
            for (int i = 0; i < resolution - 2; i++) {
                // TODO fix normal seam
                sideTris.Add (i);
                sideTris.Add (i + resolution + 1);
                sideTris.Add (i + resolution);
                //
                sideTris.Add (i);
                sideTris.Add (i + 1);
                sideTris.Add (i + resolution + 1);
            }
            MeshUtility.MeshFromMultipleSources (mesh, new List<Vector3>[] { topVerts, bottomVerts, sideVerts }, new List<int>[] { topTris, bottomTris, sideTris });
        }

    }
}