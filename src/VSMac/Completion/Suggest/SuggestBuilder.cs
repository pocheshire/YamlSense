using System.Collections.Generic;
using System.IO;
using System.Linq;
using FriendlyLocale.Parser;
using MonoDevelop.Ide;
using MonoDevelop.Ide.CodeCompletion;
using YamlSense.VSMac.Completion.CompletionCategory;

namespace YamlSense.VSMac.Completion.Suggest
{
    public interface ISuggestBuilder
    {
        CompletionDataList BuildOrGetList();

        void RebuildList();
    }

    public class SuggestBuilder : ISuggestBuilder
    {
        private CompletionDataList _completitionDataList;

        private static ISuggestBuilder _instance;
        public static ISuggestBuilder Instance => (_instance ?? (_instance = new SuggestBuilder()));

        public CompletionDataList BuildOrGetList()
        {
            return _completitionDataList?.Any() == true ? 
                                         _completitionDataList 
                                             :
                                         (_completitionDataList = GenerateCompletionDataList());
        }

        private CompletionDataList GenerateCompletionDataList()
        {
            var solutionProjects = IdeApp.ProjectOperations.CurrentSelectedSolution.Items;

            var completitionDataList = new CompletionDataList();

            foreach (var project in solutionProjects)
            {
                var files = project.GetItemFiles(true);
                var yamlFiles =
                    from file in files
                    where file.Extension == ".yaml"
                    select file.FullPath;

                var fileContents = new List<string>();

                foreach (var yamlFile in yamlFiles)
                    fileContents.Add(File.ReadAllText(yamlFile.FullPath.ToString()));

                var parser = new YParser(fileContents.ToArray());

                completitionDataList.AddRange(
                    parser.map.Select(
                        x => new CompletionData(x.Key) 
                        {
                            DisplayFlags = DisplayFlags.DescriptionHasMarkup, 
                            CompletionCategory = new YamlFileCompletionCategory(project.Name),
                            //Description = project.Name
                    Description = $"<markup><tt>{project.Name}</tt> – <big>{x.Value}</big></markup>"
                        }
                    )
                );
            }

            return completitionDataList;
        }

        public void RebuildList()
        {
            _completitionDataList = GenerateCompletionDataList();
        }
    }
}
