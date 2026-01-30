# API Endpoint Implementation Plan: POST /api/checklists

## 1. Przegląd
Zapis nowej checklisty z itemami.

## 2. Body
- title
- changeDescription
- mode (compare/single)
- codeBefore/codeAfter lub codeOnly
- items[]

## 3. Walidacja
- title, changeDescription wymagane
- items min. 1
