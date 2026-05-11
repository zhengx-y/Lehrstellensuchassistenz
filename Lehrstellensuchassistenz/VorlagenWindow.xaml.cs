using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Lehrstellensuchassistenz
{
    public partial class VorlagenWindow : Window
    {
        private Unternehmen _unternehmen;
        private UnternehmenElement _unternehmenElement;
        public string? GewaehlteDatei { get; private set; }

        private enum BewerbungsTyp
        {
            Generisch,
            Leer,
            Eigen
        }

        public VorlagenWindow(Unternehmen unternehmen, UnternehmenElement unternehmenElement)
        {
            InitializeComponent();
            _unternehmen = unternehmen;
            _unternehmenElement = unternehmenElement;
        }

        private void GenerischeVorlage_Click(object sender, RoutedEventArgs e)
        {
            Bewerben(BewerbungsTyp.Generisch);
        }

        private void LeeresDokument_Click(object sender, RoutedEventArgs e)
        {
            Bewerben(BewerbungsTyp.Leer);
        }
        private void EigeneVorlage_Click(object sender, RoutedEventArgs e)
        {
            Bewerben(BewerbungsTyp.Eigen);
        }

        private void EigeneVorlageHochladen_Click(object sender, RoutedEventArgs e)
        {
            // Ermögliche dem Benutzer, eine eigene Vorlage auszuwählen (hochladen)
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Eigene Vorlage hochladen",
                Filter = "Word-Dokumente (*.docx;*.doc)|*.docx;*.doc", // Filter für Word-Dateien
                Multiselect = false // Keine Mehrfachauswahl
            };

            if (dialog.ShowDialog() == true)
            {
                string eigeneVorlagePfad = dialog.FileName;

                // Validierung der Datei
                string extension = Path.GetExtension(eigeneVorlagePfad).ToLower();
                if (extension != ".docx" && extension != ".doc")
                {
                    MessageBox.Show("Bitte wählen Sie eine gültige Word-Vorlage (.docx oder .doc) aus.");
                    return;
                }

                // Bestimme den AppData-Pfad, um die Vorlage im "Lehrstellensuchassistenz" Ordner zu speichern
                string appDataOrdner = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Lehrstellensuchassistenz",
                    "user-files"
                );

                // Sicherstellen, dass der Ordner existiert
                Directory.CreateDirectory(appDataOrdner);

                // Bestimme den Dateinamen und Zielpfad
                string neueDateiName = "Eigen_Vorlage" + Path.GetExtension(eigeneVorlagePfad);
                string zielPfad = Path.Combine(appDataOrdner, neueDateiName);

                try
                {
                    // Kopiere die Datei in den Zielordner
                    File.Copy(eigeneVorlagePfad, zielPfad, true); // Überschreiben falls bereits vorhanden

                    // Optional: Pfad irgendwo speichern, falls nötig
                    // _unternehmen.LetzteBewerbungPfad = zielPfad;

                    // Zeige den Continue-Button **nicht** und speichere keinen Pfad
                    // Continue-Button und Pfad sollten nur beim Erstellen einer Bewerbung aktiviert werden
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Kopieren der Datei: " + ex.Message);
                }
            }
        }

        private void Bewerben(BewerbungsTyp typ)
        {
            try
            {
                string? quelleDatei = null;

                switch (typ)
                {
                    case BewerbungsTyp.Generisch:
                        quelleDatei = Path.Combine(
                            AppDomain.CurrentDomain.BaseDirectory,
                            "resources",
                            "word_doc",
                            "Bewerbung_Beispiel.docx"
                        );
                        break;

                    case BewerbungsTyp.Leer:
                        quelleDatei = Path.Combine(
                            AppDomain.CurrentDomain.BaseDirectory,
                            "resources",
                            "word_doc",
                            "empty.docx"
                        );
                        break;

                    case BewerbungsTyp.Eigen:
                        // Hier verwendest du die hochgeladene Vorlage direkt
                        string appDataOrdner = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            "Lehrstellensuchassistenz",
                            "user-files"
                        );

                        // Den Pfad zur hochgeladenen Vorlage holen
                        string eigeneVorlagePfad = Path.Combine(appDataOrdner, "Eigen_Vorlage.docx");

                        if (!File.Exists(eigeneVorlagePfad))
                        {
                            MessageBox.Show("Keine benutzerdefinierte Vorlage gefunden.");
                            return;
                        }

                        quelleDatei = eigeneVorlagePfad;
                        break;
                }

                // Wenn keine Vorlage gefunden wurde
                if (string.IsNullOrEmpty(quelleDatei) || !File.Exists(quelleDatei))
                {
                    MessageBox.Show("Keine gültige Vorlage ausgewählt.");
                    return;
                }

                // Bestimme den Bewerbungsordner im AppData-Verzeichnis
                string bewerbungsOrdner = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Lehrstellensuchassistenz",
                    "bewerbungen"
                );

                // Sicherstellen, dass der Bewerbungsordner existiert
                Directory.CreateDirectory(bewerbungsOrdner);

                // Erstelle einen eindeutigen Dateinamen für die Bewerbung
                string firmenName = _unternehmen?.Name ?? "Bewerbung";
                foreach (char c in Path.GetInvalidFileNameChars())
                    firmenName = firmenName.Replace(c, '_');

                string neueDateiName = $"{firmenName}_{DateTime.Now:yyyyMMdd}.docx";
                string zielPfad = Path.Combine(bewerbungsOrdner, neueDateiName);

                try
                {
                    // Kopiere die ausgewählte Vorlage in den Bewerbungsordner
                    File.Copy(quelleDatei, zielPfad, true);

                    // Pfad in Unternehmen speichern
                    _unternehmen.LetzteBewerbungPfad = zielPfad;

                    // Zeige den Continue-Button, da wir mit einer Vorlage weiterarbeiten
                    _unternehmenElement.ShowContinueButton();

                    // Öffne die Datei direkt in Word (nicht im Explorer)
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = zielPfad,
                        UseShellExecute = true,
                        Verb = "open" // sorgt dafür, dass die Datei direkt geöffnet wird
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler: " + ex.Message);
            }
        }
    }
}