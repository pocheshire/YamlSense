using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            System.Diagnostics.Debug.WriteLine($"YamlEditorCompletionExtension -> KeyPress -> keyChar: {descriptor.KeyChar}{Environment.NewLine}");

            var isShowingNeeded = false;

            if (descriptor.SpecialKey == SpecialKey.None)
                isShowingNeeded = descriptor.KeyChar.IsCharValid() &&
                                  IsEditingInString(descriptor.KeyChar) &&
                                  !CompletionWindowManager.IsVisible;
            
            var result = base.KeyPress(descriptor);

            if (isShowingNeeded)
            {
                var completionWidget = DocumentContext.GetContent<ICompletionWidget>();
                ShowCompletion(
                    _suggestProvider.Complete(CurrentCompletionContext ?? completionWidget.CreateCodeCompletionContext(Editor.CaretOffset), new CompletionTriggerInfo(CompletionTriggerReason.CharTyped, descriptor.KeyChar))
                );

                System.Diagnostics.Debug.WriteLine($"YamlEditorCompletionExtension -> KeyPress -> ShowCompletion");
            }

            return result;
        }

        //documentation: https://github.com/mono/monodevelop/blob/master/main/src/addins/CSharpBinding/MonoDevelop.CSharp.Completion/CSharpCompletionTextEditorExtension.cs
        //sources: https://github.com/mono/monodevelop/blob/master/main/src/core/MonoDevelop.Ide/MonoDevelop.Ide.Editor.Extension/CompletionTextEditorExtension.cs
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

        private bool IsEditingInString(char keyChar)
        {
            var currentLine = Editor.GetLineText(Editor.CaretLine);
            var lineText = currentLine.Length >= Editor.CaretColumn ? currentLine.Substring(0, Editor.CaretColumn - 1) : string.Empty;

            return lineText.Count(x => x == '"') == 1 || keyChar == '"';
        }
    }
}
