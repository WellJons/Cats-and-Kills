using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CatsAndKills.Core
{
    public sealed class HapticsManager : MonoBehaviour
    {
        public static HapticsManager Instance { get; private set; }

        private Coroutine _rumble;

        private void Awake()
        {
            Instance = this;
        }

        public void Pulse(float low, float high, float duration)
        {
            if (Gamepad.current == null || !GamePreferences.Haptics) return;

            if (_rumble != null) StopCoroutine(_rumble);
            _rumble = StartCoroutine(RumbleRoutine(low, high, duration));
        }

        private IEnumerator RumbleRoutine(float low, float high, float duration)
        {
            Gamepad.current?.SetMotorSpeeds(
                Mathf.Clamp01(low),
                Mathf.Clamp01(high));

            yield return new WaitForSecondsRealtime(duration);

            Gamepad.current?.SetMotorSpeeds(0f, 0f);
            _rumble = null;
        }

        private void OnDisable()
        {
            Gamepad.current?.SetMotorSpeeds(0f, 0f);
        }

        private void OnDestroy()
        {
            Gamepad.current?.SetMotorSpeeds(0f, 0f);

            if (Instance == this)
                Instance = null;
        }
    }
}
