# Graphium

**Integrovaný systém pro synchronizovaný sběr a vizualizaci biomedicínských a VR dat**

Graphium je desktopová WPF aplikace umožňující v reálném čase sbírat biologické signály z více zdrojů současně, synchronizovat je na společnou časovou osu a vizualizovat je vedle dat z prostředí virtuální reality. Systém vznikl v kontextu výzkumného projektu Neurologické kliniky FNO zaměřeného na využití VR v desenzitizační terapii.

---

## Obsah

- [Přehled](#přehled)
- [Architektura](#architektura)
- [Funkce](#funkce)
- [Požadavky](#požadavky)
- [Instalace](#instalace)
- [Konfigurace](#konfigurace)
- [Podporované zdroje dat](#podporované-zdroje-dat)
- [Export dat](#export-dat)
- [Autor](#autor)

---

## Přehled

Systém umožňuje korelovat fyziologické signály pacienta (ECG, EDA, RESP, …) s aktuálním stavem VR scény — například zjistit, zda nárůst elektrodermální aktivity odpovídá přiblížení stimulu ve virtuálním prostředí, nebo je pouze artefaktem pohybu hlavy.

Klíčovým technickým příspěvkem je **algoritmus interpolace časových razítek** mezi UDP pakety, který garantuje striktně monotónní časovou osu bez duplicit. Ověřeno na datasetu 31 měření s celkovým objemem přes **39,8 milionu vzorků**.

---

## Architektura

Projekt se skládá ze tří částí:

| Projekt | Popis |
|---|---|
| **DataHub** | .NET knihovna pro modulární sběr dat (PCAP, UDP, HTTP) |
| **Graphium** | Hlavní WPF aplikace — vizualizace, konfigurace, export (architektura MVVM-S) |
| **Graphium Live** | ASP.NET Core webová aplikace pro vzdálené monitorování přes prohlížeč |

Aplikace Graphium používá vzor **MVVM-S** (Model–View–ViewModel + Service Layer) s DI kontejnerem (`Microsoft.Extensions.DependencyInjection`). Každá služba je registrována jako Singleton a do ViewModelů vstupuje přes konstruktor.

---

## Funkce

- 📡 **Příjem dat z BIOPAC MP200** — zachytávání UDP paketů přes SharpPcap, dekódování interleaved Int16 vzorků
- 🥽 **Integrace s VR prostředím** — příjem JSON telemetrie z Unreal Engine / Unity přes HTTP POST
- ⌚ **Příjem dat z chytrých zařízení** — obecný UDP příjem (ověřeno na Garmin Venu 3)
- 🔄 **Synchronizace datových proudů** — interpolace časových razítek pro zdroje s různými vzorkovacími frekvencemi
- 📈 **Vizualizace v reálném čase** — grafy ~60 fps (ScottPlot), sdílená osa X, režim Follow, volitelná barevná paleta
- 💾 **Export do CSV** — průběžný zápis, finální uložení po skončení měření
- 📤 **UDP export** — odesílání dat do externích systémů v reálném čase (např. do Graphium Live nebo VR scény)
- 🗂️ **Správa konfigurace** — ukládání a načítání nastavení signálů a kanálů

---

## Požadavky

- **OS:** Windows 10 / 11 (64-bit)
- **.NET:** .NET 8 nebo novější
- **Npcap:** [npcap.com](https://npcap.com/) — nutný pro zachytávání PCAP paketů
- **Hardware:** BIOPAC MP200 *(volitelné)*

### NuGet závislosti

| Balíček | Použití |
|---|---|
| [SharpPcap](https://github.com/dotpcap/sharppcap) | Zachytávání síťových paketů |
| [ScottPlot](https://scottplot.net/) | Vykreslování grafů v WPF |
| [NLog](https://nlog-project.org/) | Logování |
| SignalR | Real-time streaming (Graphium Live) |

---

## Instalace

```bash
git clone <url-repozitáře>
cd graphium
dotnet build
dotnet run --project Graphium
```

> Pro zachytávání PCAP paketů nainstalujte **Npcap** a spusťte aplikaci s dostatečnými oprávněními (nebo povolte Npcap pro neprivilegované uživatele při instalaci).

---

## Konfigurace

Konfigurace se ukládá do `%AppData%\Graphium` a je editovatelná přímo v aplikaci přes **Data Acquisition → Preferences**.

| Parametr | Popis |
|---|---|
| Capture Device | Síťové rozhraní pro zachytávání |
| BIOPAC IP Address | IP adresa zařízení MP200 |
| UDP Packet Payload Size | Velikost datové části paketu MP200 |
| URI | HTTP endpoint pro VR data (např. `http://localhost:8888/`) |
| UDP Port | Port pro příjem UDP dat |
| Export Host / Port | Cíl UDP exportu do externího systému |

---

## Podporované zdroje dat

### BIOPAC MP200 — PCAP
Zařízení komunikuje přes UDP na portu `16010`. Aplikace zachytává pakety v promiskuitním režimu a dekóduje prokládané Int16 vzorky. Vzorkovací frekvence až **100 000 Hz / kanál**.

### VR prostředí — HTTP
VR aplikace odesílá JSON přes HTTP POST na endpoint aplikace. Příklad:

```json
{
  "spiders": [{ "id": 0, "size": 5, "level": 2, "speed": 3.14 }],
  "location": [512.3, 48.7, 23.1],
  "rotation": [-12.5, 94.3, 180.0]
}
```

### Chytrá zařízení — UDP
Obecný UDP příjem libovolného JSON payloadu. Příklad (Garmin Venu 3):

```json
{
  "sensor_id": "VENU3",
  "pulse_rate": 102,
  "timestamp": "2026-02-20 12:48:54.247535"
}
```

---

## Export dat

### CSV
Data jsou průběžně ukládána do `%AppData%\Graphium\Measurements\`. Po ukončení měření si uživatel zvolí výsledné umístění souboru. Časová osa je uložena jako unixový čas v mikrosekundách.

### UDP (Graphium Live)
Aktuální hodnoty všech signálů jsou odesílány ~100× za sekundu ve formátu:

```json
{ "t": 1234567890.123, "s": { "ECG": 1024, "EDA": 3.14, "RESP": -512 } }
```

---

## Autor

**Bc. Filip Huser** — Diplomová práce, VŠB-TU Ostrava, FEI, 2026  
Vedoucí práce: Mgr. Ing. Michal Krumnikl, Ph.D.
