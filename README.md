# Evacuation

System wskazywania drogi ewakuacji z budynku. Aplikacja mobilna wyznacza optymalną trasę do najbliższego wyjścia ewakuacyjnego na podstawie rzutu piętra, omijając strefy oznaczone jako niebezpieczne.

Projekt realizowany jako praca inżynierska — informatyka stosowana, Wydział Fizyki, Astronomii i Informatyki Stosowanej UMK w Toruniu.

## Jak to działa

1. Administrator budynku wgrywa rzuty pięter jako obrazy PNG/JPG w ustalonym kontrakcie kolorów
2. Backend przetwarza obraz: segmentuje pomieszczenia (flood fill), wykrywa ich sąsiedztwo i buduje graf, w którym wierzchołkami są pomieszczenia, a wagami krawędzi odległości między ich centroidami
3. Użytkownik lokalizuje budynek przez GPS, wybiera piętro i pomieszczenie startowe
4. Algorytm Dijkstry wyznacza najkrótszą trasę do dowolnego wyjścia ewakuacyjnego; strefy niebezpieczne podnoszą koszt przejścia zamiast całkowicie blokować krawędź, więc trasa przez zagrożenie pozostaje ostatecznością, a nie brakiem wyniku

Plany są dostarczane przez zarządców budynków, nie pobierane ze źródeł publicznych — OpenStreetMap i podobne bazy nie zawierają informacji o drzwiach, korytarzach ani klatkach schodowych.

## Kontrakt kolorów

Rzut piętra musi używać poniższych kolorów. Dzięki temu przetwarzanie sprowadza się do odczytu koloru piksela, bez progowania i odszumiania.

| Kolor | Hex | Znaczenie |
|---|---|---|
| Biały | `#FFFFFF` | podłoga / przejście |
| Czarny | `#000000` | ściana |
| Zielony | `#00FF00` | wyjście ewakuacyjne |
| Czerwony | `#FF0000` | strefa niebezpieczna |
| Niebieski | `#0000FF` | schody / winda |
| Żółty | `#FFFF00` | punkt startowy |

Pomieszczenia oznaczone jako schody łączą się z odpowiadającymi im pomieszczeniami na sąsiednich piętrach, co pozwala wyznaczać trasy międzypiętrowe.

## Architektura

```
┌─────────────────┐
│  .NET MAUI      │  aplikacja mobilna
└────────┬────────┘
         │ REST / JSON
┌────────▼────────┐
│ ASP.NET Core    │  Web API
│      API        │
└────────┬────────┘
         │
┌────────▼────────┐      ┌──────────────────┐
│  Serwis grafów  │      │   PostgreSQL     │
│  preprocessing  │◄────►│   EF Core        │
│  segmentacja    │      │   Code First     │
│  budowa grafu   │      └──────────────────┘
│  Dijkstra       │
└─────────────────┘
```

Orkiestracja środowiska deweloperskiego: **.NET Aspire**. AppHost uruchamia kontener PostgreSQL i API, wstrzykuje connection string i udostępnia dashboard z logami, metrykami i trace'ami.

## Stack

| Warstwa | Technologia |
|---|---|
| Mobilka | .NET MAUI |
| Backend | ASP.NET Core Web API |
| Orkiestracja | .NET Aspire |
| Baza danych | PostgreSQL |
| ORM | Entity Framework Core (Code First) |
| Przetwarzanie obrazu | własna implementacja w C# |
| Testy | xUnit |

Pipeline przetwarzania obrazu napisany jest od zera, bez bibliotek do wizji komputerowej. Zewnętrzny kod odpowiada wyłącznie za dekodowanie PNG/JPG do bitmapy.

## Struktura repozytorium

```
Backend/
  Evacuation.sln
  src/
    Evacuation.AppHost/          orkiestracja Aspire
    Evacuation.ServiceDefaults/  telemetria, health checks, discovery
    Evacuation.Domain/           encje, bez zależności zewnętrznych
    Evacuation.Application/      pipeline obrazu, budowa grafu, routing
    Evacuation.Infrastructure/   EF Core, DbContext, migracje
    Evacuation.Api/              kontrolery, DTO
  tests/
    Evacuation.Application.Tests/
MobileApp/
  Evacuation.Mobile.sln
  Evacuation.Mobile/             aplikacja MAUI
Makieta/                         makiety UI mobilki (HTML)
```

Backend i mobilka mają osobne solucje celowo: `Evacuation.Mobile` wymaga workloadów MAUI,
a backend buduje się na czystym SDK. Dzięki rozdzieleniu `dotnet build Backend/Evacuation.sln`
i CI dla API nie potrzebują workloadów Androida ani iOS.

Warstwa `Application` nie zna EF Core ani HTTP — algorytmy są testowalne jednostkowo na syntetycznych bitmapach generowanych w kodzie.

## Model danych

| Encja | Opis |
|---|---|
| `Building` | nazwa, adres, współrzędne geograficzne |
| `FloorPlan` | numer piętra, obraz, wymiary |
| `Room` | etykieta, centroid, flagi wyjścia i klatki schodowej |
| `RoomEdge` | para pomieszczeń, waga krawędzi |
| `DangerZone` | pomieszczenie, czas zgłoszenia, status |

## Uruchomienie

Wymagania: **.NET 10 SDK**, Docker (lub Podman) dla kontenera bazy. Wersja SDK jest przypięta
w `global.json` (`10.0.300`, `rollForward: latestFeature`).

Aspire **nie jest workloadem** — od .NET 9 dostarczany jest jako pakiet NuGet z szablonami
plus opcjonalne CLI. Instalacja narzędzi (jednorazowo):

```bash
dotnet new install Aspire.ProjectTemplates   # szablony aspire-apphost, aspire-servicedefaults, ...
winget install Microsoft.Aspire              # opcjonalnie: polecenie `aspire`
```

Backend — wszystkie polecenia z katalogu głównego repozytorium:

```bash
git clone <repo>
cd PracaInzynierska
dotnet run --project Backend/src/Evacuation.AppHost
```

Alternatywnie, z zainstalowanym CLI: `cd Backend && aspire run`.

AppHost wystartuje PostgreSQL, zastosuje migracje i uruchomi API. Adres dashboardu pojawi się w konsoli.

Testy jednostkowe:

```bash
dotnet test Backend/Evacuation.sln
```

Migracje EF Core wymagają narzędzia `dotnet-ef` (`dotnet tool install -g dotnet-ef`):

```bash
dotnet ef migrations add <nazwa> \
  --project Backend/src/Evacuation.Infrastructure \
  --startup-project Backend/src/Evacuation.Api
```

Aplikacja mobilna uruchamiana jest osobno — Aspire nie hostuje klientów MAUI. Wymaga workloadów
MAUI (`dotnet workload install maui-android`):

```bash
dotnet build MobileApp/Evacuation.Mobile -t:Run -f net10.0-android
```

Adres API ustawiany jest w konfiguracji projektu mobilnego. Przy emulatorze Androida host maszyny widoczny jest pod `10.0.2.2`.

## Roadmapa

- [ ] Szkielet rozwiązania i pipeline przetwarzania obrazu
- [ ] Persystencja grafu w PostgreSQL
- [ ] Routing z omijaniem stref niebezpiecznych
- [ ] REST API i upload planów
- [ ] Wyszukiwanie budynków po GPS
- [ ] Aplikacja mobilna i wizualizacja trasy na planie
- [ ] Testy wydajnościowe na dużych planach

## Opiekun pracy

dr hab. Rafał Adamczak
