using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Scripts.Infra.Music
{
    public class MusicPlayer : MonoBehaviour
    {
        static protected MusicPlayer s_Instance;
        static public MusicPlayer instance { get { return s_Instance; } }
        
        [SerializeField]
        private AudioClip menuTheme;

        public UnityEngine.Audio.AudioMixer mixer;
        public Stem[] stems;
        public float maxVolume = 0.1f;

        void Awake()
        {
            if (s_Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            s_Instance = this;
        
            Application.targetFrameRate = 30;
            AudioListener.pause = false;
        
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            mixer.SetFloat ("MasterVolume", 0.3f);
            mixer.SetFloat ("MusicVolume", 0.3f);
            mixer.SetFloat ("MasterSFXVolume", 0.3f);

            SetStem(0, menuTheme);
            RestartAllStems().Forget();
        }

        public void SetStem(int index, AudioClip clip)
        {
            if (stems.Length <= index)
            {
                Debug.LogError("Trying to set an undefined stem");
                return;
            }

            stems[index].clip = clip;
        }

        public AudioClip GetStem(int index)
        {
            return stems.Length <= index ? null : stems[index].clip;
        }

        public async UniTask RestartAllStems()
        {
            for (int i = 0; i < stems.Length; ++i)
            {
                stems[i].source.clip = stems[i].clip;
                stems [i].source.volume = 0.0f;
                stems[i].source.Play();
            }

            await UniTask.Delay(500);

            for (int i = 0; i < stems.Length; ++i) 
            {
                stems [i].source.volume = stems[i].startingSpeedRatio <= 0.0f ? maxVolume : 0.0f;
            }
        }

        public void UpdateVolumes(float currentSpeedRatio)
        {
            const float fadeSpeed = 0.5f;

            for(int i = 0; i < stems.Length; ++i)
            {
                float target = currentSpeedRatio >= stems[i].startingSpeedRatio ? maxVolume : 0.0f;
                stems[i].source.volume = Mathf.MoveTowards(stems[i].source.volume, target, fadeSpeed * Time.deltaTime);
            }
        }
    }
}
