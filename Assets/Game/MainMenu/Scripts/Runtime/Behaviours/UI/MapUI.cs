using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DanielLochner.Assets.CreatureCreator
{
    public class MapUI : MonoBehaviour
    {
        #region Fields
        [SerializeField] private Menu mapMenu;
        [SerializeField] private Image screenshotImg;
        [SerializeField] private Sprite[] screenshots;
        [SerializeField] private GameObject lockedIcon;
        [SerializeField] private GameObject timedPanel;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private SimpleScrollSnap.SimpleScrollSnap leaderboardsScrollSnap;

        private OptionSelector mapOS, modeOS, customIdOS;
        #endregion

        #region Methods
        public void Setup(OptionSelector mapOS, OptionSelector modeOS, OptionSelector customIdOS)
        {
            this.mapOS = mapOS;
            this.modeOS = modeOS;
            this.customIdOS = customIdOS;

            OnMapChanged(mapOS.Selected);
            OnModeChanged(modeOS.Selected);
        }

        public void View(Transform anchor)
        {
            mapMenu.transform.position = anchor.position;
            mapMenu.Open();
        }
        public void Hide()
        {
            mapMenu.Close();
        }
        public void OpenLeaderboard()
        {
            LeaderboardsMenu.Instance.Open();
            leaderboardsScrollSnap.GoToPanel(mapOS.Selected);
        }

        public void OnMapChanged(int option)
        {
            Map map = (Map)option;
            if (map == Map.Custom)
            {
                CustomIdOption customIdOption = (CustomIdOption)customIdOS.Options[customIdOS.Selected];
                byte[] thumbnailBytes = File.ReadAllBytes(Path.Combine(CCConstants.MapsDir, customIdOption.MapId, "thumb.png"));
                Texture2D thumbnailTex = new Texture2D(1920, 1080);
                if (ImageConversion.LoadImage(thumbnailTex, thumbnailBytes))
                {
                    screenshotImg.sprite = Sprite.Create(thumbnailTex, new Rect(0, 0, thumbnailTex.width, thumbnailTex.height), new Vector2(0.5f, 0.5f));
                }
                else
                {
                    screenshotImg.sprite = null;
                }
            }
            else
            {
                screenshotImg.sprite = screenshots[option];
            }

            UpdatePadlock();
            UpdateTime();
        }
        public void OnModeChanged(int option)
        {
            Mode mode = (Mode)option;
            timedPanel.SetActive(mode == Mode.Timed);

            UpdatePadlock();
            UpdateTime();
        }

        public void UpdatePadlock()
        {
            bool unlocked = true;
            Map map = (Map)mapOS.Selected;
            if (map == Map.ComingSoon)
            {
                unlocked = false;
            }
            else
            if (map == Map.Custom)
            {
                unlocked = true;
            }
            else
            {
                Mode mode = (Mode)modeOS.Selected;
                if (mode == Mode.Adventure)
                {
                    if (!ProgressManager.Instance.IsMapUnlocked(map))
                    {
                        unlocked = false;
                    }
                }
            }

            lockedIcon.SetActive(!unlocked);
        }
        public void UpdateTime()
        {
            Map map = (Map)mapOS.Selected;
            if (LeaderboardsManager.Instance.MyTimes.TryGetValue(map, out var time))
            {
                timeText.text = FormatTime((int)(time.Score));
            }
            else
            {
                timeText.text = "--:--";
            }
        }

        private string FormatTime(int seconds)
        {
            int mins = seconds / 60;
            int secs = seconds % 60;
            return $"{mins:00}:{secs:00}";
        }
        #endregion
    }
}