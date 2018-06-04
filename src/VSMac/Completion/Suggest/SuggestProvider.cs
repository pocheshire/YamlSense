using MonoDevelop.Ide.CodeCompletion;
using YamlSense.VSMac.Completion.Extensions;

namespace YamlSense.VSMac.Completion.Suggest
{
    class SuggestProvider
    {
        private const int TRIGGERED_WORD_LENGTH = 1;

        public ICompletionDataList Complete(CodeCompletionContext completionContext, CompletionTriggerInfo triggerInfo)
        {
            CompletionDataList result = null;

            var filteredCompletionDataList = SuggestBuilder.Instance.BuildOrGetList(); //TODO

            switch (triggerInfo.CompletionTriggerReason)
            {
                case CompletionTriggerReason.CharTyped:
                    if (triggerInfo.TriggerCharacter.Value.IsCharValid())
                        result = new CompletionDataList(filteredCompletionDataList)
                        {
                            TriggerWordLength = TRIGGERED_WORD_LENGTH,
                            AutoCompleteUniqueMatch = false,
                            DefaultCompletionString = string.Empty,
                            AutoSelect = false
                        };
                    break;
                case CompletionTriggerReason.CompletionCommand:
                    result = new CompletionDataList(filteredCompletionDataList)
                    {
                        TriggerWordLength = 0,
                        AutoCompleteUniqueMatch = true,
                        DefaultCompletionString = string.Empty,
                        AutoSelect = false
                    };
                    break;
                case CompletionTriggerReason.BackspaceOrDeleteCommand:
                    result = null;
                    break;
                default:
                    result = new CompletionDataList();
                    break;
            }

            return result;
        }
    }
}
