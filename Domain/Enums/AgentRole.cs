// Namespace `Leadgen.Model.Enums` definiram kako bih tipove grupirao prema projektu i odgovornosti.
namespace Leadgen.Model.Enums;

// Enum `AgentRole` definiram kako bih ograničio dozvoljene vrijednosti na mali, jasan skup.
public enum AgentRole
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
{
// Ovu enum vrijednost uvodim kao vrijednost za agenta koji razlaže misiju i planira swarm rad.
    Strategist = 0,
// Ovu enum vrijednost uvodim kao vrijednost za agenta koji traži kompanije.
    Scout = 1,
// Ovu enum vrijednost uvodim kao vrijednost za agenta koji rezolvira prave osobe.
    Anchor = 2,
// Ovu enum vrijednost uvodim kao vrijednost za agenta koji vadi kvalifikacijske signale.
    Soul = 3,
// Ovu enum vrijednost uvodim kao vrijednost za agenta koji skuplja tržišne i news dokaze.
    Sentinel = 4,
// Ovu enum vrijednost uvodim kao vrijednost za agenta koji spaja identitete i kanale.
    Stitcher = 5,
// Ovu enum vrijednost uvodim kao vrijednost za agenta koji rješava rubne i nejasne slučajeve.
    Sniper = 6
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
}
