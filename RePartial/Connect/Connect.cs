using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PartialVS
{
	public static partial class Connect
	{
		public static void Initialize(DTE2 vs)
		{
			VS = (DTE2)vs;

			winEvents = VS.Events.WindowEvents;
			docEvents = VS.Events.DocumentEvents;
			buildEvents = VS.Events.BuildEvents;

			docEvents.DocumentOpened += onDocumentOpened;
			docEvents.DocumentSaved += onDocumentSaved;
		}

		private static void onDocumentOpened(Document document)
		{
			try
			{
				if (isDestination(document))
				{
					MessageBox.Show(openDestinationMessage, "Ooops!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
			}
			catch
			{
			}
		}

		private static void onDocumentSaved(Document document)
		{
			try
			{
				FileName sourceFileName = new FileName(document);

				if (isDestination(document))
				{
					MessageBox.Show(savingDestinationMessage, "Ooops!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				else if (sourceFileName.Extension == "partial")
				{
					parsePartialSettingsFile(document);
				}
				else if (shouldCreateSettingsFile(document))
				{
					createPartialSettingsFile(document);
				}
				else
				{
					string partialFileName = getPartialParent(document);
					if (partialFileName != null)
					{
						updateOriginalFile(sourceFileName, partialFileName);
					}
				}
			}
			catch (Exception ex)
			{
				outputPartial("Error: " + ex);
			}
		}

		private static bool isDestination(Document doc)
		{
			ProjectItem projectItem = doc.ProjectItem as ProjectItem;

			if (projectItem == null)
				return false;

			try
			{
				List<ProjectItem> childrenItems = projectItem.ProjectItems.OfType<ProjectItem>().ToList();
				int countOfPartial = childrenItems.Count(ci => ci.Name.EndsWith(".partial"));

				return countOfPartial == 1;
			}
			catch
			{
				return false;
			}
		}

		private static void updateOriginalFile(FileName sourceFileName, string partialFileName)
		{
			List<string> parts =
				File.ReadAllLines(partialFileName)
				.Select(p => p.Trim())
				.Where(p => p.Length > 0).ToList();

			string originalFileNamePath = sourceFileName.OriginalFileName;
			FileName originalFileName = new FileName(originalFileNamePath);

			outputPartial(string.Format("Creating {0} from:", originalFileName.FullPath));

			using (StreamWriter dest = new StreamWriter(originalFileName.FullPath, false, Encoding.UTF8))
			{
				foreach (string partName in parts)
				{
					string partFileName = originalFileName.Combine(partName);
					output(string.Format("\t\t - {0}", partFileName));

					using (StreamReader source = new StreamReader(partFileName, Encoding.Default))
						dest.WriteLine(source.ReadToEnd());
				}
			}
		}

		private static string getPartialParent(Document document)
		{
			try
			{
				if (document.FullName.EndsWith(".partial"))
					return null;

				ProjectItem parentItem = document.ProjectItem.Collection.Parent as ProjectItem;

				string partialPath = parentItem.get_FileNames(0) + ".partial";

				if (File.Exists(partialPath))
					return partialPath;
			}
			catch { }

			return null;
		}

		private static bool shouldCreateSettingsFile(Document document)
		{
			try
			{
				using (StreamReader reader = new StreamReader(document.FullName))
				{
					string firstLine = reader.ReadLine();
					if (firstLine.Trim().StartsWith("//") && firstLine.Trim().ToLower().EndsWith("partialize!"))
						return true;
				}
				return false;
			}
			catch
			{
				return false;
			}
		}

		private static void parsePartialSettingsFile(Document document)
		{
			try
			{
				List<string> parts =
					File.ReadAllLines(document.FullName)
					.Select(p => p.Trim())
					.Where(p => p.Length > 0).ToList();

				FileName originalFileName = new FileName(document.FullName.Replace(".partial", ""));
				ProjectItem originalProjectItem = VS.Solution.FindProjectItem(originalFileName.FullPath);

				foreach (string part in parts)
				{
					string partPath = originalFileName.Combine(part);

					if (File.Exists(partPath))
					{
						ProjectItem existingItem = VS.Solution.FindProjectItem(partPath);
						if (existingItem == null)
						{
							originalProjectItem.ProjectItems.AddFromFile(partPath);
						}
					}
					else
					{
						File.WriteAllText(partPath, "");
						originalProjectItem.ProjectItems.AddFromFile(partPath);
					}
				}

				originalProjectItem.ExpandView();
				originalProjectItem.ContainingProject.Save();
			}
			catch
			{
				outputPartial("ERROR: couldn't find the parent item.");
			}
		}

		private static void createPartialSettingsFile(Document document)
		{
			string partialFileName = document.FullName + ".partial";
			string partialShortFileName = document.Name + ".partial";

			// Check if partial already exists
			if (File.Exists(partialFileName))
			{
				outputPartial(string.Format("File {0} already exists.", partialShortFileName));
				return;
			}

			List<string> lines = File.ReadAllLines(document.FullName).Skip(1).ToList();

			outputPartial(string.Format("Creating {0}.", partialShortFileName));
			output("\t\tWrite the partial names, each in its own line. Then save the file.");

			string partialFileContents = "";

			if (lines.Count > 0)
			{
				partialFileContents = "original";

				string originalFileContents = string.Join(Environment.NewLine, lines);
				FileName originalFileName = new FileName(document);
				string partialOriginalPath = originalFileName.Combine("original");

				createAndInsertIntoProjectItem(partialOriginalPath, originalFileContents, document);
			}

			createAndInsertIntoProjectItem(partialFileName, partialFileContents, document, true);

			// Close the main file.
			document.Close(vsSaveChanges.vsSaveChangesNo);

			// Rewrite the file to remove the partialize! line
			File.WriteAllLines(document.FullName, lines);
		}

		private static ProjectItem createAndInsertIntoProjectItem(string path, string contents, Document document, bool open = false)
		{
			outputPartial(string.Format("Creating: {0}", path));

			File.WriteAllText(path, contents);

			ProjectItem documentProjectItem = document.ProjectItem;
			ProjectItems listOfProjectItems = documentProjectItem.ProjectItems;
			ProjectItem createdItem = listOfProjectItems.AddFromFile(path);

			documentProjectItem.ExpandView();
			documentProjectItem.ContainingProject.Save();

			if (open)
			{
				Window window = createdItem.Open(Constants.vsViewKindCode);
				window.Visible = true;
			}

			createdItem.ExpandView();
			createdItem.ContainingProject.Save();

			return createdItem;
		}

		private static void outputPartial(string text)
		{
			output("PARTIAL: " + text, true);
		}

		private static void output(string text, bool newLine = true)
		{
			try
			{
				OutputWindow win = VS.Windows.Item(EnvDTE.Constants.vsWindowKindOutput).Object as OutputWindow;

				win.ActivePane.OutputString(text);
				if (newLine)
					win.ActivePane.OutputString(Environment.NewLine);

				win.ActivePane.Activate();
			}
			catch (Exception)
			{

			}
		}
	}
}