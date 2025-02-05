using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class KingOfTheHill : IndividualMinigame
    {
        #region Fields
        [Header("King Of The Hill")]
        [SerializeField] private TrackRegion region;
        #endregion

        #region Methods
        protected override void Setup()
        {
            base.Setup();

            playing.onEnter += OnPlayingEnter;
            playing.onExit  += OnPlayingExit;
        }

        #region Playing
        private void OnPlayingEnter()
        {
            SetRegionActiveClientRpc(true);
            StartCoroutine(ScoreRoutine());
        }
        private void OnPlayingExit()
        {
            SetRegionActiveClientRpc(false);
        }

        [ClientRpc]
        private void SetRegionActiveClientRpc(bool isActive)
        {
            region.gameObject.SetActive(isActive);
        }

        private IEnumerator ScoreRoutine()
        {
            while (State.Value == MinigameStateType.Playing)
            {
                foreach (var t in region.tracked)
                {
                    CreaturePlayer player = t.GetComponent<CreaturePlayer>();
                    if (player != null)
                    {
                        Score s = Scoreboard[playerIndices[player.OwnerClientId]];
                        Scoreboard[playerIndices[player.OwnerClientId]] = new Score(s, s.score + 1);
                    }
                }
                yield return new WaitForSeconds(1f);
            }
        }
        #endregion
        #endregion
    }
}