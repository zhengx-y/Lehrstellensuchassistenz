# Lehrstellensuchassistenz (Apprenticeship Search Assistant)

> **"You don’t need more motivation. You need less friction."** – *Atomic Habits*

Lehrstellensuchassistenz is a specialized Windows desktop application designed to support youth, adults, and school leavers during their career integration process. Created to act as a centralized hub for apprenticeship hunting, this tool minimizes the daily friction of managing applications by organizing company data, custom notes, and application statuses in one automated space.

---

## 🧠 Core Features & Logic

* **Centralized Company Hub:** Track company names, application links, customized notes (supporting rich text and image screenshots), and individual statuses.
* **Frictionless Workflow:** Configurable autostart settings ensure the program launches directly as you boot up your PC, encouraging instant daily progress.
* **Dynamic Document Automation:** Quick access to localized Word application/CV templates that dynamically adapt to your selected files.
* **Bulk Management (QoL):** Multi-selection checkboxes allow users to bulk-delete or bulk-update statuses efficiently from the main interface.
* **Clean Sorting System:** Sort items by Name (A-Z/Z-A), Creation Date, Last Modified, or utilize intelligent status grouping (pushing "Rejected" or "No Response" entries to the bottom).
* **Data Privacy & Clean Sweep:** All data is saved locally via JSON within `AppData`. The integrated settings window features a "Full Wipe" button to completely purge all local files, registry keys, and autostart tasks in a single click.

---

## 🛠 Tech Stack

* **Language:** C# (.NET)
* **UI Framework:** WPF (Windows Presentation Foundation)
* **Data Persistence:** JSON (`System.Text.Json`)
* **Localization:** Multi-language support (English & German via `.resx` files)

---

## 📈 Development Roadmap & Changelog

### Logs 1 to 2: The Foundation (v0.1.0-alpha)
* Established local JSON storage architecture (`lehrstellen.data.json`).
* Designed the core layout utilizing WPF DataBinding and `ObservableCollection` for responsive interface updates.
* Implemented essential Fields: Company Name, Link, and basic Enum-based Application Statuses.

### Logs 3 to 8: Detail Expansion & Refactoring (v1.0.0)
* Added the detail-view screen featuring a direct "Delete" mechanism, embedded notes, and creation timestamps.
* Improved user flow by allowing localized UI zoom, automatic creation of desktop shortcuts upon initial launch, and automated closure of auxiliary windows.
* **Visual Polish:** Integrated specialized UI layer architecture, soft drop-shadows, layout rounding, and crisp text rendering filters. Fully color-coded status attributes:
    * ⚪ *Unapplied / Saved for later*
    * 🔵 *Applied (Awaiting Response)*
    * 🔴 *Rejected*
    * 🟢 *Internship / Next Steps*
    * ❇️ *Accepted*

### Logs 9 to 10: Rich Media & Advanced Sorting (v1.2.0)
* Upgraded the core text editor to a `RichTextBox`, adding native support for pasting screenshots directly into company notes with smart line breaks.
* Upgraded sorting behaviors, allowing complex filtering combinations (e.g., forcing "Unapplied" positions to the top while pushing stagnant requests downwards).
* Refactored workspace control patterns: clicking the UI canvas background clears keyboard focus from input fields to immediately reactivate global system shortcuts.

### Logs 11 to 15: Architecture Overhaul & Bulk Actions (v2.1.0)
* **Solid Principles:** Deconstructed massive "God Classes" into clean, dedicated domains:
    * `/Models`: Base properties (`Company.cs`, enums)
    * `/Data`: IO management (`CompanyRepository.cs`, `CompanySorter.cs`)
    * `/Services`: Hardware utilities (`RegistryService.cs`, `ShortcutManager.cs`, `FileService.cs`)
* Fully internationalized the naming conventions and code architecture to English.
* Added full multi-selection checkboxes allowing quick batch adjustments.

### Logs 16 to 19: Localization & File Logic Polish (v3.1.1 - Current)
* Migrated all user-facing hardcoded strings into `.resx` resource packs for native **English** and **German** execution.
* Resolved layout sizing conflicts inside dropdown components and fixed formatting rules regarding automated document duplicate handlers.
* **File Name Preservation:** Optimized the CV file handler utility to extract and use the selected document's original file name dynamically rather than overwriting it with a generic placeholder prefix.

---

## 🚀 Future Horizons

* [ ] Re-architect the desktop client into a modern web-based utility.
* [ ] Direct integration with local public employment engines (AMS API systems).
