# Plan bazy danych (MVP)

## 1. Encje i pola
### Checklist
- Id (Guid)
- UserId (string)
- Title (string)
- CodeBefore (string)
- CodeAfter (string)
- ChangeDescription (string)
- CreatedAt (DateTimeOffset)
- UpdatedAt (DateTimeOffset)

### ChecklistItem
- Id (Guid)
- ChecklistId (Guid)
- Text (string)
- IsChecked (bool)
- SortOrder (int)

### ChecklistTemplate
- Id (Guid)
- UserId (string)
- Name (string)
- Description (string?)
- CreatedAt (DateTimeOffset)
- UpdatedAt (DateTimeOffset)

### ChecklistTemplateItem
- Id (Guid)
- TemplateId (Guid)
- Text (string)
- SortOrder (int)

## 2. Relacje
- Checklist 1..N ChecklistItem (cascade)
- ChecklistTemplate 1..N ChecklistTemplateItem (cascade)

## 3. Indeksy
- Checklist(UserId)
- ChecklistTemplate(UserId)

## 4. Migracje
- Jedna migracja tworząca tabele domenowe
