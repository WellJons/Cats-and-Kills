namespace CatsAndKills.World
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }
        void Interact();
    }
}
