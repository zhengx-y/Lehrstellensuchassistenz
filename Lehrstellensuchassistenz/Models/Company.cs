using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lehrstellensuchassistenz.Models
{
    public class Company : INotifyPropertyChanged
    {
        private string? name;
        public string? Name
        {
            get => name;
            set
            {
                if (name == value) return; // Dirty Check
                name = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanApply));
                UpdateTimestamp();
            }
        }

        private string? website;
        public string? Website
        {
            get => website;
            set
            {
                if (website == value) return; // Dirty Check
                website = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanApply));
                UpdateTimestamp();
            }
        }

        private ApplicationStatus status;
        public ApplicationStatus Status
        {
            get => status;
            set
            {
                if (status == value) return; // Dirty Check
                status = value;
                OnPropertyChanged();
                UpdateTimestamp();
            }
        }

        private string? notesXaml;
        public string? NotesXaml
        {
            get => notesXaml;
            set
            {
                if (notesXaml == value) return; // Dirty Check
                notesXaml = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanApply));
                UpdateTimestamp();
            }
        }

        private string? photoReference;
        public string? PhotoReference
        {
            get => photoReference;
            set
            {
                if (photoReference == value) return; // Dirty Check
                photoReference = value;
                OnPropertyChanged();
                UpdateTimestamp();
            }
        }

        private string? _lastApplicationPath;
        public string? LastApplicationPath
        {
            get => _lastApplicationPath;
            set
            {
                if (_lastApplicationPath == value) return; // Dirty Check
                _lastApplicationPath = value;
                OnPropertyChanged();
                UpdateTimestamp();
            }
        }

        private DateTime createdAt = DateTime.Now;
        public DateTime CreatedAt
        {
            get => createdAt;
            set
            {
                if (createdAt == value) return;
                createdAt = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CreatedAtFormatted));
            }
        }

        public string CreatedAtFormatted => CreatedAt.ToString("dd.MM.yyyy HH:mm");

        private DateTime lastModified = DateTime.Now;
        public DateTime LastModified
        {
            get => lastModified;
            set
            {
                if (lastModified == value) return;
                lastModified = value;
                OnPropertyChanged();
            }
        }

        public void UpdateTimestamp()
        {
            lastModified = DateTime.Now;
            OnPropertyChanged(nameof(LastModified));
        }

        public bool CanApply =>
            !string.IsNullOrWhiteSpace(Name) &&
            !string.IsNullOrWhiteSpace(Website) &&
            !string.IsNullOrWhiteSpace(NotesXaml);

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public enum TippsType
        {
            Bewerbungstipps,
            Lebenslauftipps
        }
    }
}