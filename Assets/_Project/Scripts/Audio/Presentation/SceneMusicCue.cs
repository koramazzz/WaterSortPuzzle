using UnityEngine;

namespace WaterSortPuzzle.Audio
{
    public sealed class SceneMusicCue : MonoBehaviour
    {
        [SerializeField] private MusicRequestChannel requestChannel;
        [SerializeField] private MusicTrack musicTrack;

        private void Start()
        {
            requestChannel.Request(musicTrack);
        }
    }
}
