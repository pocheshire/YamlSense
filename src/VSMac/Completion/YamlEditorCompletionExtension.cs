using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.CodeCompletion;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Editor.Extension;

namespace YamlSense.VSMac.Completion
{
    public class YamlEditorCompletionExtension : CompletionTextEditorExtension
    {
        private readonly List<FilePath> _yamlFiles = new List<FilePath>();

        #region Init

        protected override void Initialize() // 2 
        {
            var projects = IdeApp.ProjectOperations.CurrentSelectedSolution.Items;

            foreach (var project in projects)
            {
                var files = project.GetItemFiles(true);
                var projectYamls =
                    from file in files
                    where file.Extension == ".yaml"
                    select file.FullPath;

                _yamlFiles.AddRange(projectYamls);
            }

            base.Initialize();
        }

        #endregion

        #region Code completion

        public override bool IsValidInContext(DocumentContext context) // 1
        {
            return true; // base.IsValidInContext(context);
        }

        public override bool KeyPress(KeyDescriptor descriptor) // 9
        {
            var result = base.KeyPress(descriptor);
            System.Diagnostics.Debug.WriteLine($"KeyPress {result}");
            return result;
        }

        protected override void OnCompletionContextChanged(object o, EventArgs a) // 6
        {
            base.OnCompletionContextChanged(o, a);
        }

        protected override bool IsActiveExtension() // 7
        {
            return _yamlFiles?.Any() ?? false;
        }

        public override bool CanRunCompletionCommand()
        {
            return true;// base.CanRunCompletionCommand();
        }

        public override Task<ICompletionDataList> HandleCodeCompletionAsync(CodeCompletionContext completionContext, CompletionTriggerInfo triggerInfo, CancellationToken token = default(CancellationToken))
        {
            var result = new CompletionDataList(new List<CompletionData>()
            {
                new CompletionData("Display text 1", IconId.Null, "Description 1", "Completion text 1"),
                new CompletionData("Display text 2", IconId.Null, "Description 2", "Completion text 2"),
                new CompletionData("Display text 3", IconId.Null, "Description 3", "Completion text 3"),
                new CompletionData("Display text 4", IconId.Null, "Description 4", "Completion text 4")
            });

            result.AddKeyHandler(new SuggestionKeyHandler());
            result.AutoCompleteUniqueMatch = false;
            result.AutoCompleteEmptyMatch = false;
            result.AutoSelect = true;

            return Task.FromResult<ICompletionDataList>(result);
        }

        class SuggestionKeyHandler : ICompletionKeyHandler
        {
            public bool PostProcessKey(CompletionListWindow listWindow, KeyDescriptor descriptor, out KeyActions keyAction)
            {
                var key = descriptor.SpecialKey;
                var keyChar = descriptor.KeyChar;

                if (key == SpecialKey.Return)
                    keyAction = KeyActions.Complete;
                else if (key == SpecialKey.BackSpace)
                {
                    keyAction = KeyActions.None;
                    return false;
                }
                else if (keyChar != '\0')
                {
                    //keyAction = KeyActions.CloseWindow;
                    keyAction = KeyActions.Process;
                }
                else
                    keyAction = KeyActions.None;

                listWindow.PostProcessKeyEvent(descriptor);

                return true;
            }

            public bool PreProcessKey(CompletionListWindow listWindow, KeyDescriptor descriptor, out KeyActions keyAction)
            {
                keyAction = KeyActions.None;
                return false;
            }
        }

        protected override void HandlePositionChanged(object sender, EventArgs e) // 8
        {
            base.HandlePositionChanged(sender, e);
        }

        #endregion
    }
}
