using UnityEngine;

namespace CatsAndKills.Core
{
    public static class InputConsumption
    {
        private static int _interactFrame = -1;

        public static bool InteractConsumed =>
            _interactFrame == Time.frameCount;

        public static void ConsumeInteract()
        {
            _interactFrame = Time.frameCount;
        }
    }
}
