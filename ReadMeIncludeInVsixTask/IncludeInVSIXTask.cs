using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace ReadMeIncludeInVsixTask
{
    public class IncludeInVSIXTask : Task
    {
        public override bool Execute()
        {
            var solutionDirectory = GetSolutionDirectory();
            var readMePath = Path.Combine(solutionDirectory.FullName, "README.md");
            var readMe = File.ReadAllText(readMePath);
            var markdownDocument = Markdown.Parse(readMe, new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());
            var assets = markdownDocument.Descendants<LinkInline>().Where(linkInline => linkInline.IsImage && linkInline.Url != null && Uri.IsWellFormedUriString(linkInline.Url, UriKind.Relative));
            var readMeTaskItem = CreateTaskItem(readMePath);
            IncludesInVsix = assets.Select(asset => CreateTaskItem(GetAssetPath(solutionDirectory.FullName, asset.Url),asset.Url))
                .Append(readMeTaskItem).ToArray();
            return true;
        }

        private TaskItem CreateTaskItem(string assetPath,string assetUrl = "")
        {
            var taskItem = new TaskItem(assetPath);
            taskItem.SetMetadata("IncludeInVSIX", "true");
            var vsixSubPath = "ReadMeAndAssets";
            if (!String.IsNullOrEmpty(assetUrl))
            {
                vsixSubPath = Path.Combine(vsixSubPath, Path.GetDirectoryName(assetUrl));
            }
            taskItem.SetMetadata("VSIXSubPath", vsixSubPath);
            return taskItem;
        }

        private static string GetAssetPath(string solutionDirectoryPath, string assetPath)
        {
            assetPath = assetPath.Replace("/", "\\");
            return Path.Combine(solutionDirectoryPath, assetPath);
        }

        private static DirectoryInfo GetSolutionDirectory()
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var directory = new FileInfo(assemblyLocation).Directory;
            while(directory.Name != "FineCodeCoverage")
            {
                directory = directory.Parent;
            }

            return directory;
        }

        [Output]
        public ITaskItem[] IncludesInVsix { get; set; } = Array.Empty<ITaskItem>();
    }
}
