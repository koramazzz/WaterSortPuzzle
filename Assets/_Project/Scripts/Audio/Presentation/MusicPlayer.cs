using System;
using System.Collections;
using UnityEngine;

namespace WaterSortPuzzle.Audio
{
    public sealed class MusicPlayer : MonoBehaviour
    {
        [SerializeField] private MusicRequestChannel requestChannel;
        [SerializeField] private AudioSettingsChannel settingsChannel;
        [SerializeField] private AudioSource audioSource;
        [SerializeField, Min(0f)] private float fadeDuration;

        private MusicTrack currentTrack;
        private MusicTrack requestedTrack;
        private Coroutine transition;
        private bool isPaused;
        private float volumeMultiplier = PlayerAudioSettings.DefaultVolume;
        private float fadeFactor = 1f;

        private void Awake()
        {
            audioSource.loop = true;
        }

        private void OnEnable()
        {
            requestChannel.Requested += Play;
            requestChannel.PauseRequested += Pause;
            requestChannel.ResumeRequested += Resume;

            if (settingsChannel != null)
            {
                settingsChannel.Changed += ApplySettings;
                ApplySettings(settingsChannel.Current);
            }
        }

        private void OnDisable()
        {
            requestChannel.Requested -= Play;
            requestChannel.PauseRequested -= Pause;
            requestChannel.ResumeRequested -= Resume;

            if (settingsChannel != null)
            {
                settingsChannel.Changed -= ApplySettings;
            }
        }

        public void Play(MusicTrack musicTrack)
        {
            if (musicTrack == null)
            {
                throw new ArgumentNullException(nameof(musicTrack));
            }

            if (musicTrack.Clip == null)
            {
                throw new InvalidOperationException($"Music track '{musicTrack.name}' has no audio clip.");
            }

            if (musicTrack == requestedTrack)
            {
                Resume();
                return;
            }

            requestedTrack = musicTrack;
            isPaused = false;
            StartTransition(TransitionTo(musicTrack));
        }

        public void Pause()
        {
            if (requestedTrack == null || isPaused)
            {
                return;
            }

            isPaused = true;
            StartTransition(PauseCurrentTrack());
        }

        public void Resume()
        {
            if (!isPaused)
            {
                return;
            }

            isPaused = false;

            if (requestedTrack != currentTrack)
            {
                StartTransition(TransitionTo(requestedTrack));
                return;
            }

            StartTransition(ResumeCurrentTrack());
        }

        private void StartTransition(IEnumerator transitionRoutine)
        {
            if (transition != null)
            {
                StopCoroutine(transition);
            }

            transition = StartCoroutine(transitionRoutine);
        }

        private IEnumerator TransitionTo(MusicTrack musicTrack)
        {
            if (audioSource.isPlaying)
            {
                yield return FadeFactorTo(0f);
            }

            audioSource.Stop();
            audioSource.clip = musicTrack.Clip;
            currentTrack = musicTrack;
            fadeFactor = 0f;
            ApplyOutputVolume();
            audioSource.Play();

            yield return FadeFactorTo(1f);
            transition = null;
        }

        private IEnumerator PauseCurrentTrack()
        {
            if (audioSource.isPlaying)
            {
                yield return FadeFactorTo(0f);
                audioSource.Pause();
            }

            transition = null;
        }

        private IEnumerator ResumeCurrentTrack()
        {
            audioSource.UnPause();

            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }

            yield return FadeFactorTo(1f);
            transition = null;
        }

        private IEnumerator FadeFactorTo(float targetFactor)
        {
            if (Mathf.Approximately(fadeDuration, 0f))
            {
                fadeFactor = targetFactor;
                ApplyOutputVolume();
                yield break;
            }

            float initialFactor = fadeFactor;
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                fadeFactor = Mathf.Lerp(
                    initialFactor,
                    targetFactor,
                    Mathf.Clamp01(elapsedTime / fadeDuration));
                ApplyOutputVolume();
                yield return null;
            }

            fadeFactor = targetFactor;
            ApplyOutputVolume();
        }

        private void ApplySettings(PlayerAudioSettings settings)
        {
            SetVolume(settings.MusicVolume);
        }

        public void SetVolume(float volume)
        {
            volumeMultiplier = Mathf.Clamp01(volume);
            ApplyOutputVolume();
        }

        private void ApplyOutputVolume()
        {
            if (currentTrack == null)
            {
                return;
            }

            audioSource.volume = currentTrack.VolumeScale * volumeMultiplier * fadeFactor;
        }
    }
}
