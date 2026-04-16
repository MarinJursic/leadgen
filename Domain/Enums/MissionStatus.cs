// Namespace `Leadgen.Model.Enums` definiram kako bih tipove grupirao prema projektu i odgovornosti.
namespace Leadgen.Model.Enums;

// Enum `MissionStatus` definiram kako bih ograničio dozvoljene vrijednosti na mali, jasan skup.
public enum MissionStatus
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
{
// Ovu enum vrijednost uvodim kao status za nedovršenu skicu misije.
    Draft = 0,
// Ovu enum vrijednost uvodim kao status koji pokazuje da intake još nije dovoljno jasan.
    NeedsClarification = 1,
// Ovu enum vrijednost uvodim kao status spremnosti za istraživanje.
    ReadyForResearch = 2,
// Ovu enum vrijednost uvodim kao status aktivnog izvođenja.
    Running = 3,
// Ovu enum vrijednost uvodim kao status dovršenog rada.
    Completed = 4,
// Ovu enum vrijednost uvodim kao status arhiviranog zapisa.
    Archived = 5
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
}
