# Weather Forecast Model Comparison
## Aplikacja do porównywania prognoz pogodowych z różnych modeli meteorologicznych
Aplikacja umożliwia pobieranie, przechowywanie i porównywanie prognoz pogodowych 
dla dowolnej lokalizacji na świecie.  Dane są cache'owane w bazie danych (6h), 
co minimalizuje liczbę zapytań do zewnętrznego API. 

### 1.2 Technologie
- **Backend**: ASP.NET Core 9 Web API
- **Baza danych**: SQLite + Entity Framework Core 9
- **External API**: Open-Meteo (geocoding + forecast)
- **Dokumentacja**: Swagger/OpenAPI
- **Frontend**: React 18 + Vite + Chart.js
- 
### 1.3 Funkcjonalności
1. **Geokodowanie** - konwersja nazwy miasta na współrzędne GPS (z cache)
2. **Pobieranie prognoz** - dla 5 modeli pogodowych równolegle
3. **Caching** - 6-godzinny cache w bazie danych
4. **Walidacja** - sprawdzanie poprawności danych wejściowych
5. **Rate limiting** - ochrona przed nadmiernym użyciem (10 req/min)
6. **Obsługa błędów** - Result Pattern zamiast exceptions
7. **Normalizacja** - wsparcie polskich znaków (Kraków → Krakow)
8. 
### 1.4 Modele pogodowe
| Model | Rozdzielczość | Zakres | Instytucja |
|-------|---------------|--------|------------|
| ECMWF IFS | 0.25° (~25km) | Globalny | European Centre for Medium-Range Weather Forecasts |
| ICON Global | 0.25° | Globalny | DWD (Niemcy) |
| ICON EU | 0.0625° (~7km) | Europa | DWD |
| GFS | 0.25° | Globalny | NOAA (USA) |
| ARPEGE | 0.25° | Europa | Météo-France 


### 1.5 Architektura
Projekt implementuje **Clean Architecture** z podziałem na 4 warstwy:
- **Domain** - encje biznesowe (Forecast, Location, WeatherModel)
- **Application** - logika biznesowa (Facade, Chain of Responsibility)
- **Infrastructure** - dostęp do danych i zewnętrznych API
- **Web** - warstwa prezentacji (REST API)

Zależności płyną jednokierunkowo:  Web → Application → Infrastructure → Domain


## ARCHITEKTURA SYSTEMU

<img width="4000" height="3500" alt="ArchitectureDiagram" src="https://github.com/user-attachments/assets/484037ac-874e-430e-9384-fa1836913181" />

## SOLID
### 3.1 Single Responsibility Principle (SRP)
**Każda klasa ma jedną odpowiedzialność:**

- **ValidationHandler** - tylko walidacja miasta
- **GeocodingHandler** - tylko konwersja miasto → GPS
- **CacheCheckHandler** - tylko sprawdzanie cache
- **ApiFetchHandler** - tylko pobieranie z API
- **CacheSaveHandler** - tylko zapis do DB


### 3.2 Open/Closed Principle (OCP)

**BaseForecastHandler** - bazowa klasa abstrakcyjna
Możemy dodać nowy handler (np. LoggingHandler) bez zmiany istniejących

### 3.3 Liskov Substitution Principle (LSP)

Podklasy mogą zastąpić klasy bazowe bez zmiany zachowania:
- **IForecastHandler** - wszystkie implementacje działają identycznie
- ValidationHandler, GeocodingHandler, etc. można zamiennie użyć jako IForecastHandler

### 3.4 Interface Segregation Principle (ISP)

Klienci nie powinni zależeć od interfejsów, których nie używają:
Małe, wyspecjalizowane interfejsy:
- IForecastRepository - tylko operacje na prognozach
- ILocationRepository - tylko operacje na lokacjach
- IGeocodingService - tylko geocoding
- IWeatherApiClient - tylko API calls
### 3.5 Dependency Inversion Principle (DIP)
Zależności od abstrakcji, nie od konkretnych implementacji:
Wszystkie klasy zależą od interfejsów:
- ForecastFacade zależy od IForecastHandler 
- GeocodingHandler zależy od IGeocodingService 
## 4. WZORCE PROJEKTOWE
### 4.1 Facade Pattern
**Problem**:  Kontroler nie powinien wiedzieć o złożoności systemu handlerów. 
**Rozwiązanie**: `ForecastFacade` ukrywa łańcuch handlerów za prostym interfejsem.

<img width="300" height="900" alt="Facade" src="https://github.com/user-attachments/assets/504dc89e-116f-47b6-bcc2-2e4d0d9b98aa" />

### 4.2 Chain of Responsibility Pattern

**Problem**: Przetwarzanie żądania wymaga wielu kroków, które mogą się zmieniać. Rozwiązanie: Każdy krok to osobny handler, łatwo dodać/usunąć/zmienić kolejność.

<img width="4000" height="1000" alt="ChainOfResponsibility" src="https://github.com/user-attachments/assets/e1153889-96f0-462b-9086-d06f65b1f0d6" />

## Domain Diagram:

<img width="300" height="900" alt="EntityDiagram" src="https://github.com/user-attachments/assets/3545fbf5-2cb8-4532-9b7a-edfc5fc42d39" />

## Działanie:

<img width="1773" height="634" alt="obraz" src="https://github.com/user-attachments/assets/879d1312-4ef8-4558-8ee2-e255206bf5e5" />



