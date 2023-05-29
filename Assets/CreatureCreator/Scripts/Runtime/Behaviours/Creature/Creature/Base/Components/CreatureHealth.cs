// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using DamageNumbersPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    [RequireComponent(typeof(CreatureConstructor))]
    public class CreatureHealth : PlayerHealth
    {
        #region Fields
        [SerializeField] private PlayerEffects.Sound[] takeDamageSounds;
        [SerializeField] private float damageTime;
        [SerializeField] private Material damageMaterial;
        [SerializeField] private DamageNumber damagePopupPrefab;

        private Rigidbody rb;

        private Coroutine damageCoroutine;
        #endregion

        #region Properties
        public CreatureConstructor Constructor { get; private set; }
        public PlayerEffects Effects { get; private set; }

        public override float MaxHealth => Constructor.Statistics.Health;
        #endregion

        #region Methods
        private void Awake()
        {
            Constructor = GetComponent<CreatureConstructor>();
            Effects = GetComponent<PlayerEffects>();

            rb = GetComponent<Rigidbody>();
        }

        protected override void Start()
        {
            base.Start();
            OnTakeDamage += delegate (float damage, Vector3 force)
            {
                if (IsOwner) // only one source should play a sound
                {
                    Effects.PlaySound(takeDamageSounds);
                }
                damagePopupPrefab.Spawn(Constructor.Body.position, damage);

                if (damageCoroutine == null)
                {
                    damageCoroutine = StartCoroutine(DamageRoutine());
                }
            };
        }

        private IEnumerator DamageRoutine()
        {
            Dictionary<Renderer, Material[]> rm = new Dictionary<Renderer, Material[]>();

            foreach (BodyPartConstructor bpc in Constructor.BodyParts)
            {
                RecordRenderer(ref rm, bpc.Renderer);
                RecordRenderer(ref rm, bpc.Flipped.Renderer);
            }
            RecordRenderer(ref rm, Constructor.SkinnedMeshRenderer);

            yield return new WaitForSeconds(damageTime);

            foreach (Renderer renderer in rm.Keys)
            {
                renderer.materials = rm[renderer];
            }

            damageCoroutine = null;
        }
        private void RecordRenderer(ref Dictionary<Renderer, Material[]> rm, Renderer renderer)
        {
            rm[renderer] = renderer.sharedMaterials;

            Material[] placeholder = new Material[renderer.materials.Length];
            for (int i = 0; i < placeholder.Length; ++i)
            {
                placeholder[i] = damageMaterial;
            }
            renderer.materials = placeholder;
        }
        #endregion
    }
}