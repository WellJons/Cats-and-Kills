using CatsAndKills.Narrative;
using CatsAndKills.UI;
using UnityEngine;

namespace CatsAndKills.World
{
    public sealed class PropagandaPoster2D :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField] private string slogan;
        [SerializeField] private string stateFlag;

        public string InteractionPrompt =>
            "ОСМОТРЕТЬ ПЛАКАТ [E]";

        public void Configure(
            string text,
            string observedFlag = null)
        {
            slogan = text;
            stateFlag = observedFlag;
        }

        public void Interact()
        {
            RadioDialogueSystem.Instance
                ?.Say(
                    "АГИТАЦИОННЫЙ ПЛАКАТ",
                    slogan,
                    3.4f);

            if (!string.IsNullOrWhiteSpace(
                    stateFlag))
            {
                NarrativeWorldState.Instance
                    ?.SetFlag(
                        stateFlag);
            }
        }
    }
}
