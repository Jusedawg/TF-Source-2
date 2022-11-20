using Sandbox;

namespace TFS2;

abstract partial class Filter : Entity
{
	public virtual bool Test( Entity entity )
	{
		// By default everything passes!
		return true;
	}

	[Input]
	public void TestActivator( Entity Activator )
	{
		if ( Test( Activator ) ) OnPass.Fire( Activator );
		else OnFail.Fire( Activator );
	}

	protected Output OnPass { get; set; }
	protected Output OnFail { get; set; }
}
