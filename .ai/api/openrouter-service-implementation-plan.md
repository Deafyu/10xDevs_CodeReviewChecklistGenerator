# Plan integracji OpenRouter

## 1. Przegląd
OpenRouter generuje checklistę na podstawie zmian w kodzie lub pojedynczego bloku kodu.

## 2. Konfiguracja
- OpenRouter:ApiKey
- OpenRouter:BaseUrl
- OpenRouter:DefaultModel
- OpenRouter:FallbackModels[]

## 3. Request
- Endpoint: POST /chat/completions
- Format: messages + response_format=json_object

## 4. Response
- JSON `{ "items": ["..."] }`

## 5. Błędy
- 404: model niedostępny => fallback
- 401/403: błędny klucz => komunikat do UI
