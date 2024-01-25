namespace EasyBuildSystem.Packages.Addons.AdvancedBuilding
{
    public enum InteractableType { BUILDABLE, CARRIABLE }

    public interface IInteractable
    {
        InteractableType InteractableType { get; }
    }
}