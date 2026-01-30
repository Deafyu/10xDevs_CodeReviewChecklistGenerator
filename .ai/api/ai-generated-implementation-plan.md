# Plan generowania checklisty (AI)

## 1. Wejście
- changeDescription
- tryb compare: codeBefore + codeAfter
- tryb single: codeOnly
- template items (opcjonalnie)

## 2. Logika
- Compare: analizujemy tylko linie zmienione
- Single: analizujemy cały blok
- Deduplikacja i trim

## 3. Wyjście
- lista punktów do review
- format JSON: { "items": ["..."] }

## 4. Przykład
- input: 1 zmieniona linia
- output: 1-3 punkty specyficzne dla tej linii
