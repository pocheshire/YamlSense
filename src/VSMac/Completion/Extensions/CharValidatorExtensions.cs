using System;

namespace YamlSense.VSMac.Completion.Extensions
{
    public static class CharValidatorExtensions
    {
        private const char dot = '.';
        private const char quote = '"';

        public static bool IsCharValid(this char charToValidate)
        {
            return char.IsLetterOrDigit(charToValidate) || charToValidate == dot || charToValidate == quote;
        }
    }
}
