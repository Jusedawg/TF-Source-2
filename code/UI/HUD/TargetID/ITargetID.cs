using Sandbox;

namespace TFS2;

public interface ITargetID : IValid
{
	public string Name { get; }
	public string Avatar { get; }
	public TFTeam Team { get; }
	public Entity Entity => this as Entity;
}
public interface ITargetIDSubtext
{
	public bool HasSubtext => !string.IsNullOrWhiteSpace( Subtext );
	public string Subtext { get; }
}

public interface IInteractableTargetID : ITargetID
{
	public bool CanInteract( TFPlayer user );
	public string InteractText { get; }
	public string InteractButton { get; }
}
