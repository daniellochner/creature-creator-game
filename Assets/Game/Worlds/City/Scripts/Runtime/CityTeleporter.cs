using DanielLochner.Assets.CreatureCreator.Cinematics.Farm;
using System.Collections;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class CityTeleporter : TeleportManager
    {
		#region Fields
        [SerializeField] private Cinematic arriveOnBusCinematic;
        [SerializeField] private Cinematic exitMineshaftCinematic;
        #endregion

        #region Methods
        private IEnumerator Start()
		{
			yield return new WaitForSeconds(1f);
			if (!HasRequestedReview)
			{
				RatingManager.Instance.Rate();
			}
		}
		
        public override void OnEnter(string prevScene, string nextScene)
        {
            base.OnEnter(prevScene, nextScene);
            if (prevScene == "Farm")
            {
                arriveOnBusCinematic.Begin();
            }
            else
            if (prevScene == "Cave")
            {
                exitMineshaftCinematic.Begin();
            }
        }
		#endregion
    }
}