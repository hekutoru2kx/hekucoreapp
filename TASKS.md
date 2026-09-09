# Tasks

## Pending

- [ ] **Validate email during registration** — registration creates the Identity user from whatever address is typed (format-checked client-side only) and never confirms it's reachable/owned: `CreateUserAsync` in `backend/Hekucoreapp.Infrastructure/Identity/UserRepository.cs` sets no `EmailConfirmed`, there's no confirmation token, and login (`ValidateUserAsync`) never gates on `EmailConfirmed`. ASP.NET Identity already exposes `GenerateEmailConfirmationTokenAsync`/`ConfirmEmailAsync`; the existing transactional-email pipeline (welcome/password-reset) can send the confirmation link too. The Google OAuth path is fine as-is (`EmailConfirmed = true` — Google already verified it). Same gap found in ludemia, gestamind and hekutenantcoreapp.

## Done

- [x] **Moved `BootstrapAdminEmail` out of `appsettings.json` into user-secrets** — the key in `backend/Hekucoreapp.Api/appsettings.json` held a real personal email in a tracked, non-gitignored file. Set it via `dotnet user-secrets set "BootstrapAdminEmail" "<email>" --project backend/Hekucoreapp.Api` and blanked the default to `""` in `appsettings.json` (same pattern already used for `Jwt:Key`, `SmtpUser`, `SmtpPassword`). `Program.cs` already skips break-glass admin recovery when the value is empty (`if (!string.IsNullOrEmpty(bootstrapEmail))`), so a blank default is safe. The address stays in git history — this only stops further exposure. Fixed in gestamind and hekutenantcoreapp in the same sweep (2026-09-09); ludemia still pending.
- [x] **Renamed `backend/Hekucoreapp.Infrastructure/Respositories/` → `Repositories/`** — the folder name was misspelled while the namespace declared inside the files already read `Hekucoreapp.Infrastructure.Repositories`. Pure `git mv` folder rename, no code change (SDK-style csproj globs `.cs`); backend build clean afterward. Fixed in gestamind and hekutenantcoreapp in the same sweep (2026-09-09); ludemia still pending.
