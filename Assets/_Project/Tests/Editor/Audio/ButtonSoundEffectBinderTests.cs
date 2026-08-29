using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WaterSortPuzzle.Audio;

namespace WaterSortPuzzle.Tests.EditMode.Audio
{
    public sealed class ButtonSoundEffectBinderTests
    {
        private GameObject canvasObject;
        private Button firstButton;
        private Button secondButton;
        private ButtonSoundEffectBinder buttonSoundEffectBinder;
        private SoundEffectRequestChannel requestChannel;
        private List<SoundEffectId> requestedSoundEffects;

        [SetUp]
        public void SetUp()
        {
            canvasObject = new GameObject(
                "Canvas",
                typeof(RectTransform),
                typeof(Canvas));
            canvasObject.SetActive(false);

            firstButton = CreateButton("FirstButton");
            secondButton = CreateButton("SecondButton");
            secondButton.gameObject.SetActive(false);

            buttonSoundEffectBinder =
                canvasObject.AddComponent<ButtonSoundEffectBinder>();
            requestChannel = ScriptableObject.CreateInstance<SoundEffectRequestChannel>();
            requestedSoundEffects = new List<SoundEffectId>();
            requestChannel.Requested += requestedSoundEffects.Add;

            Configure(buttonSoundEffectBinder);
            InvokeLifecycleMethod("OnEnable");
        }

        [TearDown]
        public void TearDown()
        {
            InvokeLifecycleMethod("OnDisable");
            requestChannel.Requested -= requestedSoundEffects.Add;
            Object.DestroyImmediate(requestChannel);
            Object.DestroyImmediate(canvasObject);
        }

        [Test]
        public void ButtonClicks_RequestConfiguredSoundEffect()
        {
            firstButton.onClick.Invoke();
            secondButton.onClick.Invoke();

            Assert.That(
                requestedSoundEffects,
                Is.EqualTo(new[]
                {
                    SoundEffectId.ButtonClick,
                    SoundEffectId.ButtonClick
                }));
        }

        [Test]
        public void OnDisable_StopsRequestingSoundEffect()
        {
            InvokeLifecycleMethod("OnDisable");

            firstButton.onClick.Invoke();
            secondButton.onClick.Invoke();

            Assert.That(requestedSoundEffects, Is.Empty);
        }

        private Button CreateButton(string name)
        {
            GameObject buttonObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            buttonObject.transform.SetParent(canvasObject.transform);

            return buttonObject.GetComponent<Button>();
        }

        private void Configure(ButtonSoundEffectBinder soundEffectBinder)
        {
            SerializedObject serializedSoundEffect =
                new SerializedObject(soundEffectBinder);
            serializedSoundEffect.FindProperty("requestChannel")
                .objectReferenceValue = requestChannel;
            serializedSoundEffect.FindProperty("soundEffectId")
                .enumValueIndex = (int)SoundEffectId.ButtonClick;
            serializedSoundEffect.ApplyModifiedPropertiesWithoutUndo();
        }

        private void InvokeLifecycleMethod(string methodName)
        {
            MethodInfo method = typeof(ButtonSoundEffectBinder).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            method.Invoke(buttonSoundEffectBinder, null);
        }
    }
}
