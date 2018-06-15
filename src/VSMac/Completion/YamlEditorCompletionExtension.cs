using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MonoDevelop.Ide.CodeCompletion;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Editor.Extension;
using YamlSense.VSMac.Completion.Extensions;
using YamlSense.VSMac.Completion.Suggest;

// documentation: https://github.com/mono/monodevelop/blob/master/main/src/addins/CSharpBinding/MonoDevelop.CSharp.Completion/CSharpCompletionTextEditorExtension.cs
// sources: https://github.com/mono/monodevelop/blob/master/main/src/core/MonoDevelop.Ide/MonoDevelop.Ide.Editor.Extension/CompletionTextEditorExtension.cs
// sources: https://github.com/mono/monodevelop/blob/master/main/src/core/MonoDevelop.Ide/MonoDevelop.Ide.Editor.Extension/TextEditorExtension.cs
// sources: https://github.com/dotnet/roslyn/blob/master/src/Workspaces/CSharp/Portable/Extensions/ContextQuery/CSharpSyntaxContext.cs
// sources: https://github.com/dotnet/roslyn/blob/878ffad23b8b06cb229c9ab31eada7634a473508/src/Workspaces/Core/Portable/Shared/Extensions/SyntaxNodeExtensions.cs#L638

namespace YamlSense.VSMac.Completion
{
    public class YamlEditorCompletionExtension : CompletionTextEditorExtension
    {
        private readonly SuggestProvider _suggestProvider = new SuggestProvider();

        private SemanticModel _semanticModel;

        protected override async void Initialize()
        {
            base.Initialize();

            var analysisDocument = DocumentContext.AnalysisDocument;
            if (analysisDocument != null)
                _semanticModel = await analysisDocument.GetSemanticModelAsync(default(CancellationToken));
        }

        public override bool KeyPress(KeyDescriptor descriptor)
        {
            var result = base.KeyPress(descriptor);

            if (descriptor.KeyChar == '"')
            {
                if (Editor.CaretOffset > 2 && 
                    (Editor.GetCharAt(Editor.CaretOffset - 1) == '"' || Editor.GetCharAt(Editor.CaretOffset) == '"') && 
                    ((Editor.GetCharAt(Editor.CaretOffset - 2) == '=' || Editor.GetCharAt(Editor.CaretOffset - 3) == '=') || (Editor.GetCharAt(Editor.CaretOffset - 2) == '(' || Editor.GetCharAt(Editor.CaretOffset - 1) == '(')))
                {
                    var completionWidget = DocumentContext.GetContent<ICompletionWidget>();
                    ShowCompletion(
                        _suggestProvider.Complete(CurrentCompletionContext ?? completionWidget.CreateCodeCompletionContext(Editor.CaretOffset), new CompletionTriggerInfo(CompletionTriggerReason.CharTyped, descriptor.KeyChar))
                    );

                    System.Diagnostics.Debug.WriteLine($"YamlEditorCompletionExtension -> KeyPress -> ShowCompletion");
                }
            }
            else if (descriptor.SpecialKey == SpecialKey.None && HandleStringContext())
            {
                var completionWidget = DocumentContext.GetContent<ICompletionWidget>();
                ShowCompletion(
                    _suggestProvider.Complete(CurrentCompletionContext ?? completionWidget.CreateCodeCompletionContext(Editor.CaretOffset), new CompletionTriggerInfo(CompletionTriggerReason.CharTyped, descriptor.KeyChar))
                );

                System.Diagnostics.Debug.WriteLine($"YamlEditorCompletionExtension -> KeyPress -> ShowCompletion");
            }

            return result;
        }

        private bool IsEditingInString(char keyChar)
        {
            var currentLine = Editor.GetLineText(Editor.CaretLine);
            var lineText = currentLine.Length >= Editor.CaretColumn ? currentLine.Substring(0, Editor.CaretColumn - 1) : string.Empty;

            HandleStringContext();

            return lineText.Count(x => x == '"') == 1 || keyChar == '"';
        }

        private bool HandleStringContext()
        {
            var handled = false;
            
            var analysisDocument = DocumentContext.AnalysisDocument;
            if (analysisDocument != null && analysisDocument.TryGetSemanticModel(out _semanticModel))
            {
                var syntaxTree = _semanticModel.SyntaxTree;

                var root = syntaxTree.GetRoot();

                var preProcessorTokenOnLeftOfPosition = root.FindTokenOnLeftOfPosition(Editor.CaretOffset, includeDirectives: true);

                handled = IsStringToken(preProcessorTokenOnLeftOfPosition);
            }

            return handled;
        }

        private static bool IsStringToken(SyntaxToken token)
        {
            return token.IsKind(SyntaxKind.StringLiteralToken)
                || token.IsKind(SyntaxKind.CharacterLiteralToken)
                || token.IsKind(SyntaxKind.InterpolatedStringStartToken)
                || token.IsKind(SyntaxKind.InterpolatedVerbatimStringStartToken)
                || token.IsKind(SyntaxKind.InterpolatedStringTextToken)
                || token.IsKind(SyntaxKind.InterpolatedStringEndToken);
        }
    }
}
