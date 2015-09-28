using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using System.IO;

namespace PartialVS
{
	class FileName
	{
		public FileName(Document document)
		{
			FullPath = document.FullName;
			Extension = Path.GetExtension(document.FullName).Replace(".", "");
			Name = Path.GetFileNameWithoutExtension(document.FullName);
			Directory = Path.GetDirectoryName(document.FullName);
		}

		public FileName(string path)
		{
			FullPath = path;
			Extension = Path.GetExtension(path).Replace(".", "");
			Name = Path.GetFileNameWithoutExtension(path);
			Directory = Path.GetDirectoryName(path);
		}

		public string Combine(string name)
		{
			return string.Format("{0}\\{1}.{2}.{3}", Directory, Name, name, Extension);
		}

		public string OriginalFileName
		{
			get
			{
				int lastPeriodIndex = Name.LastIndexOf(".");
				string originalNamePart = Name.Substring(0, lastPeriodIndex);
				return string.Format("{0}\\{1}.{2}", Directory, originalNamePart, Extension);
			}
		}

		public string FullPath { get; private set; }
		public string Extension { get; private set; }
		public string Name { get; private set; }
		public string Directory { get; private set; }
	}
}
