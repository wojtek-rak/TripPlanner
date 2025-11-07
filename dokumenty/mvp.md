
### Główny problem
Planowanie podróży jest rozproszone i czasochłonne: trzeba samodzielnie łączyć atrakcje, noclegi, transport i budżet w spójny plan dniowy. Brakuje prostego narzędzia, które szybko wygeneruje sensowny szkic planu i pozwoli go łatwo dopracować w jednym miejscu.

### Najmniejszy zestaw funkcjonalności
- Generowanie wstępnego planu podróży przez AI na podstawie celu, dat, preferencji i budżetu (wykorzystanie istniejącej usługi `TripPlanGeneratorService` oraz konfiguracji klucza/endpointu).
- Manualne tworzenie i edycja planu: dodawanie/edycja/usuwanie podróży (`Trip`), dni (`TripPlanDay`) i aktywności w dniach.
- Przeglądanie listy podróży oraz szczegółów planu (widoki kart i podsumowania: `TripCard`, `TripSummary`, `TripDetail`).
- Prosty system kont użytkowników do przechowywania prywatnych planów (Identity z `ApplicationUser`).
- Gotowy, prosty algorytm harmonogramu dnia (poranek/popłudnie/wieczór, bez optymalizacji tras) z możliwością ręcznych poprawek.
- Trwałość danych w bazie (EF Core, repozytorium `TripRepository`, migracje), Blazor (.NET 9) jako interfejs web.

### Co NIE wchodzi w zakres MVP
- Zaawansowana optymalizacja tras/czasów przejazdów, integracje z mapami w czasie rzeczywistym.
- Rezerwacje i integracje z zewnętrznymi dostawcami (linie lotnicze, hotele, OTA).
- Współdzielenie planów między użytkownikami, współpraca w czasie rzeczywistym.
- Import/eksport zaawansowanych formatów (PDF, DOCX, ICS, itp.).
- Aplikacje mobilne (na start wyłącznie web), tryb offline, wielowalutowość z dynamicznymi kursami.

### Kryteria sukcesu
- 75% planów wygenerowanych przez AI jest akceptowanych przez użytkownika po jednorazowej, drobnej edycji.
- Użytkownicy tworzą 75% nowych podróży z wykorzystaniem generatora AI.