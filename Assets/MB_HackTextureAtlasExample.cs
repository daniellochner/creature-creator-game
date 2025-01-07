using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalOpus.MB.Core;

/// <summary>
/// Sometimes we want to create an atlas using a custom script (instead of using the TextureBaker)
/// 
/// It is easy to 
/// </summary>
public class MB_HackTextureAtlasExample : MonoBehaviour
{
    [Header("Hack Atlas Generation")]
    public string colorTintPropertyName;
    public Material[] sourceMaterials;
    public MB2_TextureBakeResults materialBakeResult;
    public Material atlasMaterial;
    public Texture2D atlasTexture;

    [Header("Mesh Baker Config")]
    public MB3_MeshBaker targetMeshBaker;

    [ContextMenu("Generate Material Bake Result")]
    public void GenerateMaterialBakeResult()
    {
        {
            // Validate
            // All materials must exist, be more than one, be unique, have colorTintProperty
            // colorTintProperty must be a valid string
        }

        int padding = 2;
        int colorBlockSize = 16;
        bool isProjectLinear = MBVersion.GetProjectColorSpace() == ColorSpace.Linear;

        // Visit each source material and generate a solid color texture matching the color tint.
        Texture2D[] solidColorTextures = new Texture2D[sourceMaterials.Length];
        {
            Color[] colorsToSet = new Color[colorBlockSize * colorBlockSize];
            for (int matIdx = 0; matIdx < sourceMaterials.Length; matIdx++)
            {
                //      Get the color tint
                Material m = sourceMaterials[matIdx];
                Debug.Assert(m.HasProperty(colorTintPropertyName), "Material was missing the colorTint property");
                Color colorTint = m.GetColor(colorTintPropertyName);

                //     Generate a small solid color block texture
                Texture2D tex = solidColorTextures[matIdx] = new Texture2D(colorBlockSize, colorBlockSize, TextureFormat.ARGB32, false, isProjectLinear);
                for (int cIdx = 0; cIdx < colorsToSet.Length; cIdx++)
                {
                    colorsToSet[cIdx] = colorTint;
                }

                tex.SetPixels(colorsToSet);
                tex.Apply();
            }
        }

        // Calculate the atlas dimensions
        int atlasSize_pixels;
        {
            float numTexPerRow = Mathf.Ceil(Mathf.Sqrt(sourceMaterials.Length));
            atlasSize_pixels = (int) numTexPerRow * colorBlockSize;
        }

        Rect[] atlasRects;
        {
            // Create the atlas
            atlasTexture = new Texture2D(atlasSize_pixels, atlasSize_pixels, TextureFormat.ARGB32, false, isProjectLinear);
            
            {
                atlasRects = atlasTexture.PackTextures(solidColorTextures, 0, atlasSize_pixels);
            }

            Debug.Log("Atlas size: w:" + atlasTexture.width + "  h:" + atlasTexture.height + "  numTex: " + solidColorTextures.Length + " (" + colorBlockSize + "x" + colorBlockSize + ")");

            atlasTexture.filterMode = FilterMode.Point;
        }

        // Generate a combined material
        atlasMaterial = new Material(Shader.Find("Standard"));
        atlasMaterial.SetTexture("_MainTex", atlasTexture);
        atlasMaterial.SetColor(colorTintPropertyName, Color.white);
        atlasMaterial.SetFloat("_Glossiness", 0f);

        // Create the material bake result
        {
            materialBakeResult = ScriptableObject.CreateInstance<MB2_TextureBakeResults>();
            materialBakeResult.resultType = MB2_TextureBakeResults.ResultType.atlas;
            materialBakeResult.materialsAndUVRects = new MB_MaterialAndUVRect[solidColorTextures.Length];
            float paddingWidth = ((float) padding) / atlasTexture.width;
            float paddingHeight = ((float)padding) / atlasTexture.height;
            for (int i = 0; i < solidColorTextures.Length; i++)
            {
                Rect rInAtlas = atlasRects[i];
                {
                    // Pad the rectangle in the atlas
                    rInAtlas.x += paddingWidth;
                    rInAtlas.y += paddingHeight;
                    rInAtlas.width -= 2f * paddingWidth;
                    rInAtlas.height -= 2f * paddingHeight;
                }

                materialBakeResult.materialsAndUVRects[i] = new MB_MaterialAndUVRect(
                    sourceMaterials[i],
                    rInAtlas,
                    true,
                    new Rect(0, 0, 1, 1),
                    new Rect(0, 0, 1, 1),
                    new Rect(0, 0, 0, 0),
                    MB_TextureTilingTreatment.none,
                    sourceMaterials[i].name
                    );
            }

            materialBakeResult.resultMaterials = new MB_MultiMaterial[1];
            materialBakeResult.resultMaterials[0] = new MB_MultiMaterial();
            materialBakeResult.resultMaterials[0].combinedMaterial = atlasMaterial;
            materialBakeResult.resultMaterials[0].considerMeshUVs = false;

            List<Material> smats = new List<Material>();
            smats.AddRange(sourceMaterials);
            materialBakeResult.resultMaterials[0].sourceMaterials = smats;
        }
    }
    
    [ContextMenu("Bake Mesh Baker")]
    public void BakeMeshBaker()
    {
        targetMeshBaker.textureBakeResults = materialBakeResult;
        targetMeshBaker.ClearMesh();
        if (targetMeshBaker.AddDeleteGameObjects(targetMeshBaker.GetObjectsToCombine().ToArray(), null, true))
        {
            targetMeshBaker.Apply();
        }
    }
}
