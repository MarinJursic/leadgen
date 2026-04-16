// Namespace `Leadgen.Model.Enums` definiram kako bih tipove grupirao prema projektu i odgovornosti.
namespace Leadgen.Model.Enums;

// Enum `EvidenceKind` definiram kako bih ograničio dozvoljene vrijednosti na mali, jasan skup.
public enum EvidenceKind
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
{
// Ovu enum vrijednost uvodim kao dokaz koji predstavlja opažen signal interesa ili boli.
    Signal = 0,
// Ovu enum vrijednost uvodim kao dokaz koji potvrđuje identitet ili profil.
    Profile = 1,
// Ovu enum vrijednost uvodim kao dokaz na razini organizacije.
    Organization = 2,
// Ovu enum vrijednost uvodim kao dokaz vezan uz kontaktni kanal.
    Contact = 3,
// Ovu enum vrijednost uvodim kao dokaz koji pokazuje odnos između ljudi ili tvrtki.
    Relationship = 4,
// Ovu enum vrijednost uvodim kao dokaz koji služi potvrđivanju točnosti.
    Verification = 5,
// Ovu enum vrijednost uvodim kao dokaz iz sadržaja koji je osoba ili tvrtka objavila.
    Content = 6,
// Ovu enum vrijednost uvodim kao rezervnu vrstu dokaza za sve što ne upada u ostale kategorije.
    Other = 7
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
}
