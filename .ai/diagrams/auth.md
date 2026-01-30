# Diagram: Autoryzacja

```mermaid
sequenceDiagram
  participant U as Użytkownik
  participant A as Aplikacja
  U->>A: Rejestracja
  A-->>U: Konto aktywne
  U->>A: Logowanie
  A-->>U: Sesja cookie
  U->>A: Dostęp do checklist
  A-->>U: Dane użytkownika
```
