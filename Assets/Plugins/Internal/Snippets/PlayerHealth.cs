using System;
using Unity.Netcode;
using UnityEngine;

namespace DanielLochner.Assets
{
    public class PlayerHealth : NetworkBehaviour
    {
        #region Fields
        [SerializeField] private NetworkVariable<float> health = new NetworkVariable<float>(100);
        #endregion

        #region Properties
        public float Health
        {
            get => health.Value;
            set
            {
                if (IsServer)
                {
                    health.Value = value;
                }
                else
                {
                    SetHealthServerRpc(value);
                }
            }
        }
        public float HealthPercentage
        {
            get => Mathf.InverseLerp(0f, MaxHealth, Health);
            set => Health = Mathf.Lerp(0f, MaxHealth, value);
        }

        public virtual float MaxHealth => 100f;

        public Action<float> OnHealthChanged { get; set; }
        public Action<float, DamageReason, string> OnTakeDamage { get; set; }
        public Action<DamageReason, string> OnDie { get; set; }

        public bool IsDead
        {
            get
            {
                return Health <= 0;
            }
        }
        public virtual bool CanTakeDamage => !IsDead;
        #endregion

        #region Methods
        protected virtual void Start()
        {
            health.OnValueChanged += UpdateHealth;
            health.SetDirty(true);
        }

        [ServerRpc]
        private void SetHealthServerRpc(float health)
        {
            Health = health;
        }
        private void UpdateHealth(float oldHealth, float newHealth)
        {
            OnHealthChanged?.Invoke(Health);
        }

        public void Kill(DamageReason reason, string inflicter = null)
        {
            TakeDamage(Health, reason, inflicter);
        }

        public void TakeDamage(float damage, DamageReason reason, string inflicter = null)
        {
            TakeDamageServerRpc(damage, reason, inflicter);
        }
        [ServerRpc(RequireOwnership = false)]
        private void TakeDamageServerRpc(float damage, DamageReason reason, string inflicter)
        {
            if (CanTakeDamage)
            {
                Health -= damage;
                TakeDamageClientRpc(damage, reason, inflicter);

                if (Health <= 0f)
                {
                    Die(reason, inflicter);
                }
            }
        }
        [ClientRpc]
        private void TakeDamageClientRpc(float damage, DamageReason reason, string inflicter)
        {
            OnTakeDamage?.Invoke(damage, reason, inflicter);
        }

        public void Die(DamageReason reason, string inflicter)
        {
            DieServerRpc(reason, inflicter);
        }
        [ServerRpc(RequireOwnership = false)]
        private void DieServerRpc(DamageReason reason, string inflicter)
        {
            transform.parent = null;
            DieClientRpc(reason, inflicter);
        }
        [ClientRpc]
        private void DieClientRpc(DamageReason reason, string inflicter)
        {
            this.InvokeAtEndOfFrame(() => OnDie?.Invoke(reason, inflicter));
        }

        [ContextMenu("Take Random Damage")]
        private void TakeRandomDamage()
        {
            TakeDamage(UnityEngine.Random.Range(0f, MaxHealth), DamageReason.Misc);
        }
        #endregion
    }
}