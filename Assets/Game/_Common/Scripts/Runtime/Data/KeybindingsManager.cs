using DanielLochner.Assets.CreatureCreator.Abilities;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class KeybindingsManager : DataManager<KeybindingsManager, Keybindings>
    {
        #region Fields
        [Header("Keybindings")]
        [SerializeField] private NetworkPlayersMenu networkPlayersMenu;
        [Space]
        [SerializeField] private Drop dropAbility;
        [SerializeField] private Hold holdAbility;
        [SerializeField] private Eat eatAbility;
        [SerializeField] private Shoot shootAbility;
        [SerializeField] private Spit spitAbility;
        [SerializeField] private BreatheFire breatheFireAbility;
        [SerializeField] private Growl growlAbility;
        [SerializeField] private NightVision nightVisionAbility;
        [SerializeField] private Bite[] biteAbilities;
        [SerializeField] private Jump[] jumpAbilities;
        [SerializeField] private Flap[] flapAbilities;
        [SerializeField] private Sprint[] sprintAbilities;
        [SerializeField] private Strike[] strikeAbilities;
        [SerializeField] private Spin[] spinAbilities;
        [SerializeField] private Dance[] danceAbilities;
        #endregion

        #region Methods
        protected override void Start()
        {
            base.Start();

            if (Data.AnyNone)
            {
                Data.Revert();
                Save();
            }

            RebindViewPlayers(Data.ViewPlayers);
            RebindBite(Data.Bite);
            RebindDrop(Data.Drop);
            RebindHold(Data.Hold);
            RebindShoot(Data.Shoot);
            RebindFlap(Data.Flap);
            RebindJump(Data.Jump);
            RebindSpit(Data.Spit);
            RebindBreatheFire(Data.BreatheFire);
            RebindGrowl(Data.Growl);
            RebindSprint(Data.Sprint);
            RebindNightVision(Data.NightVision);
            RebindDance(Data.Dance);
            RebindSpin(Data.Spin);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Save();
        }

        public void RebindViewPlayers(Keybind key)
        {
            Data.ViewPlayers = networkPlayersMenu.Keybind = key;
        }
        public void RebindBite(Keybind key)
        {
            foreach (Bite biteAbility in biteAbilities)
            {
                biteAbility.PerformKeybind = key;
            }
            Data.Bite = key;
        }
        public void RebindDrop(Keybind key)
        {
            dropAbility.PerformKeybind = key;
            Data.Drop = key;
        }
        public void RebindHold(Keybind key)
        {
            holdAbility.PerformKeybind = key;
            Data.Hold = key;
        }
        public void RebindShoot(Keybind key)
        {
            shootAbility.PerformKeybind = key;
            Data.Shoot = key;
        }
        public void RebindFlap(Keybind key)
        {
            foreach (Flap flapAbility in flapAbilities)
            {
                flapAbility.PerformKeybind = key;
            }
            Data.Flap = key;
        }
        public void RebindJump(Keybind key)
        {
            foreach (Jump jumpAbility in jumpAbilities)
            {
                jumpAbility.PerformKeybind = key;
            }
            Data.Jump = key;
        }
        public void RebindSpit(Keybind key)
        {
            spitAbility.PerformKeybind = key;
            Data.Spit = key;
        }
        public void RebindBreatheFire(Keybind key)
        {
            breatheFireAbility.PerformKeybind = key;
            Data.BreatheFire = key;
        }
        public void RebindGrowl(Keybind key)
        {
            growlAbility.PerformKeybind = key;
            Data.Growl = key;
        }
        public void RebindSprint(Keybind key)
        {
            foreach (Sprint sprintAbility in sprintAbilities)
            {
                sprintAbility.PerformKeybind = key;
            }
            Data.Sprint = key;
        }
        public void RebindNightVision(Keybind key)
        {
            nightVisionAbility.PerformKeybind = key;
            Data.NightVision = key;
        }
        public void RebindDance(Keybind key)
        {
            foreach (Dance danceAbility in danceAbilities)
            {
                danceAbility.PerformKeybind = key;
            }
            Data.Dance = key;
        }
        public void RebindSpin(Keybind key)
        {
            foreach (Spin spinAbility in spinAbilities)
            {
                spinAbility.PerformKeybind = key;
            }
            Data.Spin = key;
        }
        #endregion
    }
}