using Crosstales.BWF;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DanielLochner.Assets
{
    public static class TextSanitizer
    {
        public static bool TrySanitize(string input, int maxLength, bool checkForProfanity, out string output)
        {
            string message = input.Trim();

            // Empty message
            if (string.IsNullOrEmpty(message))
            {
                output = null;
                return false;
            }

            // Max length
            if (message.Length > maxLength)
            {
                InformationDialog.Inform(LocalizationUtility.Localize("sent_message_too_long_title"), LocalizationUtility.Localize("sent_message_too_long_message", maxLength));
                output = null;
                return false;
            }

            // Profanity
            if (checkForProfanity)
            {
                var profanity = BWFManager.Instance.GetAll(message);
                if (profanity.Count > 0)
                {
                    InformationDialog.Inform(LocalizationUtility.Localize("profanity_detected_title"), LocalizationUtility.Localize("profanity_detected_message_terms", profanity.JoinAnd()));
                    output = null;
                    return false;
                }
            }

            // Rich text tags
            message = message.Trim().NoParse();

            output = message;
            return true;
        }
    }
}