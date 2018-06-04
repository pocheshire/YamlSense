using System;
using MonoDevelop.Ide.CodeCompletion;

namespace YamlSense.VSMac.Completion.CompletionCategory
{
    public class YamlFileCompletionCategory : MonoDevelop.Ide.CodeCompletion.CompletionCategory
    {
        public YamlFileCompletionCategory(string categoryName)
            : base (categoryName, string.Empty)
        {
            
        }

        protected YamlFileCompletionCategory(string displayText, string icon)
            : base(displayText, icon)
        {
        }

        public override int CompareTo(MonoDevelop.Ide.CodeCompletion.CompletionCategory other)
        {
            return string.Compare(DisplayText, other.DisplayText, StringComparison.InvariantCulture);
        }
    }
}
