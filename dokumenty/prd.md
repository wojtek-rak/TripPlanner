# Dokument wymagañ produktu (PRD) - Trip Planner

## 1. Przegl¹d produktu
Trip Planner to webowa aplikacja Blazor (.NET 9) do szybkiego tworzenia i edycji planów podró¿y. £¹czy generowanie szkicu planu przez AI z prost¹, rêczn¹ edycj¹ w jednym miejscu. Dane s¹ prywatne i powi¹zane z kontem u¿ytkownika (ASP.NET Core Identity). Trwa³oœæ zapewnia EF Core z repozytorium `TripRepository`. Generowanie planu wykorzystuje istniej¹c¹ us³ugê `TripPlanGeneratorService` z konfigurowalnym kluczem i endpointem.

Cele produktu:
- Skróciæ czas przygotowania sensownego szkicu planu do minut.
- Zapewniæ proste narzêdzia do rêcznego dopracowania planu w widokach kart, podsumowania i szczegó³ów (`TripCard`, `TripSummary`, `TripDetail`).
- Utrzymaæ minimalny, stabilny zestaw funkcji w MVP z prywatnoœci¹ danych u¿ytkownika.

G³ówne komponenty techniczne:
- Interfejs: Blazor (.NET 9) komponenty stron: lista podró¿y, szczegó³y podró¿y, edycja dni planu.
- Logika: `TripPlanGeneratorService` (AI), prosty algorytm harmonogramu dnia (poranek/popo³udnie/wieczór).
- Dane: EF Core, encje `Trip`, `TripPlanDay`, repozytorium `TripRepository`, migracje.
- Uwierzytelnianie: ASP.NET Core Identity z `ApplicationUser`.
- Konfiguracja AI: zapisywane ustawienia klucza/endpointu dla generatora (mig. addApiKeyAndEndpoint).

## 2. Problem u¿ytkownika
Planowanie podró¿y jest rozproszone i czasoch³onne. U¿ytkownicy musz¹ rêcznie ³¹czyæ atrakcje, noclegi, transport i bud¿et, aby z³o¿yæ dzienny harmonogram. Brakuje narzêdzia, które:
- Na podstawie celu, dat, preferencji i bud¿etu wygeneruje sensowny szkic planu.
- Pozwoli szybko i intuicyjnie dopracowaæ plan w jednym miejscu.
- Zapewni prywatnoœæ i prosty dostêp do w³asnych planów.

Konsekwencje dziœ:
- Du¿a iloœæ czasu na integracjê danych z wielu Ÿróde³.
- Chaotyczne pliki/arkusze z planami.
- Niska satysfakcja z pierwszej wersji planu, koniecznoœæ wielokrotnych poprawek.

## 3. Wymagania funkcjonalne
Identyfikatory FR s³u¿¹ traceability z historyjkami.

- FR-001 Uwierzytelnianie i konta: rejestracja, logowanie, wylogowanie, reset has³a z ASP.NET Core Identity (`ApplicationUser`).
- FR-002 Prywatnoœæ: u¿ytkownik widzi i edytuje wy³¹cznie w³asne podró¿e i plany.
- FR-003 Konfiguracja AI: u¿ytkownik mo¿e zapisaæ klucz i endpoint dla `TripPlanGeneratorService` w ustawieniach konta.
- FR-004 Lista podró¿y: widok listy z kartami (`TripCard`) z podstawowym podsumowaniem.
- FR-005 Tworzenie podró¿y: formularz z walidacj¹ dla celu/miejsca, zakresu dat, bud¿etu, preferencji.
- FR-006 Szczegó³y podró¿y: widok `TripDetail` z `TripSummary` oraz dziennym planem.
- FR-007 Generowanie planu AI: na podstawie danych wejœciowych (cel, daty, preferencje, bud¿et) tworzy szkic planu w encjach `Trip` i `TripPlanDay`.
- FR-008 Manualna edycja planu: dodawanie, edycja i usuwanie dni (`TripPlanDay`) oraz aktywnoœci w ramach dnia.
- FR-009 Harmonogram dnia: struktura poranek, popo³udnie, wieczór bez optymalizacji tras; mo¿liwoœæ rêcznych poprawek.
- FR-010 Operacje na dniach: dodawanie dnia na koniec/miêdzy datami, usuwanie dnia, zmiana kolejnoœci dni.
- FR-011 Operacje na aktywnoœciach: dodaj/edytuj/usuñ aktywnoœæ w segmencie (tytu³, opis, opcjonalny koszt/czas).
- FR-012 Bud¿et: pole bud¿etu ca³kowitego w podró¿y oraz sumowanie szacunków z aktywnoœci na potrzeby podsumowania.
- FR-013 Walidacje: spójnoœæ dat (start <= end), niepuste pole celu, bud¿et nieujemny, klucz/endpoint AI wymagane do generowania.
- FR-014 Trwa³oœæ: zapis/odczyt przez EF Core z migrowalnym schematem, repository `TripRepository`.
- FR-015 Wydajnoœæ UI: sensowne stany ³adowania, brak blokowania UI podczas generowania, mo¿liwoœæ anulowania.
- FR-016 Odpornoœæ: ³agodne b³êdy dla niedostêpnoœci AI lub nieprawid³owej konfiguracji; nie utraciæ wprowadzonych danych formularza.
- FR-017 Dostêpnoœæ i u¿ytecznoœæ: responsywny uk³ad, klawiaturowa nawigacja, teksty statusu b³êdów.
- FR-018 Rewizje podstawowe: znacznik czasu utworzenia/aktualizacji dla `Trip` i `TripPlanDay`.
- FR-019 Logowanie zdarzeñ: minimalne logi dla operacji generowania i b³êdów integracji AI (bez wra¿liwych danych).
- FR-020 Internacjonalizacja podstawowa: format dat i liczb zgodny z kultur¹ przegl¹darki; treœæ w jêzyku interfejsu.

Minimalny model danych:
- Trip: Id, OwnerUserId, Destination, StartDate, EndDate, Budget, Preferences (tekst), Notes, CreatedAt, UpdatedAt.
- TripPlanDay: Id, TripId, Date, DayIndex, MorningActivities[], AfternoonActivities[], EveningActivities[] (lista prostych obiektów: Title, Description?, EstimatedCost?, EstimatedTime?).

## 4. Granice produktu
W zakresie MVP:
- Generowanie wstêpnego planu podró¿y przez AI na podstawie celu, dat, preferencji i bud¿etu z u¿yciem `TripPlanGeneratorService`.
- Manualne tworzenie i edycja planu: CRUD dla `Trip`, `TripPlanDay` oraz aktywnoœci dziennych.
- Przegl¹danie listy podró¿y oraz szczegó³ów planu w widokach `TripCard`, `TripSummary`, `TripDetail`.
- System kont u¿ytkowników (Identity) z prywatnymi planami i konfiguracj¹ klucza/endpointu AI.
- Prosty algorytm harmonogramu dnia (poranek/popó³udnie/wieczór), rêczne poprawki.
- Trwa³oœæ danych w bazie (EF Core, `TripRepository`, migracje). Interfejs web Blazor (.NET 9).

Poza zakresem MVP:
- Zaawansowana optymalizacja tras lub czasy przejazdów, integracje z mapami w czasie rzeczywistym.
- Rezerwacje i integracje z zewnêtrznymi dostawcami (linie lotnicze, hotele, OTA).
- Wspó³dzielenie planów i wspó³praca w czasie rzeczywistym.
- Import/eksport do formatów PDF, DOCX, ICS i innych.
- Aplikacje mobilne natywne, tryb offline, wielowalutowoœæ z dynamicznymi kursami.

Za³o¿enia:
- Jedno Ÿród³o prawdy w bazie; brak zewnêtrznego zapisu planu poza DB.
- Dostêp do AI wymaga poprawnie ustawionego klucza i endpointu przez u¿ytkownika.
- Brak zaawansowanej kontroli wersji planu w MVP.

## 5. Historyjki u¿ytkowników
US-001  
Tytu³: Rejestracja konta  
Opis: Jako nowy u¿ytkownik chcê utworzyæ konto, aby prywatnie zapisywaæ swoje plany.  
Kryteria akceptacji:
- Po podaniu poprawnego e-maila i has³a konto zostaje utworzone, a u¿ytkownik jest zalogowany.
- B³êdne dane wyœwietlaj¹ komunikaty walidacji bez utraty wpisów.
- Has³o musi spe³niaæ minimalne wymagania zdefiniowane w systemie.

US-002  
Tytu³: Logowanie i wylogowanie  
Opis: Jako u¿ytkownik chcê zalogowaæ siê i wylogowaæ, aby bezpiecznie zarz¹dzaæ planami.  
Kryteria akceptacji:
- Poprawne dane logowania skutkuj¹ dostêpem do listy podró¿y.
- Niepoprawne dane wyœwietlaj¹ jednoznaczny komunikat bez ujawniania, co by³o b³êdne.
- Wylogowanie koñczy sesjê i ukrywa prywatne dane.

US-003  
Tytu³: Konfiguracja klucza i endpointu AI  
Opis: Jako u¿ytkownik chcê zapisaæ klucz i endpoint dla generatora, aby móc generowaæ plany AI.  
Kryteria akceptacji:
- Formularz ustawieñ konta pozwala wprowadziæ/aktualizowaæ klucz i endpoint.
- Klucz jest przechowywany bezpiecznie i nie jest wyœwietlany w ca³oœci po zapisaniu.
- Próba generowania bez konfiguracji pokazuje proœbê o uzupe³nienie ustawieñ.

US-004  
Tytu³: Lista moich podró¿y  
Opis: Jako u¿ytkownik chcê widzieæ listê swoich podró¿y w formie kart z podsumowaniem.  
Kryteria akceptacji:
- Ka¿da karta pokazuje nazwê/cel, daty, liczbê dni i skrót bud¿etu.
- Lista jest posortowana po dacie utworzenia lub rozpoczêcia.
- Brak podró¿y wyœwietla przyjazny stan pusty z akcj¹ dodania.

US-005  
Tytu³: Utworzenie podró¿y manualnie  
Opis: Jako u¿ytkownik chcê utworzyæ now¹ podró¿, podaj¹c cel, daty, bud¿et i preferencje.  
Kryteria akceptacji:
- Walidacja: cel wymagany; start <= end; bud¿et >= 0.
- Po zapisaniu podró¿ pojawia siê na liœcie i mo¿na przejœæ do szczegó³ów.
- Niepowodzenie zapisu pokazuje komunikat i nie dubluje wpisów.

US-006  
Tytu³: Podgl¹d szczegó³ów podró¿y  
Opis: Jako u¿ytkownik chcê zobaczyæ podsumowanie i plan dzienny podró¿y.  
Kryteria akceptacji:
- Widok wyœwietla dane ogólne oraz listê dni z segmentami poranek/popo³udnie/wieczór.
- Brak dni pokazuje stan pusty i opcje dodania dnia lub wygenerowania planu.

US-007  
Tytu³: Generowanie planu przez AI  
Opis: Jako u¿ytkownik chcê wygenerowaæ szkic planu z danych wejœciowych.  
Kryteria akceptacji:
- Po uruchomieniu wyœwietla siê stan ³adowania; UI nie jest zablokowane.
- Po sukcesie dni i aktywnoœci s¹ utworzone zgodnie z d³ugoœci¹ podró¿y.
- W przypadku b³êdu (brak konfiguracji, b³¹d sieci, limit) pojawia siê komunikat i plan nie jest czêœciowo zapisywany, o ile u¿ytkownik nie potwierdzi.

US-008  
Tytu³: Dodanie dnia do planu  
Opis: Jako u¿ytkownik chcê dodaæ nowy dzieñ do istniej¹cego planu.  
Kryteria akceptacji:
- Mogê dodaæ dzieñ na koniec lub wstawiæ miêdzy istniej¹ce dni.
- Nowy dzieñ ma poprawny indeks i datê w zakresie podró¿y.
- Zmiany s¹ zapisywane i widoczne w szczegó³ach.

US-009  
Tytu³: Usuniêcie dnia z planu  
Opis: Jako u¿ytkownik chcê usun¹æ wybrany dzieñ z planu.  
Kryteria akceptacji:
- Usuniêcie wymaga potwierdzenia.
- Indeksy pozosta³ych dni aktualizuj¹ siê poprawnie.
- Operacja nie narusza spójnoœci zakresu dat podró¿y.

US-010  
Tytu³: Edycja informacji o podró¿y  
Opis: Jako u¿ytkownik chcê edytowaæ cel, daty, bud¿et i preferencje.  
Kryteria akceptacji:
- Walidacje identyczne jak przy tworzeniu.
- Zmiana zakresu dat aktualizuje zgodnoœæ dni; u¿ytkownik jest proszony o potwierdzenie przy skracaniu.
- Zapis zachowuje ci¹g³oœæ istniej¹cych aktywnoœci w mieszcz¹cych siê dniach.

US-011  
Tytu³: Dodanie aktywnoœci do segmentu dnia  
Opis: Jako u¿ytkownik chcê dodaæ aktywnoœæ do poranka/popo³udnia/wieczoru.  
Kryteria akceptacji:
- Aktywnoœæ ma co najmniej tytu³; opis i szacunki s¹ opcjonalne.
- Po zapisie aktywnoœæ pojawia siê w odpowiednim segmencie.
- Koszt i czas sumuj¹ siê w podsumowaniu dnia/podró¿y.

US-012  
Tytu³: Edycja aktywnoœci  
Opis: Jako u¿ytkownik chcê edytowaæ szczegó³y istniej¹cej aktywnoœci.  
Kryteria akceptacji:
- Zmiany s¹ walidowane i zapisywane bez dublowania.
- Zmiana segmentu przenosi aktywnoœæ do nowego segmentu.

US-013  
Tytu³: Usuniêcie aktywnoœci  
Opis: Jako u¿ytkownik chcê usun¹æ aktywnoœæ z dnia.  
Kryteria akceptacji:
- Usuniêcie wymaga potwierdzenia.
- Po usuniêciu sumy bud¿etu aktualizuj¹ siê poprawnie.

US-014  
Tytu³: Zmiana kolejnoœci dni  
Opis: Jako u¿ytkownik chcê zmieniæ kolejnoœæ dni w planie.  
Kryteria akceptacji:
- Przeci¹gnij-upuœæ lub akcje przenieœ w górê/w dó³ aktualizuj¹ indeksy.
- Kolejnoœæ odzwierciedla siê natychmiast w widoku i zapisie.

US-015  
Tytu³: Zmiana kolejnoœci aktywnoœci w segmencie  
Opis: Jako u¿ytkownik chcê u³o¿yæ aktywnoœci w ramach segmentu.  
Kryteria akceptacji:
- Kolejnoœæ mo¿na zmieniæ i zostaje zapisana.
- Podsumowania pozostaj¹ spójne.

US-016  
Tytu³: Podsumowanie bud¿etu  
Opis: Jako u¿ytkownik chcê widzieæ sumy kosztów na dzieñ i ca³¹ podró¿.  
Kryteria akceptacji:
- Wyœwietlane s¹ sumy z aktywnoœci; brak wartoœci nie zaburza obliczeñ.
- Przekroczenie bud¿etu oznaczane jest wizualnie.

US-017  
Tytu³: Usuniêcie podró¿y  
Opis: Jako u¿ytkownik chcê trwale usun¹æ ca³¹ podró¿.  
Kryteria akceptacji:
- Operacja wymaga potwierdzenia z informacj¹ o nieodwracalnoœci.
- Wszystkie dni i aktywnoœci powi¹zane s¹ usuwane.

US-018  
Tytu³: Ochrona dostêpu do cudzych danych  
Opis: Jako u¿ytkownik chcê mieæ pewnoœæ, ¿e inni nie maj¹ dostêpu do moich planów.  
Kryteria akceptacji:
- Próba dostêpu do zasobów innego u¿ytkownika zwraca b³¹d 403/404 i nic nie ujawnia.
- Lista podró¿y zawiera wy³¹cznie zasoby w³aœciciela.

US-019  
Tytu³: Walidacje formularzy  
Opis: Jako u¿ytkownik chcê otrzymywaæ jasne komunikaty o b³êdach przy niepoprawnych danych.  
Kryteria akceptacji:
- B³êdy wskazuj¹ konkretne pola i powód.
- Zawartoœæ formularza pozostaje zachowana po b³êdzie.

US-020  
Tytu³: Odpornoœæ na b³êdy AI  
Opis: Jako u¿ytkownik chcê zrozumia³y komunikat, gdy generator nie dzia³a.  
Kryteria akceptacji:
- W przypadku timeoutu/HTTP 4xx/5xx wyœwietlany jest opisowy komunikat.
- Wprowadzonych danych nie tracê; mogê spróbowaæ ponownie.

US-021  
Tytu³: Anulowanie generowania  
Opis: Jako u¿ytkownik chcê móc anulowaæ d³ugie generowanie planu.  
Kryteria akceptacji:
- Dostêpny jest przycisk anuluj podczas generowania.
- Anulowanie koñczy ¿¹danie do us³ugi i przywraca UI do stanu sprzed akcji.

US-022  
Tytu³: Wylogowanie nieaktywnej sesji  
Opis: Jako u¿ytkownik chcê, aby moja sesja wygasa³a po d³u¿szej nieaktywnoœci.  
Kryteria akceptacji:
- Po wygaœniêciu sesji pierwsza próba modyfikacji przekierowuje do logowania.
- Po zalogowaniu powracam do poprzedniego kontekstu.

US-023  
Tytu³: Kopiowanie podró¿y jako szablonu  
Opis: Jako u¿ytkownik chcê skopiowaæ istniej¹c¹ podró¿, aby szybciej tworzyæ podobne plany.  
Kryteria akceptacji:
- Kopia zawiera dni i aktywnoœci, ale ma nowy identyfikator.
- U¿ytkownik mo¿e zmieniæ daty i cel przed zapisem.

US-024  
Tytu³: Edycja preferencji  
Opis: Jako u¿ytkownik chcê edytowaæ preferencje (np. tempo, typ atrakcji), aby dopasowaæ plan.  
Kryteria akceptacji:
- Preferencje s¹ zapisywane w podró¿y i wykorzystywane przez generator przy kolejnym wywo³aniu.
- Zmiana preferencji nie kasuje istniej¹cych aktywnoœci bez potwierdzenia.

US-025  
Tytu³: Przegl¹d w wersji mobilnej przegl¹darki  
Opis: Jako u¿ytkownik chcê wygodnie przegl¹daæ plany na telefonie.  
Kryteria akceptacji:
- Widoki listy i szczegó³ów s¹ responsywne.
- Tabele/segmenty przewijaj¹ siê poziomo lub sk³adaj¹ do list.

US-026  
Tytu³: Widok stanu pustego i pierwsze kroki  
Opis: Jako u¿ytkownik chcê wskazówek, gdy nie mam jeszcze ¿adnych podró¿y.  
Kryteria akceptacji:
- Strona wyœwietla akcje: utwórz rêcznie, skonfiguruj AI, wygeneruj plan.
- Link prowadzi do ustawieñ klucza/endpointu AI.

US-027  
Tytu³: Filtrowanie i wyszukiwanie podró¿y  
Opis: Jako u¿ytkownik chcê szybko znaleŸæ podró¿ po nazwie lub dacie.  
Kryteria akceptacji:
- Proste wyszukiwanie po frazie filtruje listê po Destination.
- Filtry po zakresie dat zawê¿aj¹ wyniki.

US-028  
Tytu³: Bezpieczne przechowywanie klucza AI  
Opis: Jako w³aœciciel danych chcê, aby mój klucz AI nie wycieka³.  
Kryteria akceptacji:
- Klucz nie jest logowany ani wysy³any do klienta w jawnej postaci po zapisaniu.
- Uprawnienia dostêpu do pola s¹ ograniczone do w³aœciciela.

US-029  
Tytu³: Eksport podstawowych danych do schowka  
Opis: Jako u¿ytkownik chcê skopiowaæ podsumowanie planu do schowka.  
Kryteria akceptacji:
- Dostêpna akcja kopiuje zwiêz³y tekst z list¹ dni i aktywnoœci.
- Operacja dzia³a bez po³¹czeñ zewnêtrznych.

US-030  
Tytu³: Podgl¹d zmian przed zapisem  
Opis: Jako u¿ytkownik chcê widzieæ, co ulegnie zmianie przed akceptacj¹ generowania nadpisuj¹cego plan.  
Kryteria akceptacji:
- Prezentowana jest ró¿nica: nowe/zmienione/usuniête dni/aktywnoœci.
- U¿ytkownik mo¿e zaakceptowaæ lub anulowaæ.

Bezpieczeñstwo i uwierzytelnianie s¹ objête przez US-001, US-002, US-018, US-022, US-028.

## 6. Metryki sukcesu
Metryki g³ówne:
- 75% planów wygenerowanych przez AI jest akceptowanych przez u¿ytkownika po jednorazowej, drobnej edycji.
- 75% nowych podró¿y jest tworzonych z wykorzystaniem generatora AI.

Metryki wspieraj¹ce:
- Œredni czas do pierwszego szkicu planu poni¿ej 60 sekund od wejœcia na widok tworzenia.
- Odsetek niepowodzeñ generowania AI poni¿ej 5% (z wy³¹czeniem b³êdnych konfiguracji u¿ytkownika).
- Œrednia liczba interakcji edycyjnych po generowaniu do akceptacji planu poni¿ej 5.
- Czas wczytania listy podró¿y poni¿ej 1,5 s dla 95 percentyla.
- Zero incydentów ujawnienia kluczy/planów w logach produkcyjnych.

Checklist koñcowy:
- Ka¿da historyjka ma jasne, testowalne kryteria akceptacji.
- Kryteria s¹ konkretne i sprawdzalne w testach UI/API.
- Zestaw historyjek pokrywa pe³en przep³yw: konfiguracja AI, tworzenie, generowanie, edycja, podgl¹d, usuwanie, bezpieczeñstwo.
- Wymagania uwierzytelniania i autoryzacji s¹ ujête w US-001/002/018/022/028 oraz FR-001/002.