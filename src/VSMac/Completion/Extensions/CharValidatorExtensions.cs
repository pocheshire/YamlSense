using System;

namespace YamlSense.VSMac.Completion.Extensions
{
    public static class CharValidatorExtensions
    {
        private const char dot = '.';

        public static bool IsCharValid(this char charToValidate)
        {
            return char.IsLetterOrDigit(charToValidate) || charToValidate == dot;
        }
    }
}
