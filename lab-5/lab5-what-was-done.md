# Lab 5 - Sto je napravljeno

## Najvaznije na vrhu: upload i Google OAuth

### Kako radi upload datoteka

Upload je vezan uz postojecu misiju (`BusinessDnaMission`). Korisnik mora biti prijavljen kao `Admin` ili `Manager`, otvara edit stranicu misije, odabire datoteku i klikne `Upload`. Stranica ne radi full refresh nego JavaScript salje datoteku preko `fetch` poziva i `FormData`.

Upload tok:

1. `Views/Missions/Edit.cshtml` prikazuje upload formu na edit stranici misije.
2. JavaScript u `Views/Missions/Edit.cshtml` napravi `FormData` i salje `POST /missions/{missionId}/attachments`.
3. `Controllers/MissionsController.cs` metoda `UploadAttachment` prima `IFormFile`, provjerava postoji li misija, validira velicinu i ekstenziju.
4. Datoteka se sprema na disk u `wwwroot/uploads/missions/{missionId}/` pod generiranim sigurnijim imenom.
5. `Domain/Entities/MissionAttachment.cs` definira metadata zapis: originalni naziv, storage naziv, content type, velicina, vrijeme uploada i korisnik.
6. `Domain/Entities/BusinessDnaMission.cs` ima kolekciju `Attachments`, pa je svaka datoteka vezana uz konkretnu misiju.
7. `Data/LeadgenDbContext.cs` ima `DbSet<MissionAttachment>` i konfigurira relaciju misija -> attachmenti.
8. `Migrations/20260609071304_AddLab5IdentityAndMissionAttachments.cs` dodaje tablicu `MissionAttachments`.
9. `Views/Missions/_AttachmentList.cshtml` renderira listu uploadanih datoteka koju frontend ponovno ucita AJAX-om.

Brisanje uploadane datoteke ide slicno: `Delete` button u `_AttachmentList.cshtml` pokrene JavaScript u `Edit.cshtml`, on salje `POST /missions/attachments/{id}/delete`, a `MissionsController.DeleteAttachment` brise datoteku s diska i metadata zapis iz baze.

### Kroz koje fileove prolazi Google OAuth login

Google OAuth je external login preko ASP.NET Core Identity. Korisnik klikne Google button, aplikacija ga preusmjeri na Google, Google nakon odobrenja vrati korisnika na `/signin-google`, a aplikacija dovrsi lokalni Identity login.

OAuth tok:

1. `leadgen.csproj` ima package `Microsoft.AspNetCore.Authentication.Google` i `UserSecretsId` za lokalne tajne.
2. `Program.cs` cita `Authentication:Google:ClientId` i `Authentication:Google:ClientSecret` iz konfiguracije.
3. Ako su oba podatka postavljena, `Program.cs` registrira Google provider kroz `AddAuthentication().AddGoogle(...)`.
4. `Views/Shared/_LoginPartial.cshtml` prikazuje Google button u navigaciji kada je Google provider konfiguriran.
5. `Views/Account/Login.cshtml` i `Views/Account/Register.cshtml` prikazuju `Continue with Google` button.
6. Klik na button salje `POST /account/external-login` na `Controllers/AccountController.cs`.
7. `AccountController.ExternalLogin` zove `ConfigureExternalAuthenticationProperties` i vraca `Challenge`, sto salje korisnika na Google OAuth stranicu.
8. Google vraca korisnika na default callback `/signin-google`.
9. `AccountController.ExternalLoginCallback` cita external login info i pokusava prijaviti postojeceg korisnika.
10. Ako korisnik prvi put dolazi preko Googlea, prikazuje se `Views/Account/ExternalLoginConfirmation.cshtml`.
11. `AccountController.ExternalLoginConfirmation` kreira ili povezuje lokalnog `AppUser` korisnika, dodaje Google login zapis i potpisuje korisnika u aplikaciju.
12. `Models/Identity/AppUser.cs` je lokalni Identity korisnik s dodatnim poljima `DisplayName`, `OIB` i `JMBG`.

Google secret nije commitan u repository. Lokalno se sprema kroz `dotnet user-secrets`, a `appsettings.json` ostaje bez stvarnih vrijednosti.

## Sto je trebalo napraviti

| Zahtjev iz Lab 5 | Je li napravljeno? | Gdje je pokriveno |
| --- | --- | --- |
| Implementirati kompletnu API podrsku za sve entitete kroz CRUD i DTO modele | Da | `Controllers/Api/LeadgenApiControllers.cs`, `Models/Api/LeadgenDtos.cs` |
| Omoguciti `GET` listu s pretragom, `GET` po ID-u, `POST`, `PUT` i `DELETE` | Da | Svi endpointi pod `/api/...` |
| Ne izlagati EF entitete direktno kroz API | Da | API koristi DTO i write DTO klase |
| Omoguciti autentikaciju kroz ASP.NET Core Identity | Da | `Program.cs`, `Models/Identity/AppUser.cs`, `Controllers/AccountController.cs` |
| Prosiriti `AppUser` trazenim poljima `OIB` i `JMBG` | Da | `Models/Identity/AppUser.cs` |
| Omoguciti lokalnu registraciju i prijavu | Da | `Views/Account/Register.cshtml`, `Views/Account/Login.cshtml` |
| Implementirati role `Admin` i barem jos jednu rolu | Da | `Admin` i `Manager` u `Models/Identity/LeadgenRoles.cs` |
| Ograniciti create/edit/delete akcije autorizacijom | Da | MVC i API controlleri koriste `[Authorize]` atribute |
| Omoguciti upload datoteka vezan uz konkretan zapis | Da | Upload je vezan uz `BusinessDnaMission` kroz `MissionAttachment` |
| Spremiti uploadane datoteke na disk i metapodatke u bazu | Da | `wwwroot/uploads/missions/{missionId}` i tablica `MissionAttachments` |
| Popis datoteka ucitati AJAX pozivom | Da | `Views/Missions/Edit.cshtml`, `Views/Missions/_AttachmentList.cshtml` |
| Omoguciti brisanje postojecih datoteka | Da | `MissionsController.DeleteAttachment` |
| Omoguciti Google ili Facebook login | Da | Google provider je konfiguriran u `Program.cs`; stvarni secret ide kroz konfiguraciju |
| Implementirati integracijske testove za API CRUD endpointe | Da | `leadgen.Tests/LeadgenApiCrudTests.cs` |
| Testirati uspjesne scenarije, nepostojece ID-eve i validacijske greske | Da | 56 integracijskih testova prolazi |

## Kratki sazetak

Lab 5 je prosirio Leadgen MVC aplikaciju s API slojem, autentikacijom, autorizacijom, uploadom datoteka i integracijskim testovima.

U originalnom zadatku primjer se vrti oko kvizova. U ovoj aplikaciji nema `Quiz` entiteta, pa je zahtjev za upload datoteka mapiran na najblizi stvarni domenski korijen: `BusinessDnaMission`. Zato su datoteke vezane uz konkretnu misiju.

## Kako isprobati Lab 5 funkcionalnosti

### 1. Pokretanje aplikacije

Iz root foldera projekta pokrenuti:

```bash
dotnet restore
dotnet build leadgen.sln
dotnet run --project leadgen.csproj
```

Aplikacija se otvara na:

- `http://localhost:5267`
- ili `https://localhost:7135`, ovisno o launch profilu

Ako baza ne postoji, aplikacija sama pokrece migracije i seed podataka pri startu.

### 2. Kako isprobati login

Otvoriti:

```text
http://localhost:5267/account/login
```

Prijaviti se s jednim od seedanih korisnika:

| Rola | Email | Lozinka | Sto moze |
| --- | --- | --- | --- |
| Admin | `admin@leadgen.local` | `LeadgenAdmin1!` | create, edit i delete |
| Manager | `manager@leadgen.local` | `LeadgenManager1!` | create i edit, ali ne delete |

Za provjeru:

1. Otvori `/missions`.
2. Klikni neku misiju.
3. Ako nisi prijavljen, detalji ili edit akcije preusmjeravaju prema loginu.
4. Nakon login-a u navigaciji se vidi prijavljeni korisnik i `Sign out`.

Datoteke kroz koje prolazi login:

- `Program.cs` - registrira Identity, cookie paths i authentication middleware
- `Models/Identity/AppUser.cs` - korisnicki model s `OIB` i `JMBG`
- `Models/Identity/LeadgenRoles.cs` - role `Admin` i `Manager`
- `Data/LeadgenIdentitySeeder.cs` - seeda role i demo korisnike
- `Controllers/AccountController.cs` - login, register, logout i external login flow
- `ViewModels/Account/LoginViewModel.cs` - model za login formu
- `ViewModels/Account/RegisterViewModel.cs` - model za registraciju
- `Views/Account/Login.cshtml` - login forma
- `Views/Account/Register.cshtml` - register forma
- `Views/Shared/_LoginPartial.cshtml` - prikaz login/register/sign out linkova u layoutu
- `Views/Shared/_Layout.cshtml` - ukljucuje login partial u navigaciju

### 3. Kako isprobati upload datoteke

Upload je dostupan samo na edit stranici misije, jer misija mora vec imati ID.

Koraci:

1. Pokreni aplikaciju.
2. Prijavi se kao `admin@leadgen.local` ili `manager@leadgen.local`.
3. Otvori:

```text
http://localhost:5267/missions
```

4. Odaberi jednu misiju.
5. Otvori edit akciju za tu misiju.
6. Na desnoj strani edit forme nalazi se sekcija `Supporting documents`.
7. Odaberi datoteku i klikni `Upload`.
8. Datoteka se pojavi u listi bez refresha stranice.

Dozvoljene ekstenzije u trenutnoj implementaciji:

- `.csv`
- `.doc`
- `.docx`
- `.jpeg`
- `.jpg`
- `.json`
- `.md`
- `.pdf`
- `.png`
- `.txt`

Datoteke se spremaju na disk ovdje:

```text
wwwroot/uploads/missions/{missionId}/
```

Metapodaci se spremaju u bazu u tablicu:

```text
MissionAttachments
```

### 4. Kroz koje fileove prolazi upload

Upload flow ide ovim redom:

| Korak | Datoteka | Sto radi |
| --- | --- | --- |
| 1 | `Views/Missions/Edit.cshtml` | Prikazuje upload formu i JavaScript koji salje file preko `fetch` + `FormData` |
| 2 | `Controllers/MissionsController.cs` | Akcija `UploadAttachment` prima `IFormFile`, validira file i sprema ga |
| 3 | `Domain/Entities/MissionAttachment.cs` | Definira metadata zapis za uploadanu datoteku |
| 4 | `Domain/Entities/BusinessDnaMission.cs` | Ima kolekciju `Attachments` |
| 5 | `Data/LeadgenDbContext.cs` | Ima `DbSet<MissionAttachment>` i konfigurira vezu misija -> attachmenti |
| 6 | `Migrations/20260609071304_AddLab5IdentityAndMissionAttachments.cs` | Dodaje tablicu `MissionAttachments` u bazu |
| 7 | `Views/Missions/_AttachmentList.cshtml` | Renderira AJAX listu uploadanih datoteka |

Detaljan upload tok:

1. Korisnik odabere file na `Edit.cshtml`.
2. JavaScript kreira `FormData`.
3. `fetch` salje `POST` na `/missions/{missionId}/attachments`.
4. `MissionsController.UploadAttachment` provjeri postoji li misija.
5. Controller provjeri file size i ekstenziju.
6. File se sprema na disk pod generiranim imenom.
7. U bazu se sprema `MissionAttachment` metadata zapis.
8. Frontend ponovo ucita listu attachmenta preko AJAX-a.

### 5. Kako isprobati brisanje uploadane datoteke

Koraci:

1. Uploadaj datoteku na edit stranici misije.
2. U listi uploadanih datoteka klikni `Delete`.
3. Datoteka nestaje iz liste bez refresha stranice.
4. Datoteka se brise s diska.
5. Metadata zapis se brise iz tablice `MissionAttachments`.

Delete flow ide ovim redom:

| Korak | Datoteka | Sto radi |
| --- | --- | --- |
| 1 | `Views/Missions/_AttachmentList.cshtml` | Prikazuje `Delete` button za svaki attachment |
| 2 | `Views/Missions/Edit.cshtml` | JavaScript hvata klik i salje `POST` zahtjev za brisanje |
| 3 | `Controllers/MissionsController.cs` | Akcija `DeleteAttachment` pronalazi zapis, brise file s diska i brise zapis iz baze |
| 4 | `Data/LeadgenDbContext.cs` | Sprema promjenu u bazu kroz EF Core |

Ruta za brisanje:

```text
POST /missions/attachments/{id}/delete
```

### 6. Kako isprobati testove

Pokrenuti:

```bash
dotnet test leadgen.sln
```

Ocekivani rezultat:

```text
Passed! - Failed: 0, Passed: 56, Skipped: 0, Total: 56
```

Glavne test datoteke:

- `leadgen.Tests/leadgen.Tests.csproj`
- `leadgen.Tests/LeadgenApiTestFactory.cs`
- `leadgen.Tests/LeadgenApiCrudTests.cs`

Sto testovi rade:

- pokrecu aplikaciju kroz `WebApplicationFactory<Program>`
- koriste privremenu SQLite bazu
- koriste test authentication handler
- pozivaju stvarne API rute preko `HttpClient`
- provjeravaju CRUD za sve API controllere
- provjeravaju `404` za nepostojece ID-eve
- provjeravaju `400` za validacijske greske
- provjeravaju `401` kada korisnik nije prijavljen
- provjeravaju `403` kada korisnik nema admin rolu za delete

## 1. API podrska za sve entitete

Dodan je DTO-based Web API za cijeli Leadgen domenski graf.

API controlleri su u:

- `Controllers/Api/LeadgenApiControllers.cs`

DTO i request modeli su u:

- `Models/Api/LeadgenDtos.cs`

Pokriveni API endpointi:

- `/api/missions`
- `/api/clarification-questions`
- `/api/mission-runs`
- `/api/mission-agent-assignments`
- `/api/swarm-agents`
- `/api/target-companies`
- `/api/target-contacts`
- `/api/contact-channels`
- `/api/evidence-points`
- `/api/lead-dossiers`
- `/api/mission-attachments`

Svaki glavni API podrzava:

- `GET` za listu zapisa
- `GET /{id}` za jedan zapis
- `POST` za kreiranje
- `PUT /{id}` za izmjenu
- `DELETE /{id}` za brisanje
- `query` parametar za osnovnu pretragu gdje ima smisla

API ne vraca direktno EF entitete, nego DTO klase. To je bitno jer se tako kontrolira koje podatke klijent vidi i izbjegavaju se ciklicki JSON problemi s navigacijskim svojstvima.

## 2. Autentikacija

Dodan je ASP.NET Core Identity.

Glavne datoteke:

- `Models/Identity/AppUser.cs`
- `Models/Identity/LeadgenRoles.cs`
- `Data/LeadgenIdentitySeeder.cs`
- `Program.cs`
- `Controllers/AccountController.cs`
- `Views/Account/*`
- `Views/Shared/_LoginPartial.cshtml`

`AppUser` nasljeduje Identity korisnika i prosiren je trazenim poljima:

- `DisplayName`
- `OIB`
- `JMBG`

Dodane su MVC stranice za:

- lokalni login
- lokalnu registraciju
- logout
- external login callback
- external login confirmation
- access denied

Seedani razvojni korisnici:

- `admin@leadgen.local` / `LeadgenAdmin1!`
- `manager@leadgen.local` / `LeadgenManager1!`

## 3. Role i autorizacija

Implementirane su role:

- `Admin`
- `Manager`

Pravila pristupa:

- liste i javni API list endpointi dostupni su anonimnim korisnicima
- detalji zahtijevaju prijavu
- create/edit zahtijevaju `Admin` ili `Manager`
- delete zahtijeva `Admin`

Ta pravila su primijenjena na MVC controllere i API controllere.

## 4. Google external login

Dodan je Google login wiring u `Program.cs`.

Aplikacija cita ove konfiguracijske kljuceve:

- `Authentication:Google:ClientId`
- `Authentication:Google:ClientSecret`

Stvarni Google secret nije commitan u repository. Mora se postaviti kroz user secrets ili environment varijable.

Primjer:

```bash
dotnet user-secrets set "Authentication:Google:ClientId" "..."
dotnet user-secrets set "Authentication:Google:ClientSecret" "..."
```

## 5. Upload datoteka

Dodan je novi entitet:

- `Domain/Entities/MissionAttachment.cs`

`BusinessDnaMission` sada ima kolekciju:

- `Attachments`

Upload je dostupan na edit stranici misije:

- `Views/Missions/Edit.cshtml`
- `Views/Missions/_AttachmentList.cshtml`

Akcije su u:

- `Controllers/MissionsController.cs`

Podrzano je:

- async upload datoteke preko `fetch` + `FormData`
- spremanje datoteke na disk pod `wwwroot/uploads/missions/{missionId}`
- spremanje metapodataka u bazu
- AJAX ucitavanje popisa datoteka
- AJAX brisanje datoteke i metadata zapisa

Datoteka se na disku sprema pod generiranim imenom, a originalni naziv ostaje u bazi.

## 6. EF migracija

Dodan je migration:

- `Migrations/20260609071304_AddLab5IdentityAndMissionAttachments.cs`

Migracija dodaje:

- Identity tablice (`AspNetUsers`, `AspNetRoles`, itd.)
- stupce `DisplayName`, `OIB`, `JMBG` na korisniku
- tablicu `MissionAttachments`

Dodan je i design-time DbContext factory:

- `Data/LeadgenDbContextFactory.cs`

To omogucuje da `dotnet ef` radi bez pokretanja cijelog web startupa.

## 7. Integracijski testovi

Dodan je testni projekt:

- `leadgen.Tests/leadgen.Tests.csproj`

Glavne test datoteke:

- `leadgen.Tests/LeadgenApiTestFactory.cs`
- `leadgen.Tests/LeadgenApiCrudTests.cs`

Testovi koriste:

- `WebApplicationFactory<Program>`
- privremenu SQLite bazu
- test authentication handler
- stvarne HTTP pozive preko `HttpClient`

Pokriveno je:

- uspjesan CRUD za API controllere
- `GET all`
- `GET by id`
- `POST`
- `PUT`
- `DELETE`
- nepostojeci ID-evi
- validacijske greske
- anonimni pristup javnim listama
- zastita endpointa koji traze prijavu
- admin-only delete pravilo

Zadnja provjera:

```bash
dotnet build leadgen.sln
dotnet test leadgen.sln
```

Rezultat:

- build prolazi bez gresaka
- `56` integracijskih testova prolazi

## 8. Bitne napomene za obranu

- DTO klase su uvedene da API ne izlozi EF entitete direktno.
- Upload je vezan uz misiju jer je `BusinessDnaMission` domenski ekvivalent kvizu iz zadatka.
- Google login je konfiguriran, ali za stvaran login trebaju pravi Google credentials.
- Role su seedane u bazu pri pokretanju aplikacije.
- Integration testovi ne koriste mock controller testove, nego stvarne HTTP zahtjeve kroz aplikaciju.
