using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class ImageEffectTest : MonoBehaviour {

    public Shader shader;
    public Transform container;
    [Space ()]
    public int numSteps = 10;
    public float cloudScale;

    public float densityMultiplier;
    [Range (0, 1)]
    public float densityThreshold;

    public Vector3 offset;

    [Space ()]
    Material material;

    private void OnRenderImage (RenderTexture source, RenderTexture destination) {
        if (material == null) {
            material = new Material (shader);
        }
        // Set container bounds:
        material.SetVector ("BoundsMin", container.position - container.localScale / 2);
        material.SetVector ("BoundsMax", container.position + container.localScale / 2);
        material.SetVector ("CloudOffset", offset);
        material.SetFloat ("CloudScale", cloudScale);
        material.SetFloat ("DensityThreshold", densityThreshold);
        material.SetFloat ("DensityMultiplier", densityMultiplier);
        material.SetInt ("NumSteps", numSteps);
        // Set noise textures:
        var noise = FindObjectOfType<NoiseGenerator> ();
        material.SetTexture ("ShapeNoise", noise.shapeTexture);
        material.SetTexture ("DetailNoise", noise.detailTexture);

        // Blit does the following:
        // - sets _MainTex property on material to the source texture.
        // - sets the render target to the destination texture.
        // - draws a full-screen quad.
        // This copies the src texture to the dest texture, with whatever modifications the shader makes.
        Graphics.Blit (source, destination, material);
    }
}