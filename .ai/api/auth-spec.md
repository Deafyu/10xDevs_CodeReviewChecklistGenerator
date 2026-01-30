# Specyfikacja uwierzytelniania (MVP)

## 1. Mechanizm
- ASP.NET Core Identity
- Cookie auth

## 2. Rejestracja
- Email + hasło
- Konto aktywne od razu (brak potwierdzenia email)
- Po rejestracji automatyczne logowanie

## 3. Logowanie
- Formularz /Identity/Account/Login
- Standardowy flow Identity

## 4. Ograniczenia
- Polityka silnych haseł (do ustawienia w IdentityOptions)
- Lockout po 5 błędnych próbach (do ustawienia)

## 5. Autoryzacja
- /Checklists, /Templates oraz /api/* wymagają zalogowania
- Strona główna publiczna
