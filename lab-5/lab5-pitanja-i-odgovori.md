# Lab 5 - Moguca pitanja i kratki odgovori

## Najvaznije za obranu: upload, login/register i OAuth

### Kako radi upload?

Upload je vezan uz `BusinessDnaMission`, odnosno uz konkretnu misiju. Korisnik na edit stranici misije odabere datoteku, JavaScript salje `FormData` na controller, controller validira datoteku, sprema file na disk i sprema metadata zapis u bazu.

Kroz koje fileove prolazi upload:

| Datoteka | Uloga |
| --- | --- |
| `Views/Missions/Edit.cshtml` | Prikazuje upload formu i JavaScript koji salje AJAX upload |
| `Controllers/MissionsController.cs` | `UploadAttachment` prima i sprema file, `DeleteAttachment` brise file |
| `Domain/Entities/MissionAttachment.cs` | Entitet za metadata uploadane datoteke |
| `Domain/Entities/BusinessDnaMission.cs` | Misija ima kolekciju attachmenta |
| `Data/LeadgenDbContext.cs` | EF Core konfiguracija i `DbSet<MissionAttachment>` |
| `Migrations/20260609071304_AddLab5IdentityAndMissionAttachments.cs` | Migracija za tablicu `MissionAttachments` |
| `Views/Missions/_AttachmentList.cshtml` | Partial view za AJAX listu uploadanih datoteka |

Kratak odgovor: file se fizicki sprema u `wwwroot/uploads/missions/{missionId}/`, a podaci o fileu se spremaju u tablicu `MissionAttachments`.

### Kako rade login i register?

Login i register rade preko ASP.NET Core Identity. Register kreira lokalnog `AppUser` korisnika, a login provjerava email i password hash te postavlja authentication cookie.

Kroz koje fileove prolazi login/register:

| Datoteka | Uloga |
| --- | --- |
| `Program.cs` | Registrira Identity, cookie login path, access denied path, authentication i authorization middleware |
| `Models/Identity/AppUser.cs` | Lokalni korisnik s poljima `DisplayName`, `OIB` i `JMBG` |
| `Models/Identity/LeadgenRoles.cs` | Definira role `Admin` i `Manager` |
| `Data/LeadgenIdentitySeeder.cs` | Seeda role i demo korisnike za login |
| `ViewModels/Account/RegisterViewModel.cs` | Model za register formu |
| `Views/Account/Register.cshtml` | Register forma; salje `POST /account/register` |
| `Controllers/AccountController.cs` | `Register` kreira korisnika preko `_userManager.CreateAsync` i prijavi ga preko `_signInManager.SignInAsync` |
| `ViewModels/Account/LoginViewModel.cs` | Model za login formu |
| `Views/Account/Login.cshtml` | Login forma; salje `POST /account/login` |
| `Controllers/AccountController.cs` | `Login` provjerava podatke preko `_signInManager.PasswordSignInAsync` |
| `Views/Shared/_LoginPartial.cshtml` | Prikazuje sign in/register/google/sign out linkove u navigaciji |
| `Views/Shared/_Layout.cshtml` | Ukljucuje login partial u layout |

Kratak odgovor: `Register` sprema novog korisnika u Identity tablice, a `Login` provjerava korisnika i postavlja cookie da je korisnik prijavljen.

### Kako radi Google OAuth?

Google OAuth je external login preko ASP.NET Core Identity. Aplikacija ne cuva Google lozinku. Korisnik klikne Google button, aplikacija ga salje na Google, Google ga vraca na `/signin-google`, a aplikacija zatim prijavi ili kreira lokalnog `AppUser` korisnika.

Kroz koje fileove prolazi OAuth:

| Datoteka | Uloga |
| --- | --- |
| `leadgen.csproj` | Ima Google auth package i `UserSecretsId` |
| `Program.cs` | Cita Google `ClientId`/`ClientSecret` i registrira `AddGoogle` |
| `Views/Shared/_LoginPartial.cshtml` | Prikazuje Google login u navigaciji |
| `Views/Account/Login.cshtml` | Prikazuje `Continue with Google` na login stranici |
| `Views/Account/Register.cshtml` | Prikazuje `Continue with Google` na register stranici |
| `Controllers/AccountController.cs` | `ExternalLogin`, `ExternalLoginCallback` i `ExternalLoginConfirmation` |
| `Views/Account/ExternalLoginConfirmation.cshtml` | Dovrsavanje lokalnog profila nakon prve Google prijave |
| `Models/Identity/AppUser.cs` | Lokalni korisnik koji se povezuje s Google loginom |

Kratak odgovor: `Program.cs` registrira Google provider, login/register button salje korisnika u `AccountController.ExternalLogin`, Google vraca na `/signin-google`, a `AccountController` dovrsava prijavu kroz ASP.NET Core Identity.

## Sto je trebalo napraviti i status

| Zahtjev | Status |
| --- | --- |
| API podrska za sve entitete, CRUD i DTO | Napravljeno |
| Pretraga kroz API list endpointove | Napravljeno |
| Autentikacija kroz ASP.NET Core Identity | Napravljeno |
| `AppUser` prosiren s `OIB` i `JMBG` | Napravljeno |
| Lokalna registracija i prijava | Napravljeno |
| Role `Admin` i `Manager` | Napravljeno |
| Autorizacija za public/details/create/edit/delete pravila | Napravljeno |
| Upload datoteka vezan uz konkretan zapis | Napravljeno, vezano uz `BusinessDnaMission` |
| Spremanje datoteka na disk i metapodataka u bazu | Napravljeno |
| AJAX popis i brisanje uploadanih datoteka | Napravljeno |
| Google external login | Napravljeno kroz konfiguraciju; pravi secret nije commitan |
| Integracijski testovi za API CRUD, nepostojece ID-eve i validaciju | Napravljeno, 56 testova prolazi |

## API i DTO

### 1. Sto je cilj Lab 5 vjezbe?

Cilj je dodati API podrsku, autentikaciju, autorizaciju, upload datoteka, external login i integracijske testove.

### 2. Zasto si dodao API controllere?

Zato da aplikacija ne vraca samo MVC HTML stranice, nego i strukturirane podatke kroz JSON endpointove koje mogu koristiti drugi klijenti ili JavaScript.

### 3. Koja je razlika izmedu MVC controllera i API controllera?

MVC controller najcesce vraca `View`, a API controller vraca podatke i HTTP status kodove, najcesce kao JSON.

### 4. Zasto API controller ima `[ApiController]`?

Zato jer `[ApiController]` ukljucuje API-friendly ponasanja kao automatsku validaciju modela, bolji model binding i standardne odgovore za validacijske greske.

### 5. Koje HTTP metode koristi API?

Koristi `GET`, `POST`, `PUT` i `DELETE`.

### 6. Sto radi `GET`?

`GET` dohvaca podatke. U aplikaciji se koristi za listu zapisa i dohvat jednog zapisa po ID-u.

### 7. Sto radi `POST`?

`POST` kreira novi zapis.

### 8. Sto radi `PUT`?

`PUT` mijenja postojeci zapis po ID-u.

### 9. Sto radi `DELETE`?

`DELETE` brise postojeci zapis po ID-u.

### 10. Zasto API ne vraca direktno EF entitete?

Zato sto EF entiteti mogu sadrzavati interna polja, navigacijska svojstva i ciklicke veze. DTO daje kontrolu nad oblikom API odgovora.

### 11. Sto je DTO?

DTO je `Data Transfer Object`, odnosno klasa koja definira podatke koje API prima ili vraca.

### 12. Gdje su DTO klase?

U `Models/Api/LeadgenDtos.cs`.

### 13. Koji API endpointi su dodani?

Dodani su endpointi za misije, pitanja, runove, assignmente, agente, kompanije, kontakte, kanale, evidence, dossiere i attachmentse.

### 14. Kako radi pretraga u API-ju?

API list endpointi primaju `query` parametar i filtriraju zapise po najvaznijim tekstualnim poljima.

### 15. Koji status se vraca kada zapis ne postoji?

Vraca se `404 Not Found`.

### 16. Koji status se vraca kada se zapis uspjesno kreira?

Vraca se `201 Created`.

### 17. Koji status se vraca kod uspjesnog brisanja?

Vraca se `204 No Content`.

### 18. Koji status se vraca kod validacijske greske?

Vraca se `400 Bad Request`.

## Autentikacija i Identity

### 19. Sto je autentikacija?

Autentikacija odgovara na pitanje tko je korisnik.

### 20. Sto je autorizacija?

Autorizacija odgovara na pitanje smije li poznati korisnik napraviti neku akciju.

### 21. Koji sustav je koristen za autentikaciju?

Koristen je ASP.NET Core Identity.

### 22. Zasto nisi pisao vlastiti login sustav?

Identity vec rjesava hashiranje lozinki, cookie login, role, claimove i sigurnosne detalje, pa je sigurnije koristiti framework.

### 23. Sto je `AppUser`?

`AppUser` je aplikacijska korisnicka klasa koja nasljeduje Identity korisnika i dodaje polja specifična za aplikaciju.

### 24. Koja su dodatna polja na `AppUser`?

Dodata su `DisplayName`, `OIB` i `JMBG`.

### 25. Zasto su dodani OIB i JMBG?

Zato sto Lab 5 trazi prosirenje osnovne korisnicke tablice tim podacima.

### 26. Gdje je konfiguriran Identity?

U `Program.cs`.

### 27. Koje su seedane role?

Seedane su role `Admin` i `Manager`.

### 28. Gdje se seedaju role i korisnici?

U `Data/LeadgenIdentitySeeder.cs`.

### 29. Koji su demo korisnici?

`admin@leadgen.local` s lozinkom `LeadgenAdmin1!` i `manager@leadgen.local` s lozinkom `LeadgenManager1!`.

### 30. Koja je razlika izmedu Admin i Manager role?

`Admin` moze i brisati, dok `Manager` moze kreirati i uredjivati, ali ne smije brisati.

### 31. Gdje se prikazuje login/logout u UI-u?

U shared partialu `Views/Shared/_LoginPartial.cshtml`, koji je ukljucen u layout.

### 32. Sto se dogada ako korisnik nema pravo pristupa?

Aplikacija ga salje na access denied stranicu ili API vraca `403 Forbidden`.

## Autorizacija

### 33. Koje akcije su javne?

Liste i javni API list endpointi dostupni su anonimnim korisnicima.

### 34. Koje akcije traze prijavu?

Detalji i svi write endpointi traze prijavljenog korisnika.

### 35. Koje akcije traze Admin ili Manager?

Create i Edit akcije traze `Admin` ili `Manager`.

### 36. Koje akcije traze samo Admin?

Delete akcije traze `Admin`.

### 37. Kako je autorizacija oznacena u kodu?

Koristi se `[Authorize]`, `[AllowAnonymous]` i `[Authorize(Roles = "...")]`.

### 38. Sto znaci `401 Unauthorized`?

Korisnik nije autentificiran.

### 39. Sto znaci `403 Forbidden`?

Korisnik je autentificiran, ali nema potrebnu rolu ili dozvolu.

## Google login

### 40. Je li Google login implementiran?

Da, dodan je Google authentication provider wiring u `Program.cs`.

### 41. Zasto stvarni Google secret nije u repositoryju?

Zato sto se tajne vrijednosti ne smiju commitati. Trebaju ici u user secrets, environment varijable ili secret manager.

### 42. Koji konfiguracijski kljucevi trebaju za Google login?

`Authentication:Google:ClientId` i `Authentication:Google:ClientSecret`.

### 43. Sto se dogada kod prve Google prijave?

Korisnik mora potvrditi ili dopuniti lokalni profil, ukljucujuci `OIB` i `JMBG`.

## Upload datoteka

### 44. Uz koji entitet je vezan upload?

Upload je vezan uz `BusinessDnaMission`.

### 45. Zasto nije vezan uz kviz?

Ova aplikacija nema `Quiz` entitet. Najblizi domenski ekvivalent je misija, jer je ona korijenski zapis za cijeli Leadgen proces.

### 46. Koji entitet cuva metapodatke o datoteci?

`MissionAttachment`.

### 47. Koji se metapodaci spremaju?

Spremaju se originalni naziv, storage naziv, putanja, content type, velicina, vrijeme kreiranja i korisnik koji je upload napravio.

### 48. Gdje se datoteke spremaju?

Na disk pod `wwwroot/uploads/missions/{missionId}`.

### 49. Kako se radi upload na frontendu?

Koristi se async `fetch` s `FormData`, bez full page reloada.

### 50. Kako se ucitava popis datoteka?

Popis se ucitava AJAX pozivom koji vraca partial view `_AttachmentList`.

### 51. Kako se brise datoteka?

AJAX poziv salje zahtjev controlleru, controller brise datoteku s diska i brise metadata zapis iz baze.

### 52. Zasto se datoteka sprema pod generiranim imenom?

Da se izbjegne konflikt naziva i smanji rizik od opasnih ili nepredvidivih uploadanih file nameova.

### 53. Koje zastite postoje kod uploada?

Provjerava se da file postoji, da nije prazan, da nije prevelik i da ekstenzija spada u dozvoljene tipove.

## EF Core i migracije

### 54. Koja migracija je dodana za Lab 5?

`AddLab5IdentityAndMissionAttachments`.

### 55. Sto migracija dodaje?

Dodaje Identity tablice i tablicu `MissionAttachments`.

### 56. Zasto `LeadgenDbContext` sada nasljeduje `IdentityDbContext<AppUser>`?

Zato da isti DbContext upravlja i Leadgen domenskim tablicama i Identity tablicama.

### 57. Zasto je dodan design-time DbContext factory?

Da EF CLI moze generirati i primijeniti migracije bez oslanjanja na cijeli runtime startup aplikacije.

## Integracijski testovi

### 58. Koji testni projekt je dodan?

Dodan je `leadgen.Tests`.

### 59. Sto koristi testni projekt za pokretanje aplikacije?

Koristi `WebApplicationFactory<Program>`.

### 60. Zasto su ovo integracijski testovi, a ne samo unit testovi?

Zato sto pozivaju stvarne HTTP endpointove i provjeravaju routing, model binding, validaciju, autorizaciju, EF i JSON odgovore zajedno.

### 61. Koju bazu koriste testovi?

Koriste privremenu SQLite bazu.

### 62. Zasto testovi ne koriste development bazu?

Da budu izolirani, ponovljivi i ne ovise o lokalnom stanju aplikacije.

### 63. Kako testovi simuliraju prijavljenog korisnika?

Koriste custom test authentication handler koji dodaje testnog korisnika i rolu.

### 64. Sto testovi pokrivaju?

Pokrivaju CRUD, nepostojece ID-eve, validacijske greske, anonimni pristup javnim listama, zasticene endpointove i admin-only delete.

### 65. Koliko testova prolazi?

Prolazi 56 integracijskih testova.

## Obrambena pitanja o odluke u dizajnu

### 66. Zasto si sve API controllere stavio u jednu datoteku?

Za Lab 5 je to prakticno jer dijele isti generic CRUD base pattern. U vecem projektu moglo bi se razdvojiti u vise datoteka.

### 67. Zasto postoji generic base API controller?

Da se ne ponavlja isti CRUD kod za svaki entitet, nego da svaki controller definira samo query, mapiranje, validaciju i posebna delete pravila.

### 68. Kako se sprjecava overposting?

API prima write DTO modele, a controller rucno mapira dozvoljena polja na postojece EF entitete.

### 69. Zasto delete ponekad prvo brise povezane dossiere?

Zbog restriktivnih EF odnosa. Neki entiteti su referencirani iz `LeadDossier`, pa ih treba ukloniti prije brisanja parent zapisa.

### 70. Sto bi jos poboljsao u produkciji?

Dodao bih bolji storage za datoteke, granularne permissione, logging audit traila, rate limiting API-ja i prave OAuth production secret vrijednosti.

## Brzi odgovori koje je dobro zapamtiti

- DTO se koristi da API ne izlozi EF entitet direktno.
- `401` znaci da korisnik nije prijavljen.
- `403` znaci da je prijavljen, ali nema pravo.
- `Admin` moze brisati.
- `Manager` moze kreirati i uredjivati.
- Google login treba prave secret vrijednosti izvan repositoryja.
- Upload je vezan uz misiju jer Leadgen nema kviz.
- Testovi koriste stvarne HTTP pozive kroz `WebApplicationFactory`.
