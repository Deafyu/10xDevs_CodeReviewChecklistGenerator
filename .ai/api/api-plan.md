# Plan API (MVP)

## 1. Przegląd
API obsługuje generowanie checklist code review, zapisywanie wyników, zarządzanie szablonami oraz eksport. Dostęp tylko dla zalogowanych użytkowników (Identity cookies).

## 2. Kontekst techniczny
- ASP.NET Core (Minimal APIs)
- ASP.NET Core Identity (cookie auth)
- Entity Framework Core + SQL Server
- OpenRouter (chat/completions)

## 3. Konwencje
- JSON jako standard odpowiedzi
- Statusy HTTP: 200/201/204 dla sukcesu, 401/404/422/500 dla błędów
- Zasada: dane tylko dla właściciela (UserId)

## 4. Endpointy

### Checklisty
1. `GET /api/checklists`
   - Opis: lista checklist użytkownika
   - Odpowiedź: `[{ id, title, createdAt, updatedAt, itemCount }]`

2. `GET /api/checklists/{id}`
   - Opis: szczegóły + itemy
   - Odpowiedź: obiekt checklisty z listą itemów

3. `POST /api/checklists/generate`
   - Opis: generowanie checklisty przez AI
   - Body: `mode`, `codeBefore`, `codeAfter`, `codeOnly`, `changeDescription`, `templateId`
   - Odpowiedź: `{ items: string[] }`

4. `POST /api/checklists`
   - Opis: zapis nowej checklisty
   - Body: `title`, `changeDescription`, `codeBefore`/`codeAfter` lub `codeOnly`, `items[]`
   - Odpowiedź: 201 Created + obiekt

5. `PUT /api/checklists/{id}`
   - Opis: aktualizacja checklisty + itemów
   - Body: jak wyżej
   - Odpowiedź: 204 No Content

6. `DELETE /api/checklists/{id}`
   - Opis: usunięcie checklisty
   - Odpowiedź: 204 No Content

7. `GET /api/checklists/{id}/export`
   - Opis: eksport TXT
   - Odpowiedź: `text/plain`

### Szablony
1. `GET /api/templates`
2. `POST /api/templates`
3. `PUT /api/templates/{id}`
4. `DELETE /api/templates/{id}`

## 5. Walidacja (serwer)
- `title` wymagany
- `changeDescription` wymagany
- tryb compare: wymagane `codeBefore` i `codeAfter`
- tryb single: wymagane `codeOnly`
- `items[]` min. 1 przy zapisie

## 6. Błędy
- 401: brak autoryzacji
- 404: brak zasobu lub brak dostępu
- 422: walidacja
- 500: błąd serwera (logowany)
