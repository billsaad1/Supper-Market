using System;
using System.Windows.Forms;
using Supermarket.BLL.Services;

namespace Supermarket.UI.Helpers
{
    public static class LanguageHelper
    {
        public static TranslationService TranslationService { get; set; }

        public static void ApplyLanguage(Control parent)
        {
            if (TranslationService == null) return;

            // Update Tag-based translation if key exists in Tag
            if (parent.Tag != null && !string.IsNullOrEmpty(parent.Tag.ToString()))
            {
                parent.Text = TranslationService.Translate(parent.Tag.ToString());
            }

            // Set RightToLeft based on language
            parent.RightToLeft = TranslationService.CurrentLanguage == Language.Arabic ? RightToLeft.Yes : RightToLeft.No;

            foreach (Control child in parent.Controls)
            {
                ApplyLanguage(child);
            }
        }
    }
}
