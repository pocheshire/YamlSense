using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MonoDevelop.Ide.CodeCompletion;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Editor.Extension;
using YamlSense.VSMac.Completion.Extensions;
using YamlSense.VSMac.Completion.Suggest;

namespace YamlSense.VSMac.Completion
{
    public class YamlEditorCompletionExtension : CompletionTextEditorExtension
    {
        private readonly SuggestProvider _suggestProvider = new SuggestProvider();

        public override bool KeyPress(KeyDescriptor descriptor)
        {
            var result = base.KeyPress(descriptor);
            if (descriptor.KeyChar == '"')
            {
                if (Editor.CaretOffset > 2 && (Editor.GetCharAt(Editor.CaretOffset - 2) == '=' || Editor.GetCharAt(Editor.CaretOffset - 1) == '"' || Editor.GetCharAt(Editor.CaretOffset - 1) == '('))
                {
                    var completionWidget = DocumentContext.GetContent<ICompletionWidget>();
                    ShowCompletion(
                        _suggestProvider.Complete(CurrentCompletionContext ?? completionWidget.CreateCodeCompletionContext(Editor.CaretOffset), new CompletionTriggerInfo(CompletionTriggerReason.CharTyped, descriptor.KeyChar))
                    );

                    System.Diagnostics.Debug.WriteLine($"YamlEditorCompletionExtension -> KeyPress -> ShowCompletion");
                }
            }
            return result;
        }

        // documentation: https://github.com/mono/monodevelop/blob/master/main/src/addins/CSharpBinding/MonoDevelop.CSharp.Completion/CSharpCompletionTextEditorExtension.cs
        // sources: https://github.com/mono/monodevelop/blob/master/main/src/core/MonoDevelop.Ide/MonoDevelop.Ide.Editor.Extension/CompletionTextEditorExtension.cs
        // sources: https://github.com/mono/monodevelop/blob/master/main/src/core/MonoDevelop.Ide/MonoDevelop.Ide.Editor.Extension/TextEditorExtension.cs
        // sources: https://github.com/dotnet/roslyn/blob/master/src/Workspaces/CSharp/Portable/Extensions/ContextQuery/CSharpSyntaxContext.cs
        // sources: https://github.com/dotnet/roslyn/blob/878ffad23b8b06cb229c9ab31eada7634a473508/src/Workspaces/Core/Portable/Shared/Extensions/SyntaxNodeExtensions.cs#L638
        public override Task<ICompletionDataList> HandleCodeCompletionAsync(CodeCompletionContext completionContext, CompletionTriggerInfo triggerInfo, CancellationToken token = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                System.Diagnostics.Debug.WriteLine($"YamlEditorCompletionExtension -> HandleCodeCompletionAsync");

                try
                {
                    if (!IsEditingInString('\0'))
                        return null;

                    return _suggestProvider.Complete(completionContext, triggerInfo);
                }
                catch (Exception)
                {
                    var sb = new StringBuilder()
                        .AppendLine($"Unexpected code completion exception.")
                        .AppendLine($"FileName: {DocumentContext.Name}")
                        .AppendLine($"Position: line={completionContext.TriggerLine} col={completionContext.TriggerLineOffset}")
                        .AppendLine($"Line text: {Editor.GetLineText(completionContext.TriggerLine)}");

                    System.Diagnostics.Debug.WriteLine(sb.ToString());

                    return null;
                }
            });
        }

        protected override bool IsActiveExtension()
        {
            return IsEditingInString(Editor.GetCharAt(Editor.CaretOffset));
        }

        private bool IsEditingInString(char keyChar)
        {
            var currentLine = Editor.GetLineText(Editor.CaretLine);
            var lineText = currentLine.Length >= Editor.CaretColumn ? currentLine.Substring(0, Editor.CaretColumn - 1) : string.Empty;

            HandleStringContext();

            return lineText.Count(x => x == '"') == 1 || keyChar == '"';
        }

        private async void HandleStringContext()
        {
            var analysisDocument = DocumentContext.AnalysisDocument;
            if (analysisDocument != null)
            {
                var semanticModel = await analysisDocument.GetSemanticModelAsync(default(CancellationToken));

                var syntaxTree = semanticModel.SyntaxTree;

                var preProcessorTokenOnLeftOfPosition = syntaxTree.GetRoot().FindTokenOnLeftOfPosition(Editor.CaretOffset, includeDirectives: true);
            }
        }
    }
}
