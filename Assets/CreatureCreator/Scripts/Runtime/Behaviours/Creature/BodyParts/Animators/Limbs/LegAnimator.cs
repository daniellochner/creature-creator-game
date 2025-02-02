// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using Pinwheel.Griffin;
using System.Collections;
using UnityEngine;
using static DanielLochner.Assets.FootstepEffects;

namespace DanielLochner.Assets.CreatureCreator
{
    /// <summary>
    /// Provides useful interface for creature animations involving legs.
    /// </summary>
    [RequireComponent(typeof(LegConstructor))]
    public class LegAnimator : LimbAnimator
    {
        #region Fields
        private AudioSource footstepAS;
        #endregion

        #region Properties
        public LegConstructor LegConstructor => LimbConstructor as LegConstructor;
        public LegAnimator FlippedLeg => Flipped as LegAnimator;

        public Transform Anchor
        {
            get; private set;
        }

        public float MaxLength
        {
            get; private set;
        }
        public float MaxDistance
        {
            get; private set;
        }
        public float MovementTimeScale
        {
            get; set;
        } = 1f;

        public float Length
        {
            get => Vector3.Distance(transform.position, LegConstructor.Extremity.position);
        }
        public Vector3 DefaultFootLocalPos
        {
            get
            {
                Vector3 localPos = LegConstructor.AttachedLimb.bones[LegConstructor.AttachedLimb.bones.Count - 1].position;
                if (IsFlipped)
                {
                    localPos.x *= -1;
                }
                return localPos;
            }
        }

        public bool IsMovingFoot
        {
            get; private set;
        }
        #endregion

        #region Methods
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (Anchor != null)
            {
                Destroy(Anchor.gameObject);
            }
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            if (LegConstructor.ConnectedFoot != null)
            {
                LegConstructor.ConnectedFoot.transform.rotation = LegConstructor.Extremity.rotation;
            }
        }

        public override void Setup(CreatureAnimator creatureAnimator)
        {
            base.Setup(creatureAnimator);

            footstepAS = LegConstructor.Extremity.GetComponent<AudioSource>();

            Anchor = new GameObject("Anchor").transform;
            Anchor.SetParent(LimbConstructor.Extremity, false);
        }

        public override void Restructure(bool isAnimated)
        {
            base.Restructure(isAnimated);

            if (isAnimated)
            {
                Anchor.SetParent(Dynamic.Transform);
                Anchor.SetPositionAndRotation(LimbConstructor.Extremity.position, LimbConstructor.Extremity.rotation);
            }
            else
            {
                Anchor.SetParent(LimbConstructor.Extremity);
                Anchor.localPosition = Vector3.zero;
                Anchor.localRotation = Quaternion.identity;
                Anchor.localScale = Vector3.one;
            }
        }
        public override void Reinitialize()
        {
            base.Reinitialize();

            // Max Length
            float length = 0;
            for (int i = 0; i < LimbConstructor.Bones.Length - 1; i++)
            {
                length += Vector3.Distance(LimbConstructor.Bones[i].position, LimbConstructor.Bones[i + 1].position);
            }
            MaxLength = length;

            // Max Distance
            float a = Vector3.ProjectOnPlane(transform.position - LegConstructor.Extremity.position, CreatureAnimator.transform.up).magnitude;
            float c = MaxLength;
            float b = Mathf.Sqrt(Mathf.Pow(c, 2) - Mathf.Pow(a, 2));
            MaxDistance = b;
        }

        protected override void HandleTarget()
        {
            target.SetPositionAndRotation(Anchor.position, Anchor.rotation);
        }
        
        public IEnumerator MoveFootRoutine(Vector3 targetPosition, Quaternion targetRotation, float timeToMove, float liftHeight)
        {
            IsMovingFoot = true;

            Vector3 initialPosition = LegConstructor.Extremity.position;
            Quaternion initialRotation = LegConstructor.Extremity.rotation;

            yield return InvokeUtility.InvokeOverTimeRoutine(delegate (float progress)
            {
                // Position
                Vector3 position = Vector3.Lerp(initialPosition, targetPosition, progress);
                position += (liftHeight * Mathf.Sin(progress * Mathf.PI)) * CreatureAnimator.transform.up;
                Anchor.position = position;

                // Rotation
                Quaternion rotation = Quaternion.Slerp(initialRotation, targetRotation, progress);
                Anchor.rotation = rotation;
            }, 
            timeToMove, MovementTimeScale);

            IsMovingFoot = false;
        }

        public void Step(StepType stepType, float intensity)
        {
            if (Physics.Raycast(LegConstructor.Extremity.position + CreatureAnimator.transform.up, -CreatureAnimator.transform.up, out RaycastHit hit, 2f, LayerMask.GetMask("Ground")))
            {
                Step(stepType, hit, intensity);
            }
        }
        public void Step(StepType stepType, RaycastHit? hit, float intensity)
        {
            if (SettingsManager.Data.Footsteps && hit != null)
            {
                FootstepEffects effects = null;

                if (hit.Value.collider.TryGetComponent(out GTerrainChunk chunk))
                {
                    Vector2 uv = chunk.Terrain.WorldPointToUV(hit.Value.point);
                    int size = chunk.Terrain.TerrainData.Shading.SplatControlResolution;
                    int x = Mathf.FloorToInt(uv.x * size);
                    int y = Mathf.FloorToInt(uv.y * size);

                    Texture2D splatControl = chunk.Terrain.TerrainData.Shading.GetSplatControl(0);
                    int index = GetDominantTextureIndex(splatControl.GetPixel(x, y));
                    Texture texture = chunk.Terrain.TerrainData.Shading.Splats.Prototypes[index].Texture;

                    effects = DatabaseManager.GetDatabaseEntry<FootstepEffects>("Footsteps", texture.name);
                }
                else
                if (hit.Value.collider.TryGetComponent(out MeshRenderer renderer))
                {
                    Texture texture = renderer.material.mainTexture;
                    string id = "";
                    if (texture != null)
                    {
                        id = texture.name;
                    }
                    else
                    {
                        int n = " (Instance)".Length;
                        string name = renderer.material.name;
                        id = name.Substring(0, name.Length - n);
                    }
                    effects = DatabaseManager.GetDatabaseEntry<FootstepEffects>("Footsteps", id);
                }

                if (effects)
                {
                    AudioClip sound = effects.GetSound(stepType);
                    if (sound)
                    {
                        footstepAS.PlayOneShot(sound, intensity);
                    }

                    GameObject particle = effects.GetParticle(stepType);
                    if (particle)
                    {
                        Instantiate(particle, hit.Value.point, Quaternion.identity, Dynamic.Transform).transform.localScale *= intensity;
                    }
                }
            }
        }

        private int GetDominantTextureIndex(Color splat)
        {
            float maxValue = 0;
            int maxIndex = 0;
            for (int i = 0; i < 3; i++)
            {
                if (splat[i] > maxValue)
                {
                    maxValue = splat[i];
                    maxIndex = i;
                }
            }
            return maxIndex;
        }
        #endregion
    }
}