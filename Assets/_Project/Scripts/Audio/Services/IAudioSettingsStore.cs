namespace WaterSortPuzzle.Audio
{
    public interface IAudioSettingsStore
    {
        PlayerAudioSettings Load();

        void Save(PlayerAudioSettings settings);
    }
}
