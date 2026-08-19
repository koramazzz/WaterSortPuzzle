using UnityEditor;
using UnityEngine;
using WaterSortPuzzle.Audio;

namespace WaterSortPuzzle.Tests.EditMode.Audio
{
    internal static class SoundEffectLibraryTestFactory
    {
        public static SoundEffectLibrary Create(SoundEffectId soundEffectId, AudioClip clip, float volumeScale)
        {
            SoundEffectLibrary soundEffectLibrary = ScriptableObject.CreateInstance<SoundEffectLibrary>();
            SerializedObject serializedLibrary = new SerializedObject(soundEffectLibrary);
            SerializedProperty soundEffects = serializedLibrary.FindProperty("soundEffects");
            soundEffects.arraySize = 1;

            SerializedProperty soundEffect = soundEffects.GetArrayElementAtIndex(0);
            soundEffect.FindPropertyRelative("id").intValue = (int)soundEffectId;
            soundEffect.FindPropertyRelative("clip").objectReferenceValue = clip;
            soundEffect.FindPropertyRelative("volumeScale").floatValue = volumeScale;
            serializedLibrary.ApplyModifiedPropertiesWithoutUndo();
            return soundEffectLibrary;
        }
    }
}
