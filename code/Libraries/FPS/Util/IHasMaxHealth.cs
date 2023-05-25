using Sandbox;

namespace Amper.FPS;

public interface IHasMaxHealth : IValid
{
	public float Health { get; set; }
	public float MaxHealth { get; set; }
}
