# UgSocial

**Autor:** Mikhail Marasanov, grupa 4

Miniaturowy serwis społecznościowy zabezpieczony OAuth 2.0 (Authorization Code + PKCE).

## Stos technologiczny

| Komponent | Technologia |
|---|---|
| Backend | ASP.NET Core 10, Minimal API |
| Serwer autoryzacji | OpenIddict 5 (wbudowany) |
| Frontend | Angular 21 + angular-oauth2-oidc |
| Baza danych | PostgreSQL 16 + EF Core 10 |
| Docker | Docker Compose |

## Uruchomienie (Docker)

```bash
docker compose up --build
```

Po uruchomieniu:
- Frontend: http://localhost:4200
- Backend API: http://localhost:8080
- Health check: http://localhost:8080/health

## Uruchomienie lokalne

**Backend** (wymagany PostgreSQL na localhost:5432):
```bash
cd backend/backend
dotnet run
```

**Frontend**:
```bash
cd frontend
npm install
npm start
```

## Konta testowe

| Email | Hasło | Rola |
|---|---|---|
| moderator@ugsocial.local | Mod123! | moderator |
| user@ugsocial.local | User123! | user |

## Testy

```bash
cd backend
dotnet test UgSocial.Tests
```

## Endpointy API

| Metoda | Ścieżka | Dostęp |
|---|---|---|
| GET | /health | publiczny |
| GET | /api/posts | zalogowany |
| GET | /api/posts/{id} | zalogowany |
| POST | /api/posts | zalogowany |
| DELETE | /api/posts/{id} | tylko moderator |
| GET | /api/users/me | zalogowany |

## PKCE (Proof Key for Code Exchange)

Na początku przepływu OAuth Angular generuje losowy ciąg znaków (`code_verifier`) i oblicza jego skrót SHA-256 (`code_challenge`). Do żądania `/connect/authorize` przekazywany jest wyłącznie skrót. Podczas wymiany authorization code na tokeny (POST `/connect/token`) Angular wysyła oryginalny `code_verifier`. OpenIddict weryfikuje, że SHA-256(`code_verifier`) === zapisany `code_challenge`. Chroni to przed przechwyceniem authorization code — bez `code_verifier` kod jest bezużyteczny.

## Serwer autoryzacji

Zastosowano **OpenIddict** — open-source'owy serwer OAuth 2.0 / OpenID Connect wbudowany bezpośrednio w aplikację ASP.NET Core. Obsługuje Authorization Code Flow + PKCE, przechowuje klientów i tokeny w PostgreSQL za pośrednictwem EF Core.
