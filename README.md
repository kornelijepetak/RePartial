# RePartial

Visual Studio plugin that lets you generate a single file from a bunch of other files, all neatly foldable in the Solution Explorer. 

### Download

You can download the .VSIX here: [RePartial Visual Studio Extension](https://github.com/kornelijepetak/RePartial/blob/master/artifacts/RePartial.vsix?raw=true).

## Instructions

**Step 1.** Open a file you want to generate from other partial files, for example myLib.js.

**Step 2.** Write `// PARTIALIZE!` in the first line.

**Step 3.** Save the file.

At this point, RePartial will take the original content (if any) and store it as myLib.js.original. It will also add a myLib.js.partial file. This configuration file contains all the parts from which myLib.js is generated.

![partial-content](https://raw.githubusercontent.com/kornelijepetak/RePartial/master/artifacts/partial-content.jpg)

![solution-explorer](https://raw.githubusercontent.com/kornelijepetak/RePartial/master/artifacts/solution-explorer.jpg)

## Operations

**Saving.** Every time you save the .partial file or any of the parts, the main (myLib.js) file will be regenerated.

**Adding.** If you want to **`add`** a new part, simply write another line into the .partial file and save it. A new file with appropriate name will be created.

**Deleting.** If you want to **`delete`** a part, simply delete the corresponding line from the .partial file and delete the file from Solution Explorer.

## Additional notes

`NOTE!` myLib.js is just an example. This works on any type of file for which the order of files is relevant. If you are doing this on files that are normally compiled, such as C#, you will then have two versions of the same code. In that case, set the `Build Action` to `None`

`NOTE!` For JavaScript libraries it is advisable to use modules (and module loaders) and not stitch files together this way. However, there is some legacy code which can use this extension even for JS.
