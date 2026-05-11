using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lehrstellensuchassistenz
{
    public enum Status
    {
        Unbeworben,
        Beworben,
        KeineAntwort,
        Abgelehnt,
        NächsteSchritte,
        Praktikum,
        Angenommen
    }

    // Hilfsklasse für ComboBox ItemsSource
    public static class StatusValues
    {
        public static Array All => Enum.GetValues(typeof(Status));
    }

    // Unternehmen-Klasse
    public class Unternehmen : INotifyPropertyChanged
    {
        private string? name;
        public string? Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(KannSichBewerben)); // Wichtig!
            }
        }

        private string? website;
        public string? Website
        {
            get => website;
            set
            {
                website = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(KannSichBewerben)); // Wichtig!
            }
        }

        private Status status;
        public Status BewerbungsStatus
        {
            get => status;
            set
            {
                status = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(KannSichBewerben)); // Optional, falls Status Einfluss haben soll
            }
        }

        private string? notizen;
        public string? Notizen
        {
            get => notizen;
            set
            {
                notizen = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(KannSichBewerben));
            }
        }

        private string? fotoReferenz;
        public string? FotoReferenz
        {
            get => fotoReferenz;
            set
            {
                fotoReferenz = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(KannSichBewerben));
            }
        }

        private string? _letzteBewerbungPfad;

        public string? LetzteBewerbungPfad
        {
            get => _letzteBewerbungPfad;
            set
            {
                _letzteBewerbungPfad = value;
                OnPropertyChanged();
            }
        }

        private DateTime erstellDatum = DateTime.Now;
        public string ErstellDatumMitZeit => ErstellDatum.ToString("dd.MM.yyyy HH:mm");
        public DateTime ErstellDatum
        {
            get => erstellDatum;
            set { erstellDatum = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool KannSichBewerben =>
            !string.IsNullOrWhiteSpace(Name) &&
            !string.IsNullOrWhiteSpace(Website) &&
            !string.IsNullOrWhiteSpace(Notizen) &&
            !string.IsNullOrWhiteSpace(FotoReferenz);
    }
}