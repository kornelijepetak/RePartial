using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using System.IO;
using System.Text;

namespace PartialVS
{
	public static partial class Connect 
	{
		private static string savingDestinationMessage =
@"You have saved the file that is usually automatically generated. While PartialVS believes you know what you are doing, it has its doubts.

If you want PartialVS to propagate the changes to the appropriate files, you have to bug the extension author to implement that feature. It is not difficult and he knows how to do it. But he's got other things on his mind.

But you can at least try. 
";

		private static string openDestinationMessage =
@"You have opened the file that is automatically generated. 

All changes made to the file will be overwritten if PartialVS regenerates the file. You should not modify this file manually. Instead, modify the source files.

Be smart, don't start!
";
	}
}