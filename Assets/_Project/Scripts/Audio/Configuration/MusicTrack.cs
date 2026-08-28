using UnityEngine;

namespace WaterSortPuzzle.Audio
{
    [CreateAssetMenu(
        fileName = "MusicTrack",
        menuName = "Water Sort Puzzle/Audio/Music Track")]
    public sealed class MusicTrack : ScriptableObject
    {
        [SerializeField] private AudioClip clip;
        [SerializeField, Range(0f, 1f)] private float volumeScale = 1f;

        public AudioClip Clip => clip;
        public float VolumeScale => volumeScale;
    }
}
