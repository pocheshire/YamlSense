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
            var result = base.KeyPress(descriptor);

            System.Diagnostics.Debug.WriteLine($"YamlEditorCompletionExtension -> KeyPress -> keyChar: {descriptor.KeyChar}");
            System.Diagnostics.Debug.WriteLine($"YamlEditorCompletionExtension -> KeyPress -> result: {result}");

            if (descriptor.SpecialKey == SpecialKey.None)
            {
                if (descriptor.KeyChar.IsCharValid() && IsEditingInString() && CompletionWindowManager.IsVisible)
                {
                    CompletionWindowManager.HideWindow();
                    RunCompletionCommand();
                }
            }

            return result;
        }

        //documentation: https://github.com/mono/monodevelop/blob/12f655f5320ae9407ad78e610df9085d5b0fc0e5/main/src/addins/CSharpBinding/MonoDevelop.CSharp.Completion/CSharpCompletionTextEditorExtension.cs
        public override Task<ICompletionDataList> HandleCodeCompletionAsync(CodeCompletionContext completionContext, CompletionTriggerInfo triggerInfo, CancellationToken token = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                System.Diagnostics.Debug.WriteLine($"YamlEditorCompletionExtension -> HandleCodeCompletionAsync");

                try
                {
                    if (!IsEditingInString())
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

        private bool IsEditingInString()
        {
            var line = Editor.GetLine(Editor.CaretLine);
            var lineText = Editor.GetTextAt(line.Offset, Editor.CaretColumn - 1);

            return lineText.Count(x => x == '"') == 1;
        }
    }
}
