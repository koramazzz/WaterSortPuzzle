using UnityEngine;
using UnityEngine.UI;

namespace WaterSortPuzzle.Audio
{
    public sealed class SettingsPopupController : MonoBehaviour
    {
        private const float MutedVolume = PlayerAudioSettings.MinimumVolume;

        [Header("Popup")]
        [SerializeField] private GameObject settingsPopup;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button closeButton;

        [Header("Music")]
        [SerializeField] private Button musicToggleButton;
        [SerializeField] private Slider musicSlider;

        [Header("Sound Effects")]
        [SerializeField] private Button soundEffectToggleButton;
        [SerializeField] private Slider soundEffectSlider;

        [Header("Data")]
        [SerializeField] private AudioSettingsChannel settingsChannel;

        [Header("Toggle Appearance")]
        [SerializeField] private Color audibleIconColor = Color.white;
        [SerializeField] private Color mutedIconColor = new Color(1f, 1f, 1f, 0.45f);

        private readonly AudioSettingsService settingsService =
            new AudioSettingsService(new PlayerPrefsAudioSettingsStore());

        private float lastAudibleMusicVolume = PlayerAudioSettings.DefaultVolume;
        private float lastAudibleSoundEffectVolume = PlayerAudioSettings.DefaultVolume;

        private void Awake()
        {
            PlayerAudioSettings settings = settingsService.Load();

            RememberAudibleVolumes(settings);

            musicSlider.SetValueWithoutNotify(settings.MusicVolume);
            soundEffectSlider.SetValueWithoutNotify(settings.SoundEffectVolume);

            settingsChannel.Apply(settings);
            RefreshToggleAppearance(settings);
            settingsPopup.SetActive(false);
        }

        private void OnEnable()
        {
            settingsButton.onClick.AddListener(Open);
            closeButton.onClick.AddListener(Close);

            musicToggleButton.onClick.AddListener(ToggleMusic);
            soundEffectToggleButton.onClick.AddListener(ToggleSoundEffects);

            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            soundEffectSlider.onValueChanged.AddListener(SetSoundEffectVolume);
        }

        private void OnDisable()
        {
            settingsButton.onClick.RemoveListener(Open);
            closeButton.onClick.RemoveListener(Close);

            musicToggleButton.onClick.RemoveListener(ToggleMusic);
            soundEffectToggleButton.onClick.RemoveListener(ToggleSoundEffects);

            musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
            soundEffectSlider.onValueChanged.RemoveListener(SetSoundEffectVolume);
        }

        public void Open()
        {
            settingsPopup.SetActive(true);
        }

        public void Close()
        {
            settingsPopup.SetActive(false);
        }

        private void ToggleMusic()
        {
            musicSlider.value = IsAudible(musicSlider.value)
                ? MutedVolume
                : lastAudibleMusicVolume;
        }

        private void ToggleSoundEffects()
        {
            soundEffectSlider.value = IsAudible(soundEffectSlider.value)
                ? MutedVolume
                : lastAudibleSoundEffectVolume;
        }

        private void SetMusicVolume(float volume)
        {
            PlayerAudioSettings settings = settingsService.SetMusicVolume(volume);

            if (IsAudible(volume))
            {
                lastAudibleMusicVolume = volume;
            }

            settingsChannel.Apply(settings);
            RefreshToggleAppearance(settings);
        }

        private void SetSoundEffectVolume(float volume)
        {
            PlayerAudioSettings settings = settingsService.SetSoundEffectVolume(volume);

            if (IsAudible(volume))
            {
                lastAudibleSoundEffectVolume = volume;
            }

            settingsChannel.Apply(settings);
            RefreshToggleAppearance(settings);
        }

        private void RememberAudibleVolumes(PlayerAudioSettings settings)
        {
            if (IsAudible(settings.MusicVolume))
            {
                lastAudibleMusicVolume = settings.MusicVolume;
            }

            if (IsAudible(settings.SoundEffectVolume))
            {
                lastAudibleSoundEffectVolume = settings.SoundEffectVolume;
            }
        }

        private void RefreshToggleAppearance(PlayerAudioSettings settings)
        {
            musicToggleButton.image.color = IsAudible(settings.MusicVolume)
                ? audibleIconColor
                : mutedIconColor;

            soundEffectToggleButton.image.color = IsAudible(settings.SoundEffectVolume)
                    ? audibleIconColor
                    : mutedIconColor;
        }

        private static bool IsAudible(float volume)
        {
            return volume > MutedVolume;
        }
    }
}
