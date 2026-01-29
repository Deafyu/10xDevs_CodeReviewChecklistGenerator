Dokument wymagań produktu (PRD) - Code Review Checklist Generator (MVP)
1. Przegląd produktu
Code Review Checklist Generator to aplikacja webowa w wersji MVP, która pomaga uporządkować i ustandaryzować code review. Użytkownik wkleja diff, fragment kodu lub opisuje kontekst zmiany, a AI generuje spersonalizowaną checklistę do review. Aplikacja pozwala zapisywać szablony checklist dla typowych typów zmian, odhaczanie punktów podczas przeglądu oraz eksport wyniku.

MVP ma dostarczyć proste, szybkie narzędzie do generowania list kontrolnych opartych o kontekst zmiany, tak aby review było bardziej spójne i kompletne.

2. Problem użytkownika
Code review bez struktury jest powierzchowne lub niespójne, a reviewerzy pomijają ważne aspekty (np. bezpieczeństwo, wydajność, zgodność z architekturą). Brakuje szybkiego sposobu na wygenerowanie listy kontrolnej dopasowanej do konkretnej zmiany, bez ręcznego przygotowania i utrzymywania szablonów.

3. Wymagania funkcjonalne
3.1. Uwierzytelnianie i konta użytkowników
Użytkownicy mogą rejestrować się i logować wyłącznie za pomocą e-maila i hasła.
Wymagana jest polityka silnych haseł (szczegóły do decyzji na etapie implementacji).
Użytkownik może trwale usunąć konto wraz z danymi.

3.2. Generowanie checklisty (AI)
Użytkownik może wkleić diff, fragment kodu lub opis kontekstu zmiany.
System generuje checklistę punktów do review, dopasowaną do dostarczonego kontekstu.
Checklista jest edytowalna przed zapisem.
Użytkownik może odznaczyć wybrane punkty jako nieistotne i zapisać finalną wersję.

3.3. Szablony checklist
Użytkownik może zapisywać szablony checklist dla typowych zmian.
Szablony można przeglądać, edytować, duplikować i usuwać.
Przy generowaniu checklisty użytkownik może wybrać szablon bazowy, który zostanie uzupełniony przez AI.

3.4. Odhaczanie i eksport
Użytkownik może odhaczac punkty checklisty w trakcie review.
Możliwy jest eksport checklisty (np. jako plik tekstowy lub PDF).

3.5. Integracja z OpenRouter AI
AI korzysta z OpenRouter do generowania checklisty.
System powinien obsługiwać timeouty i błędy odpowiedzi modelu.
W MVP nie przewiduje się zarządzania kosztami per użytkownik poza prostym limitem zapytań.

4. Granice produktu
4.1. Co wchodzi w zakres MVP
Generowanie checklisty z diffu/kodu/opisu.
Edycja, odhaczanie i eksport checklisty.
Zapisywanie i zarządzanie szablonami.
Uwierzytelnianie oparte o e-mail i hasło.
Integracja z OpenRouter AI.

4.2. Co NIE wchodzi w zakres MVP
Integracje z systemami VCS (np. GitHub PRs) i automatyczne pobieranie diffów.
Współdzielenie checklisty w czasie rzeczywistym.
Zaawansowana analityka użycia (np. scoring review).
Integracje z komunikatorami (Slack, Teams).

4.3. Nierozwiązane kwestie
Wybór modelu AI w OpenRouter i polityka fallback.
Docelowy format eksportu (TXT vs PDF) i zakres metadanych.
Dokładne limity zapytań AI na użytkownika/projekt.

5. Historyjki użytkowników
5.1. Uwierzytelnianie
ID: US-001
Tytuł: Rejestracja użytkownika
Opis: Jako nowy użytkownik, chcę móc założyć konto, abym mógł tworzyć checklisty.
Kryteria akceptacji:
Formularz zawiera pola: e-mail, hasło, potwierdzenie hasła.
Walidacja sprawdza format e-maila i siłę hasła.
Po rejestracji jestem automatycznie zalogowany.

ID: US-002
Tytuł: Logowanie
Opis: Jako zarejestrowany użytkownik, chcę się zalogować, aby korzystać z aplikacji.
Kryteria akceptacji:
Logowanie wymaga poprawnego e-maila i hasła.
Po poprawnym logowaniu trafiam na ekran generowania checklisty.

5.2. Generowanie checklisty
ID: US-003
Tytuł: Generowanie checklisty z diffu/kodu/opisu
Opis: Jako reviewer, chcę wkleić diff lub opis zmiany i otrzymać checklistę do review.
Kryteria akceptacji:
Mam pole tekstowe na diff/kod/opis.
Po kliknięciu "Generuj", widzę checklistę.
Mogę edytować i usuwać punkty przed zapisem.

ID: US-004
Tytuł: Odhaczanie punktów
Opis: Jako reviewer, chcę odhaczac punkty checklisty w trakcie review.
Kryteria akceptacji:
Każdy punkt ma checkbox.
Stan odhaczenia jest zapisywany.

5.3. Szablony
ID: US-005
Tytuł: Zapisywanie szablonu
Opis: Jako użytkownik, chcę zapisać checklistę jako szablon dla podobnych zmian.
Kryteria akceptacji:
Mogę zapisać checklistę jako nowy szablon.
Mogę edytować nazwę i punkty szablonu.

ID: US-006
Tytuł: Generowanie checklisty na bazie szablonu
Opis: Jako użytkownik, chcę wybrać szablon i użyć go jako bazy dla AI.
Kryteria akceptacji:
Przed generowaniem mogę wybrać szablon.
Wygenerowana checklista zawiera punkty z szablonu i uzupełnienia AI.

5.4. Eksport
ID: US-007
Tytuł: Eksport checklisty
Opis: Jako użytkownik, chcę wyeksportować checklistę, aby zapisać wynik review.
Kryteria akceptacji:
Mogę wybrać format eksportu.
Eksport zawiera tytuł, datę i listę punktów z statusami.

6. Metryki sukcesu
Kryterium 1: 60%+ punktów z wygenerowanej checklisty jest używanych (nieusuwanych) przez użytkowników.
Sposób pomiaru: Procent punktów, które zostały zachowane w finalnej liście po edycji.

Kryterium 2: 70% checklist jest eksportowanych po zakończeniu review.
Sposób pomiaru: Odsetek wygenerowanych checklist z akcją eksportu.
