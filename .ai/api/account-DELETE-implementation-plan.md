# API Endpoint Implementation Plan: DELETE /api/account

## 1. Przegląd
Usunięcie konta użytkownika oraz danych domenowych (checklisty i szablony).

## 2. Żądanie
- Metoda: DELETE
- URL: /api/account
- Body: brak

## 3. Odpowiedź
- 200 { success: true }
- 401 brak autoryzacji
- 500 błąd usuwania

## 4. Kroki
1. Sprawdź zalogowanie.
2. Usuń checklisty i szablony użytkownika.
3. Usuń konto Identity.
4. Wyloguj użytkownika.
