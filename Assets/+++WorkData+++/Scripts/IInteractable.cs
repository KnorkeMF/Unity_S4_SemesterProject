namespace DefaultNamespace
{
    public interface IInteractable
    {
        public string InteractMessage { get; }
        void Interact();
        
        void BeginInteract();
        void EndInteract();
    }
}
