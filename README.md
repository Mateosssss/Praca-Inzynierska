# Evacuation

System wskazywania drogi ewakuacji z budynku. Aplikacja mobilna wyznacza optymalną trasę do najbliższego wyjścia ewakuacyjnego na podstawie rzutu piętra, omijając strefy oznaczone jako niebezpieczne.

Projekt realizowany jako praca inżynierska — informatyka, Wydział Fizyki, Astronomii i Informatyki Stosowanej UMK w Toruniu.

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
src/
  Evacuation.AppHost/          orkiestracja Aspire
  Evacuation.ServiceDefaults/  telemetria, health checks, discovery
  Evacuation.Domain/           encje, bez zależności zewnętrznych
  Evacuation.Application/      pipeline obrazu, budowa grafu, routing
  Evacuation.Infrastructure/   EF Core, DbContext, migracje
  Evacuation.Api/              kontrolery, DTO
  Evacuation.Mobile/           aplikacja MAUI
tests/
  Evacuation.Application.Tests/
```

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

Wymagania: .NET 9 SDK, Docker (lub Podman) dla kontenera bazy, workload Aspire.

```bash
dotnet workload install aspire
git clone <repo>
cd evacuation
dotnet run --project src/Evacuation.AppHost
```

AppHost wystartuje PostgreSQL, zastosuje migracje i uruchomi API. Adres dashboardu pojawi się w konsoli.

Aplikacja mobilna uruchamiana jest osobno — Aspire nie hostuje klientów MAUI:

```bash
dotnet build src/Evacuation.Mobile -t:Run -f net9.0-android
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

## Status

Projekt w fazie implementacji. Termin złożenia pracy: luty 2027.****
