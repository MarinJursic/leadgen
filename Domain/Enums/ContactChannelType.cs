// Namespace `Leadgen.Model.Enums` definiram kako bih tipove grupirao prema projektu i odgovornosti.
namespace Leadgen.Model.Enums;

// Enum `ContactChannelType` definiram kako bih ograničio dozvoljene vrijednosti na mali, jasan skup.
public enum ContactChannelType
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
{
// Ovu enum vrijednost uvodim kao vrstu kanala za poslovni email jer je najvažniji izlaz za outreach.
    WorkEmail = 0,
// Ovu enum vrijednost uvodim kao vrstu kanala za privatni email kad poslovni nije dostupan.
    PersonalEmail = 1,
// Ovu enum vrijednost uvodim kao vrstu kanala za telefonski kontakt.
    Phone = 2,
// Ovu enum vrijednost uvodim kao vrstu kanala za LinkedIn profil ili poruku.
    LinkedIn = 3,
// Ovu enum vrijednost uvodim kao vrstu kanala za X identitet.
    X = 4,
// Ovu enum vrijednost uvodim kao vrstu kanala za GitHub profil.
    GitHub = 5,
// Ovu enum vrijednost uvodim kao vrstu kanala za Reddit identitet.
    Reddit = 6
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
}
