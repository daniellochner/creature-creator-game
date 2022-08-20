using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    [RequireComponent(typeof(CreatureSpeedup))]
    [RequireComponent(typeof(CreatureCamera))]
    public class CreatureSpeedEffects : MonoBehaviour
    {
        [SerializeField] private SpeedLines speedLinesPrefab;
        [SerializeField] private AudioClip speedUpSFX;
        [SerializeField] private MinMax minMaxFOV;
        [SerializeField] private float fovSmoothing;
        private Camera[] cameras;
        private float targetFOV;
        private AudioSource audioSource;

        public CreatureSpeedup Speedup { get; private set; }
        public CreatureCamera Camera { get; private set; }

        private void Awake()
        {
            Speedup = GetComponent<CreatureSpeedup>();
            Camera = GetComponent<CreatureCamera>();

            audioSource = GetComponent<AudioSource>();
        }

        public void Setup()
        {
            targetFOV = minMaxFOV.min;
            cameras = Camera.Camera.GetComponentsInChildren<Camera>();

            Speedup.OnSpeedUp += delegate (float s, float t)
            {
                audioSource.PlayOneShot(speedUpSFX);
                Instantiate(speedLinesPrefab).Setup(t);
            };
        }
        private void Update()
        {
            if (!Camera.IsSetup) return;

            float s = Mathf.InverseLerp(Speedup.MinMaxSpeedUp.min, Speedup.MinMaxSpeedUp.max, Speedup.Speed);
            float f = Mathf.Lerp(minMaxFOV.min, minMaxFOV.max, s);
            targetFOV = Mathf.Lerp(targetFOV, f, Time.deltaTime * fovSmoothing);

            foreach (Camera cam in cameras)
            {
                cam.fieldOfView = targetFOV;
            }
        }
    }
}