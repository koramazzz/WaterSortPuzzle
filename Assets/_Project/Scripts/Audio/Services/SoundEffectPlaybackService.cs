using System;

namespace WaterSortPuzzle.Audio
{
    public sealed class SoundEffectPlaybackService
    {
        private readonly SoundEffectLibrary soundEffectLibrary;
        private readonly ISoundEffectOutput soundEffectOutput;

        public SoundEffectPlaybackService(SoundEffectLibrary soundEffectLibrary, ISoundEffectOutput soundEffectOutput)
        {
            this.soundEffectLibrary = soundEffectLibrary ?? throw new ArgumentNullException(nameof(soundEffectLibrary));
            this.soundEffectOutput = soundEffectOutput ?? throw new ArgumentNullException(nameof(soundEffectOutput));
        }

        public void Play(SoundEffectId soundEffectId)
        {
            SoundEffectDefinition soundEffect = soundEffectLibrary.Get(soundEffectId);
            soundEffectOutput.PlayOneShot(soundEffect.Clip, soundEffect.VolumeScale);
        }
    }
}
