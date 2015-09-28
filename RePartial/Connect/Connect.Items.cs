using EnvDTE;
using EnvDTE80;

namespace PartialVS
{
	public static partial class Connect 
	{
		private static DTE2 VS;

		private static BuildEvents buildEvents;
		private static DocumentEvents docEvents;
		private static WindowEvents winEvents;

	}
}