# Graphium

> Integrated system for synchronized acquisition and visualization of biomedical and VR data.

Graphium is a desktop WPF application for real-time collection of biological signals from multiple sources simultaneously, synchronizing them onto a shared timeline and visualizing them alongside virtual reality environment data. The system was developed in the context of a research project at the Neurology Department of FNO focused on VR-based desensitization therapy.

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Configuration](#configuration)
- [Data Sources](#data-sources)
- [Data Export](#data-export)
- [Author](#author)

---

## Overview

Graphium enables correlation of patient physiological signals (ECG, EDA, RESP, …) with the current state of a VR scene — for example, determining whether a spike in electrodermal activity corresponds to a stimulus approaching in the virtual environment, or is merely a motion artifact.

The key technical contribution is a **timestamp interpolation algorithm** between UDP packets that guarantees a strictly monotonic timeline with no duplicates. Validated on a dataset of 31 recordings totaling over **39.8 million samples**.

---

## Architecture

The project consists of three components:

| Project | Description |
|---|---|
| **DataHub** | .NET library for modular data acquisition (PCAP, UDP, HTTP) |
| **Graphium** | Main WPF application — visualization, configuration, export (MVVM-S architecture) |
| **Graphium Live** | ASP.NET Core web application for remote browser-based monitoring |

Graphium uses the **MVVM-S** pattern (Model–View–ViewModel + Service Layer) with a DI container (`Microsoft.Extensions.DependencyInjection`). Each service is registered as a Singleton and injected into ViewModels via constructor injection.

---

## Features

- 📡 **BIOPAC MP200 acquisition** — UDP packet capture via SharpPcap, interleaved Int16 sample decoding
- 🥽 **VR environment integration** — JSON telemetry ingestion from Unreal Engine / Unity via HTTP POST
- ⌚ **Smart device support** — generic UDP receiver (validated on Garmin Venu 3)
- 🔄 **Multi-stream synchronization** — timestamp interpolation across sources with different sampling rates
- 📈 **Real-time visualization** — ~60 fps charts (ScottPlot), shared X axis, Follow mode
- 💾 **CSV export** — continuous write during session, final save on measurement end
- 📤 **UDP export** — live data forwarding to external systems (e.g. Graphium Live or VR scene)
- 🗂️ **Configuration management** — save and load signal and channel settings

---

## Requirements

- **OS:** Windows 10 / 11 (64-bit)
- **.NET:** .NET 8 or later
- **Npcap:** [npcap.com](https://npcap.com/) — required for PCAP packet capture
- **Hardware:** BIOPAC MP200 *(optional)*

### NuGet Dependencies

| Package | Purpose |
|---|---|
| [SharpPcap](https://github.com/dotpcap/sharppcap) | Network packet capture |
| [ScottPlot](https://scottplot.net/) | WPF chart rendering |
| [NLog](https://nlog-project.org/) | Logging |
| SignalR | Real-time streaming (Graphium Live) |

---

## Installation

```bash
git clone <repository-url>
cd graphium
dotnet build
dotnet run --project Graphium
```

> Install **Npcap** before first run. For PCAP capture, launch the application with elevated privileges or enable unprivileged mode during Npcap installation.

---

## Configuration

Configuration is stored in `%AppData%\Graphium` and can be edited directly in the application via **Data Acquisition → Preferences**.

| Parameter | Description |
|---|---|
| Capture Device | Network interface for packet capture |
| BIOPAC IP Address | IP address of the MP200 device |
| UDP Packet Payload Size | MP200 packet payload size |
| URI | HTTP endpoint for VR data (e.g. `http://localhost:8888/`) |
| UDP Port | Port for incoming UDP data |
| Export Host / Port | UDP export destination for external systems |

---

## Data Sources

### BIOPAC MP200 — PCAP

The device communicates over UDP on port `16010`. The application captures packets in promiscuous mode and decodes interleaved Int16 samples. Sampling rate up to **100,000 Hz per channel**.

### VR Environment — HTTP

The VR application sends JSON via HTTP POST to the application endpoint. Example payload:

```json
{
  "spiders": [{ "id": 0, "size": 5, "level": 2, "speed": 3.14 }],
  "location": [512.3, 48.7, 23.1],
  "rotation": [-12.5, 94.3, 180.0]
}
```

### Smart Devices — UDP

Generic UDP receiver for arbitrary JSON payloads. Example (Garmin Venu 3):

```json
{
  "sensor_id": "VENU3",
  "pulse_rate": 102,
  "timestamp": "2026-02-20 12:48:54.247535"
}
```

---

## Data Export

### CSV

Data is written continuously to `%AppData%\Graphium\Measurements\`. At the end of a session, the user selects the final file destination. The time axis is stored as Unix time in microseconds.

### UDP (Graphium Live)

Current values of all signals are sent ~100 times per second in the following format:

```json
{ "t": 1234567890.123, "s": { "ECG": 1024, "EDA": 3.14, "RESP": -512 } }
```

---

## Author

**Ing. Filip Huser** — Master's Thesis, VŠB-TU Ostrava, FEI, 2026  
Supervisor: Mgr. Ing. Michal Krumnikl, Ph.D.
