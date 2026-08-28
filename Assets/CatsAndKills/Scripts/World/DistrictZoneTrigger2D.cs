using CatsAndKills.Narrative;
using CatsAndKills.UI;
using UnityEngine;

namespace CatsAndKills.World
{
    public sealed class DistrictZoneTrigger2D :
        MonoBehaviour
    {
        [SerializeField] private string zoneName;
        [SerializeField] private string discoveredFlag;

        private bool _shown;

        public void Configure(
            string displayName,
            string flag)
        {
            zoneName = displayName;
            discoveredFlag = flag;
        }

        private void OnTriggerEnter2D(
            Collider2D other)
        {
            if (_shown ||
                other == null ||
                !other.transform.root.CompareTag(
                    "Player"))
            {
                return;
            }

            _shown = true;

            RadioDialogueSystem.Instance?.ShowTransient(
                zoneName,
                1.55f);

            if (!string.IsNullOrWhiteSpace(
                    discoveredFlag))
            {
                NarrativeWorldState.Instance?.SetFlag(
                    discoveredFlag);
            }
        }
    }
}
