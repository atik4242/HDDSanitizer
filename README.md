# 🪓 HDD Sanitizer

An intuitive, modern, open-source desktop tool for discovering, analyzing, safely erasing, and auditing HDDs and SSDs. Built with **.NET 9** and **Avalonia UI**, leveraging Seagate's open-source openSeaChest framework under the hood.

---

## ⚡ Key Features

* **🔌 Device Discovery:** Automatic hardware recognition for SATA, NVMe, and USB storage devices.
* **🩺 SMART Diagnostics:** Quick view of health status, temperature, and power-on hours with raw JSON export.
* **🛡️ Bulletproof Safety Checks:** System drives are automatically protected from deletion. Requires explicit serial number re-typing before starting an erase procedure.
* **🧹 Erasure Modes:** Supports native ATA/NVMe Sanitize/Secure Erase commands alongside standard Zero-Fill Overwrites.
* **📜 Audit Certificates:** Automatically generates structured, tamper-evident JSON erasure logs (\.json\) containing pre- and post-erase device signatures.
* **🖥️ Clean UI:** Minimalist, clutter-free dashboard designed for clarity when handling multiple multi-terabyte drives.

---

## 🏗️ Architecture Overview

The project adheres to Clean Architecture principles to keep low-level CLI parsing isolated from core business rules and user interfaces:

\\\	ext
HDDSanitizer/
├── src/
│   ├── HddSanitizer.Domain/         # Core entities, Enums & Certificate models
│   ├── HddSanitizer.Core/           # Business logic, Safety guards, Interfaces
│   ├── HddSanitizer.SeaChest/       # Process runner & openSeaChest CLI wrappers
│   ├── HddSanitizer.Infrastructure/ # File system logging, JSON cert writers
│   └── HddSanitizer.App/            # Avalonia UI (MVVM) Desktop Application
└── tests/
    └── HddSanitizer.Tests/          # Unit & Integration tests (xUnit & NSubstitute)
\\\

---

## 🛠️ Tech Stack

* **Framework:** .NET 9 (C# 13)
* **UI Framework:** Avalonia UI (Cross-platform XAML)
* **MVVM Pattern:** ReactiveUI
* **Backend Utilities:** openSeaChest CLI Suite (\openSeaChest_Info\, \openSeaChest_Erase\)
* **Testing:** xUnit, NSubstitute

---

## 🚀 Getting Started

### Prerequisites

* .NET 9.0 SDK
* Administrator / Root privileges (required for direct drive access via SeaChest)

### Building from Source

1. **Clone the repository:**
   \\\ash
   git clone https://github.com/atik4242/HDDSanitizer.git
   cd HDDSanitizer
   \\\

2. **Restore dependencies & Build:**
   \\\ash
   dotnet build --configuration Release
   \\\

3. **Run Unit Tests:**
   \\\ash
   dotnet test
   \\\

---

## ⚖️ License

Distributed under the **MIT License**.
