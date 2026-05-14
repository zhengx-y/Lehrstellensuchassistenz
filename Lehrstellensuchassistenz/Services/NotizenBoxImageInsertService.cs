using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Lehrstellensuchassistenz.Services
{
    internal class NotizenBoxImageInsertService
    {
        /// <summary>
        /// Prüft, ob ein Bild in der Zwischenablage liegt und fügt es formatiert in die RichTextBox ein.
        /// </summary>
        /// <param name="richTextBox">Die Ziel-RichTextBox</param>
        /// <param name="maxWidth">Die Breite, auf die das Bild skaliert werden soll</param>
        /// <returns>True, wenn ein Bild eingefügt wurde, sonst False.</returns>
        public bool HandleImagePaste(RichTextBox richTextBox, double maxWidth = 500)
        {
            if (Clipboard.ContainsImage())
            {
                // Umbruch davor
                richTextBox.CaretPosition.InsertLineBreak();
                richTextBox.CaretPosition = richTextBox.CaretPosition.GetNextInsertionPosition(LogicalDirection.Forward)
                                            ?? richTextBox.CaretPosition;

                // Bild einfügen
                richTextBox.Paste();

                // Bild skalieren
                ScaleLastInsertedImage(richTextBox, maxWidth);

                // Umbruch danach
                TextPointer nachBild = richTextBox.CaretPosition.GetNextInsertionPosition(LogicalDirection.Forward);
                if (nachBild != null)
                {
                    richTextBox.CaretPosition = nachBild;
                    richTextBox.CaretPosition.InsertLineBreak();
                    richTextBox.CaretPosition = richTextBox.CaretPosition.GetNextInsertionPosition(LogicalDirection.Forward)
                                                ?? richTextBox.Document.ContentEnd;
                }
                else
                {
                    richTextBox.CaretPosition.InsertLineBreak();
                    richTextBox.CaretPosition = richTextBox.Document.ContentEnd;
                }

                richTextBox.Focus();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sucht das zuletzt eingefügte Bild in der RichTextBox und setzt die Breite.
        /// </summary>
        private void ScaleLastInsertedImage(RichTextBox richTextBox, double maxWidth)
        {
            foreach (var block in richTextBox.Document.Blocks.Reverse())
            {
                if (block is Paragraph p)
                {
                    foreach (var inline in p.Inlines.Reverse())
                    {
                        if (inline is InlineUIContainer container && container.Child is Image img)
                        {
                            img.Width = maxWidth;
                            img.Height = double.NaN; // Seitenverhältnis beibehalten
                            img.Stretch = Stretch.Uniform;
                            container.BaselineAlignment = BaselineAlignment.Bottom;
                            img.UpdateLayout();
                            return;
                        }
                    }
                }
            }
        }
    }
}