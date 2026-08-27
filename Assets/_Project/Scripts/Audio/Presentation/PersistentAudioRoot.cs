using UnityEngine;

namespace WaterSortPuzzle.Audio
{
    public sealed class PersistentAudioRoot : MonoBehaviour
    {
        private static PersistentAudioRoot instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
