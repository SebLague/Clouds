using UnityEngine;
using Visualisation.MeshGeneration;

namespace Visualisation {
    public static partial class Vis {

        public static void DrawSphere (Vector3 centre, float radius, Color colour, Style style = Style.Standard) {
            Init ();
            drawList.Add (new DrawInfo (sphereMesh, centre, Quaternion.identity, Vector3.one * radius, colour, style));
        }

        public static void DrawLine (Vector3 start, Vector3 end, float thickness, Color colour, Style style = Style.Standard) {
            Init ();
            float thicknessFactor = 1 / 25f;
            Mesh mesh = CreateOrRecycleMesh ();
            CylinderMesh.GenerateMesh (mesh);
            Vector3 centre = (start + end) / 2;
            var rot = Quaternion.FromToRotation (Vector3.up, (start - end).normalized);
            Vector3 scale = new Vector3 (thickness * thicknessFactor, (start - end).magnitude, thickness * thicknessFactor);
            drawList.Add (new DrawInfo (mesh, centre, rot, scale, colour, style));
        }

        public static void DrawRing (Vector3 centre, Vector3 normal, float startAngle, float angle, float innerRadius, float outerRadius, Color colour, Style style = Style.Standard) {
            Init ();

            Mesh mesh = CreateOrRecycleMesh ();
            RingMesh.GenerateMesh (mesh, angle, innerRadius, outerRadius);

            //float localYAngle = (startAngle - angle / 2); // centre angle
            float localYAngle = 0;
            var rot = Quaternion.AngleAxis (localYAngle, normal) * Quaternion.FromToRotation (Vector3.up, normal);

            drawList.Add (new DrawInfo (mesh, centre, rot, Vector3.one, colour, style));
        }

        public static void DrawDisc (Vector3 centre, Vector3 normal, float radius, Color colour, Style style = Style.Standard) {
            DrawRing (centre, normal, 0, 0, 0, radius, colour, style);
        }

        public static void DrawArc (Vector3 centre, Vector3 normal, float startAngle, float angle, float radius, Color colour, Style style = Style.Standard) {
            DrawRing (centre, normal, startAngle, angle, 0, radius, colour, style);
        }
    }
}