using System;
using MonoDevelop.Ide.Editor.Extension;
using YamlSense.VSMac.Completion.Suggest;

namespace YamlSense.VSMac.Completion
{
    public class YamlEditorChangerExtension : TextEditorExtension
    {
        protected override void Initialize()
        {
            DocumentContext.Saved += DocumentContext_Saved;

            base.Initialize();
        }

        public override void Dispose()
        {
            DocumentContext.Saved -= DocumentContext_Saved;

            base.Dispose();
        }

        private void DocumentContext_Saved(object sender, EventArgs e)
            => SuggestBuilder.Instance.RebuildList();
    }
}
