using System;
using System.Collections;
using UnityEngine;

namespace WaterSortPuzzle.Audio
{
    public sealed class MusicPlayer : MonoBehaviour
    {
        [SerializeField] private MusicRequestChannel requestChannel;
        [SerializeField] private AudioSource audioSource;
        [SerializeField, Min(0f)] private float fadeDuration;

        private MusicTrack currentTrack;
        private MusicTrack requestedTrack;
        private Coroutine transition;
        private bool isPaused;

        private void Awake()
        {
            audioSource.loop = true;
        }

        private void OnEnable()
        {
            requestChannel.Requested += Play;
            requestChannel.PauseRequested += Pause;
            requestChannel.ResumeRequested += Resume;
        }

        private void OnDisable()
        {
            requestChannel.Requested -= Play;
            requestChannel.PauseRequested -= Pause;
            requestChannel.ResumeRequested -= Resume;
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
                yield return FadeTo(0f);
            }

            audioSource.Stop();
            audioSource.clip = musicTrack.Clip;
            audioSource.volume = 0f;
            currentTrack = musicTrack;
            audioSource.Play();

            yield return FadeTo(musicTrack.VolumeScale);
            transition = null;
        }

        private IEnumerator PauseCurrentTrack()
        {
            if (audioSource.isPlaying)
            {
                yield return FadeTo(0f);
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

            yield return FadeTo(currentTrack.VolumeScale);
            transition = null;
        }

        private IEnumerator FadeTo(float targetVolume)
        {
            if (Mathf.Approximately(fadeDuration, 0f))
            {
                audioSource.volume = targetVolume;
                yield break;
            }

            float initialVolume = audioSource.volume;
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                audioSource.volume = Mathf.Lerp(
                    initialVolume,
                    targetVolume,
                    Mathf.Clamp01(elapsedTime / fadeDuration));
                yield return null;
            }

            audioSource.volume = targetVolume;
        }
    }
}
