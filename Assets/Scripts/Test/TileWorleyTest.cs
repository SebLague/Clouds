using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Visualisation;

[ExecuteInEditMode]
public class TileWorleyTest : MonoBehaviour {
    public int seed;
    public int numCells = 2;
    public bool invert = true;
    public FilterMode filterMode;

    public bool threeDMode;
    public bool dontUpdate;
    public int resolution = 100;
    [Range (1, 3)]
    public float tile = 1;
    public int debugI;
    [Range (0, 9)]
    public int debugOffset;
    Vector2[] points;
    float[] values;
    Texture2D texture;
    public Material material;

    public bool showAdj;
    [Range (0, 1)]
    public float adjDimming = .6f;
    public bool useGrid;
    public bool makeTile;
    public Color gridCol = Color.green;

    [Space ()]
    public bool showPoints;
    public bool animatePoints;
    public Color pointCol = Color.white;
    public float debugRadius = 1f;
    public float pointAnimDelay = .1f;
    public float pointGrowSpeed = 2;

    [Space ()]
    public bool showTexture;
    public bool animateTexture;
    public float textureAnimSpeed = 2;
    float currentTextureAnimT;
    public float sampleRadius = 1;
    public Color sampleCol = Color.cyan;

    [Space ()]
    public bool showClosestPoint;
    public int sampleIndex;
    public float lineWidth = 1;
    public Color lineCol = Color.green;

    static Vector3[] offsets3D = {
        // centre
        new Vector3 (0, 0, 0),
        // front face
        new Vector3 (0, 0, 1),
        new Vector3 (-1, 1, 1),
        new Vector3 (-1, 0, 1),
        new Vector3 (-1, -1, 1),
        new Vector3 (0, 1, 1),
        new Vector3 (0, -1, 1),
        new Vector3 (1, 1, 1),
        new Vector3 (1, 0, 1),
        new Vector3 (1, -1, 1),
        // back face
        new Vector3 (0, 0, -1),
        new Vector3 (-1, 1, -1),
        new Vector3 (-1, 0, -1),
        new Vector3 (-1, -1, -1),
        new Vector3 (0, 1, -1),
        new Vector3 (0, -1, -1),
        new Vector3 (1, 1, -1),
        new Vector3 (1, 0, -1),
        new Vector3 (1, -1, -1),
        // ring around centre
        new Vector3 (-1, 1, 0),
        new Vector3 (-1, 0, 0),
        new Vector3 (-1, -1, 0),
        new Vector3 (0, 1, 0),
        new Vector3 (0, -1, 0),
        new Vector3 (1, 1, 0),
        new Vector3 (1, 0, 0),
        new Vector3 (1, -1, 0)
    };

    static readonly Vector2Int[] offsets = new Vector2Int[] {
        new Vector2Int (0, 0),
        new Vector2Int (-1, 0),
        new Vector2Int (1, 0),
        new Vector2Int (0, 1),
        new Vector2Int (0, -1),
        new Vector2Int (-1, -1),
        new Vector2Int (1, 1),
        new Vector2Int (1, -1),
        new Vector2Int (-1, 1)
    };

    void AnimateTexture () {
        float pixelSize = 1f / resolution;
        if (animateTexture)
            currentTextureAnimT += Time.deltaTime * textureAnimSpeed;
        int texI = Mathf.Min (texture.width * texture.height - 1, (int) currentTextureAnimT);
        if (!Application.isPlaying || !animateTexture) {
            texI = resolution * resolution;
        }

        if (showClosestPoint || Application.isPlaying && animateTexture) {
            float y = (int) (texI / (float) resolution);
            float x = texI % resolution;
            var samplePos = new Vector2 (x, y) / resolution + Vector2.one * pixelSize / 2;

            if (!Application.isPlaying) {
                y = (int) (sampleIndex / (float) resolution);
                x = sampleIndex % resolution;
                samplePos = new Vector2 (x, y) / resolution + Vector2.one * pixelSize / 2;
            }

            if (!makeTile) {
                float minSqrDst = 1;
                Vector2 nearestPos = Vector2.zero;
                for (int i = 0; i < points.Length; i++) {
                    float s = (samplePos - points[i]).sqrMagnitude;
                    if (s < minSqrDst) {
                        minSqrDst = s;
                        nearestPos = points[i];
                    }
                }

                Vis.DrawSphere (samplePos, sampleRadius / 20f, sampleCol, Style.Unlit);
                Vis.DrawLine (samplePos, nearestPos, lineWidth / 10, lineCol, Style.Unlit);
            }
        }

        Color[] cols = new Color[resolution * resolution];
        for (int i = 0; i < texI; i++) {

            cols[i] = new Color (values[i], values[i], values[i]);
        }
        texture.SetPixels (cols);
    }

    void ShowPoints () {
        for (int i = 0; i < points.Length; i++) {
            float radius = debugRadius / 20f;
            if (Application.isPlaying && animatePoints) {
                float t = Time.time - i * pointAnimDelay;
                radius = Mathf.Lerp (0, radius, t * pointGrowSpeed);
            }

            if (showAdj) {
                for (int j = 1; j < offsets.Length; j++) {
                    Vis.DrawSphere (points[i] + offsets[j], radius, new Color (pointCol.r * adjDimming, pointCol.g * adjDimming, pointCol.b * adjDimming), Style.Unlit);
                }
            }
            Vis.DrawSphere (points[i], radius, pointCol, Style.Unlit);
        }
    }

    void Do3D () {
        var prng = new System.Random (seed);
        var points = new Vector3[numCells * numCells * numCells];
        float cellSize = 1f / numCells;

        for (int x = 0; x < numCells; x++) {
            for (int y = 0; y < numCells; y++) {
                for (int z = 0; z < numCells; z++) {
                    float randomX = (float) prng.NextDouble ();
                    float randomY = (float) prng.NextDouble ();
                    float randomZ = (float) prng.NextDouble ();
                    Vector3 randomOffset = new Vector3 (randomX, randomY, randomZ) * cellSize;
                    Vector3 cellCorner = (new Vector3 (x, y, z)) * cellSize - Vector3.one * .5f;

                    int index = x + numCells * (y + z * numCells);
                    points[index] = cellCorner + randomOffset;
                }
            }
        }

        for (int i = 0; i < points.Length; i++) {
            float radius = debugRadius / 20f;
            if (Application.isPlaying && animatePoints) {
                float t = Time.time - i * pointAnimDelay-5;
                radius = Mathf.Lerp (0, radius, t * pointGrowSpeed);
            }

            if (showAdj) {
                for (int j = 1; j < offsets3D.Length; j++) {
                    Vis.DrawSphere (points[i] + offsets3D[j], radius, new Color (pointCol.r * adjDimming, pointCol.g * adjDimming, pointCol.b * adjDimming), Style.Unlit);
                }
            }
            Vis.DrawSphere (points[i], radius, pointCol, Style.Unlit);
        }

    }

    void Update () {

        if (threeDMode) {
            Do3D ();
            return;
        }

        if (Input.GetKey (KeyCode.Space)) {
            tile = Mathf.MoveTowards (tile, 3, Time.deltaTime * .2f);
        } else {
            tile = Mathf.MoveTowards (tile, 1, Time.deltaTime * .2f);
        }

        if (texture == null || texture.width != resolution) {
            texture = new Texture2D (resolution, resolution);
        }

        if (!dontUpdate) {
            GeneratePoints ();
            if (useGrid) {
                GenerateGridVals ();
            } else {
                GenerateVals ();
            }
            Normalize ();

            if (showPoints) {
                ShowPoints ();
            }
            if (showTexture) {
                AnimateTexture ();
            } else {
                Color[] cols = new Color[resolution * resolution];
                texture.SetPixels (cols);

            }

            texture.Apply ();
        }
        material.mainTexture = texture;
        texture.filterMode = filterMode;
        texture.wrapMode = TextureWrapMode.Repeat;
        material.mainTextureScale = Vector2.one * tile;
    }

    void GenerateVals () {
        float pixelSize = 1f / resolution;
        values = new float[resolution * resolution];
        for (int y = 0; y < resolution; y++) {
            for (int x = 0; x < resolution; x++) {

                int index = y * resolution + x;
                Vector2 samplePos = new Vector2 (x, y) / (resolution) + Vector2.one * pixelSize / 2;
                float minSqrDst = 1;

                for (int i = 0; i < points.Length; i++) {
                    if (makeTile) {
                        foreach (Vector2 offset in offsets) {
                            minSqrDst = Mathf.Min (minSqrDst, (points[i] - (samplePos + offset)).sqrMagnitude);
                        }
                    } else {
                        minSqrDst = Mathf.Min (minSqrDst, (points[i] - samplePos).sqrMagnitude);
                    }
                }
                values[index] = Mathf.Sqrt (minSqrDst);
            }
        }
    }

    void GenerateGridVals () {
        float pixelSize = 1f / resolution;
        values = new float[resolution * resolution];
        float cellSize = 1f / numCells;
        float maxPossSqrDst = 2 * (cellSize * cellSize);
        float maxPossDst = Mathf.Sqrt (maxPossSqrDst);

        for (int y = 0; y < resolution; y++) {
            for (int x = 0; x < resolution; x++) {
                int i = y * resolution + x;

                Vector2 samplePos = new Vector2 (x, y) / (resolution) + Vector2.one * pixelSize / 2;
                Vector2Int cell = new Vector2Int ((int) (samplePos.x * numCells), (int) (samplePos.y * numCells));
                float minSqrDst = 1;

                for (int offsetIndex = 0; offsetIndex < offsets.Length; offsetIndex++) {
                    Vector2Int offset = offsets[offsetIndex];
                    Vector2Int adjCell = cell + offset;
                    if (adjCell.x >= 0 && adjCell.x < numCells && adjCell.y >= 0 && adjCell.y < numCells) {
                        int adjCellIndex = adjCell.y * numCells + adjCell.x;
                        float sqrDst = (samplePos - points[adjCellIndex]).sqrMagnitude;
                        if (sqrDst < minSqrDst) {
                            minSqrDst = sqrDst;
                        }

                    } else {
                        if (makeTile) {
                            adjCell = new Vector2Int ((adjCell.x + numCells) % numCells, (adjCell.y + numCells) % numCells);
                            for (int wrapOffsetIndex = 0; wrapOffsetIndex < offsets.Length; wrapOffsetIndex++) {
                                int adjCellIndex = adjCell.y * numCells + adjCell.x;
                                Vector2 offsetPos = points[adjCellIndex] + new Vector2 (offsets[wrapOffsetIndex].x, offsets[wrapOffsetIndex].y);
                                float sqrDst = (samplePos - offsetPos).sqrMagnitude;
                                if (sqrDst < minSqrDst) {
                                    minSqrDst = sqrDst;
                                }
                            }

                        }

                    }
                }

                float d = Mathf.Sqrt (minSqrDst) / maxPossDst;
                values[i] = d;
            }
        }
    }

    void Normalize () {
        float min = float.MaxValue;
        float max = float.MinValue;
        for (int i = 0; i < values.Length; i++) {
            min = Mathf.Min (min, values[i]);
            max = Mathf.Max (max, values[i]);
        }
        for (int i = 0; i < values.Length; i++) {
            values[i] = Mathf.InverseLerp (min, max, values[i]);
            if (invert) {
                values[i] = 1 - values[i];
            }

        }

    }

    void GeneratePoints () {
        Random.InitState (seed);
        int numPoints = numCells * numCells;
        float cellSize = 1f / numCells;
        points = new Vector2[numPoints];

        if (useGrid) {
            for (int y = 0; y < numCells; y++) {
                for (int x = 0; x < numCells; x++) {

                    int i = y * numCells + x;
                    Vector2 cellCorner = new Vector2 (x, y) * cellSize;
                    Vector2 pos = cellCorner + new Vector2 (Random.value, Random.value) * cellSize;
                    points[i] = pos;
                }
            }
        } else {
            for (int i = 0; i < points.Length; i++) {
                points[i] = new Vector2 (Random.value, Random.value);
            }
        }
    }

    void OnDrawGizmos () {
        if (threeDMode) {
            if (showAdj) {
                Gizmos.color = new Color (gridCol.r * adjDimming, gridCol.g * adjDimming, gridCol.b * adjDimming);
                foreach (var o in offsets3D) {
                    Gizmos.DrawWireCube (o, Vector3.one);
                }
            }
            Gizmos.color = gridCol;
            Gizmos.DrawWireCube (Vector3.zero, Vector3.one);

            return;
        }
        if (useGrid) {
            float cellSize = 1 / (float) numCells;
            Gizmos.color = gridCol;
            for (int y = 0; y < numCells; y++) {
                for (int x = 0; x < numCells; x++) {

                    Vector2 cellCorner = new Vector2 (x, y) * cellSize;
                    Gizmos.DrawWireCube (cellCorner + Vector2.one * cellSize / 2, Vector2.one * cellSize);

                }
            }
        } else {
            Gizmos.color = gridCol;
            Gizmos.DrawWireCube (Vector2.one * .5f, Vector2.one);
        }

        if (showAdj) {

            Gizmos.color = new Color (gridCol.r * adjDimming, gridCol.g * adjDimming, gridCol.b * adjDimming);
            foreach (var o in offsets) {
                Gizmos.DrawWireCube (Vector2.one * .5f + o, Vector2.one);
            }
        }
    }
}