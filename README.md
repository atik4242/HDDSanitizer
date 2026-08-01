<p align="center">
  <img src="logo.svg" width="140" height="140" alt="HDD Sanitizer Logo"/>
  <h1 align="center">🪓 HDD Sanitizer Enterprise</h1>
</p>

Eine moderne C# / Avalonia UI Anwendung zur sicheren und zertifizierten Datenlöschung von Festplatten (HDD/SSD/NVMe) unter Verwendung der Seagate openSeaChest CLI.

## ✨ Features
* 🔍 **Hardware-Erkennung:** Automatische Auslesung von angeschlossenen Laufwerken (WMI).
* 🛡️ **Safety Guard:** Automatische Sperre von Systemlaufwerken (Windows-Partitionen).
* ⚙️ **Low-Level Erasure:** Unterstützung für Native Sanitize, Zero-Fill & Crypto Erase.
* 📺 **Live-Monitoring:** Terminal-Output & Fortschrittsabfrage laufender Löschvorgänge.
* 📜 **Audit Trail & PDF Exporter:** Automatische JSON-Zertifikate und professioneller PDF-Export.

---

## 🚀 Vorbereitung & Installation

### 1. openSeaChest CLI herunterladen
Da openSeaChest ein externes Open-Source-Tool von Seagate ist, wird es nicht mit diesem Repository ausgeliefert.

1. Lade die neueste Windows-Version von GitHub (Seagate/openSeaChest) herunter.
2. Kopiere die Datei openSeaChest_Erase.exe direkt in den Hauptordner dieses Projekts.

### 2. Anwendung als Administrator starten
Da die App direkten Hardware-Zugriff auf Laufwerkscontroller benötigt, muss sie mit Administratorrechten gestartet werden:

dotnet run --project src/HddSanitizer.App/HddSanitizer.App.csproj
