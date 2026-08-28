using UnityEngine;
using UnityEngine.InputSystem;

namespace CatsAndKills.Core
{
    public static class CKInput
    {
        public static bool UsingGamepad =>
            Gamepad.current != null && (
                Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.04f ||
                Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.04f ||
                Gamepad.current.rightTrigger.ReadValue() > 0.1f);

        public static Vector2 Move
        {
            get
            {
                Vector2 kb = Vector2.zero;
                var keyboard = Keyboard.current;
                if (keyboard != null)
                {
                    if (keyboard.wKey.isPressed) kb.y += 1f;
                    if (keyboard.sKey.isPressed) kb.y -= 1f;
                    if (keyboard.dKey.isPressed) kb.x += 1f;
                    if (keyboard.aKey.isPressed) kb.x -= 1f;
                    kb = Vector2.ClampMagnitude(kb, 1f);
                }

                if (Gamepad.current != null)
                {
                    Vector2 stick = Gamepad.current.leftStick.ReadValue();
                    if (stick.sqrMagnitude > kb.sqrMagnitude)
                        return Vector2.ClampMagnitude(stick, 1f);
                }

                return kb;
            }
        }

        public static Vector2 AimStick =>
            Gamepad.current != null ? Gamepad.current.rightStick.ReadValue() : Vector2.zero;

        public static bool SprintHeld =>
            (Keyboard.current != null &&
             (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed)) ||
            (Gamepad.current != null && Gamepad.current.leftStickButton.isPressed);

        public static bool FireHeld =>
            (Mouse.current != null && Mouse.current.leftButton.isPressed) ||
            (Gamepad.current != null && Gamepad.current.rightTrigger.ReadValue() > 0.35f);

        public static bool FirePressed =>
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.rightTrigger.wasPressedThisFrame);

        public static bool ReloadPressed =>
            (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.xButton.wasPressedThisFrame);

        public static bool InteractPressed =>
            (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.aButton.wasPressedThisFrame);

        public static bool GrenadePressed =>
            (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.leftShoulder.wasPressedThisFrame);

        public static bool GrenadeHeld =>
            (Keyboard.current != null && Keyboard.current.gKey.isPressed) ||
            (Gamepad.current != null && Gamepad.current.leftShoulder.isPressed);

        public static bool GrenadeReleased =>
            (Keyboard.current != null && Keyboard.current.gKey.wasReleasedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.leftShoulder.wasReleasedThisFrame);

        public static bool DashPressed =>
            (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.bButton.wasPressedThisFrame);

        public static bool CollarPressed =>
            (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame);

        public static bool Slot1Pressed =>
            (Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.dpad.left.wasPressedThisFrame);

        public static bool Slot2Pressed =>
            (Keyboard.current != null && Keyboard.current.digit2Key.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.dpad.up.wasPressedThisFrame);

        public static bool Slot3Pressed =>
            (Keyboard.current != null && Keyboard.current.digit3Key.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.dpad.right.wasPressedThisFrame);

        public static int WeaponCycleDelta
        {
            get
            {
                if (Mouse.current == null)
                    return 0;

                float y =
                    Mouse.current.scroll.ReadValue().y;

                if (y > 0.01f)
                    return -1;

                if (y < -0.01f)
                    return 1;

                return 0;
            }
        }

        public static bool TacticalMoveClickPressed =>
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame;

        public static bool TacticalShootPressed =>
            (Mouse.current != null &&
             Mouse.current.rightButton.wasPressedThisFrame) ||
            (Keyboard.current != null &&
             Keyboard.current.fKey.wasPressedThisFrame);

        public static bool MolotovPressed =>
            Keyboard.current != null &&
            Keyboard.current.mKey.wasPressedThisFrame;

        public static bool SmokePressed =>
            Keyboard.current != null &&
            Keyboard.current.xKey.wasPressedThisFrame;

        public static bool OverwatchPressed =>
            Keyboard.current != null &&
            Keyboard.current.oKey.wasPressedThisFrame;

        public static bool EndTurnPressed =>
            (Keyboard.current != null &&
             (Keyboard.current.enterKey.wasPressedThisFrame ||
              Keyboard.current.numpadEnterKey.wasPressedThisFrame)) ||
            (Gamepad.current != null &&
             Gamepad.current.startButton.wasPressedThisFrame);

        public static Vector2Int TacticalStepPressed
        {
            get
            {
                Keyboard keyboard =
                    Keyboard.current;

                if (keyboard == null)
                    return Vector2Int.zero;

                if (keyboard.wKey.wasPressedThisFrame ||
                    keyboard.upArrowKey.wasPressedThisFrame)
                {
                    return Vector2Int.up;
                }

                if (keyboard.sKey.wasPressedThisFrame ||
                    keyboard.downArrowKey.wasPressedThisFrame)
                {
                    return Vector2Int.down;
                }

                if (keyboard.aKey.wasPressedThisFrame ||
                    keyboard.leftArrowKey.wasPressedThisFrame)
                {
                    return Vector2Int.left;
                }

                if (keyboard.dKey.wasPressedThisFrame ||
                    keyboard.rightArrowKey.wasPressedThisFrame)
                {
                    return Vector2Int.right;
                }

                return Vector2Int.zero;
            }
        }

        public static Vector2 MouseScreenPosition =>
            Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
    }
}
