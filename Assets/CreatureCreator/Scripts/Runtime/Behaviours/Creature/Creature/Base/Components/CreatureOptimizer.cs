using System.Collections.Generic;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    [RequireComponent(typeof(CreatureConstructor), typeof(MB3_MeshBaker))]
    public class CreatureOptimizer : MonoBehaviour
    {
        #region Fields
        [SerializeField] private bool isAnimated;
        [SerializeField, Button("Optimize")] private bool optimize;

        private Dictionary<BodyPartConstructor, Material[]> cachedMats = new Dictionary<BodyPartConstructor, Material[]>();
        private List<BodyPartConstructor> bodyPartsToAdd = new List<BodyPartConstructor>();
        #endregion

        #region Properties
        private CreatureConstructor Constructor { get; set; }
        private MB3_MeshBaker Baker { get; set; }
        private MB_HackTextureAtlasExample Hack { get; set; }

        public SkinnedMeshRenderer OptimizedCreature { get; private set; }

        public bool IsOptimized { get; private set; }
        #endregion

        #region Methods
        private void Awake()
        {
            Constructor = GetComponent<CreatureConstructor>();
            Baker = GetComponent<MB3_MeshBaker>();
            Hack = GetComponent<MB_HackTextureAtlasExample>();
        }
        private void LateUpdate()
        {
            if (IsOptimized && isAnimated)
            {
                UpdateBlendShapes();
            }
        }

        public void Optimize()
        {
            if (SettingsManager.Instance && !SettingsManager.Data.OptimizeCreatures)
            {
                return;
            }

            if (!IsOptimized)
            {
                // Material
                List<Material> matsToCombine = new List<Material>();
                foreach (BodyPartConstructor bpc in Constructor.BodyParts)
                {
                    if (bpc.IsVisible)
                    {
                        AddBodyPart(bpc, matsToCombine);
                    }
                    if (bpc.Flipped.IsVisible)
                    {
                        AddBodyPart(bpc.Flipped, matsToCombine);
                    }
                }
                Hack.sourceMaterials = matsToCombine.ToArray();
                Hack.GenerateMaterialBakeResult();


                // Mesh
                FlipLimbs();

                GameObject[] objsToCombine = new GameObject[bodyPartsToAdd.Count];
                for (int i = 0; i < bodyPartsToAdd.Count; i++)
                {
                    objsToCombine[i] = bodyPartsToAdd[i].Renderer.gameObject;
                }

                Baker.textureBakeResults = Hack.materialBakeResult;
                Baker.ClearMesh();

                if (Baker.AddDeleteGameObjects(objsToCombine, null, true))
                {
                    Baker.Apply();
                }
                GameObject result = Baker.meshCombiner.resultSceneObject;
                result.name = "Body Parts (Optimized)";
                OptimizedCreature = result.GetComponentInChildren<SkinnedMeshRenderer>();

                FlipLimbs();


                // Blend Shapes
                UpdateBlendShapes();


                IsOptimized = true;
            }
        }
        public void Undo()
        {
            if (IsOptimized)
            {
                Constructor.SkinnedMeshRenderer.enabled = true;
                foreach (BodyPartConstructor bodyPart in bodyPartsToAdd)
                {
                    bodyPart.Renderer.enabled = true;
                    bodyPart.Renderer.sharedMaterials = cachedMats[bodyPart];
                }
                bodyPartsToAdd.Clear();
                cachedMats.Clear();

                DestroyImmediate(Baker.meshCombiner.resultSceneObject.gameObject);

                IsOptimized = false;
            }
        }

        private void AddBodyPart(BodyPartConstructor bpc, List<Material> matsToCombine)
        {
            if (bpc.SkinnedMeshRenderer != null)
            {
                bpc.SkinnedMeshRenderer.sharedMesh = Instantiate(bpc.SkinnedMeshRenderer.sharedMesh);
            }

            cachedMats[bpc] = bpc.Renderer.sharedMaterials;

            Material[] mats = bpc.Renderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i].name += $"_{bpc.name}";
            }
            bpc.Renderer.sharedMaterials = mats;
            matsToCombine.AddRange(mats);

            bodyPartsToAdd.Add(bpc);
        }
        private void FlipLimbs()
        {
            foreach (BodyPartConstructor bodyPart in bodyPartsToAdd)
            {
                if (bodyPart is LimbConstructor && bodyPart.IsFlipped)
                {
                    bodyPart.Model.localScale = new Vector3(-bodyPart.Model.localScale.x, bodyPart.Model.localScale.y, bodyPart.Model.localScale.z);
                }
            }
        }

        private void UpdateBlendShapes()
        {
            int j = 0;
            foreach (BodyPartConstructor bodyPart in bodyPartsToAdd)
            {
                if (bodyPart.SkinnedMeshRenderer == null) continue;
                for (int i = 0; i < bodyPart.SkinnedMeshRenderer.sharedMesh.blendShapeCount; i++, j++)
                {
                    OptimizedCreature.SetBlendShapeWeight(j, bodyPart.SkinnedMeshRenderer.GetBlendShapeWeight(i));
                }
            }
        }
        #endregion
    }
}