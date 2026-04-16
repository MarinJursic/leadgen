// Namespace `Leadgen.Model.Enums` uvozim jer datoteka koristi tipove iz tog prostora imena.
using Leadgen.Model.Enums;

// Namespace `Leadgen.Model.Entities` definiram kako bih tipove grupirao prema projektu i odgovornosti.
namespace Leadgen.Model.Entities;

// Klasu `ContactChannel` definiram kao nositelja podataka ili ponašanja za ovu domensku cjelinu.
public class ContactChannel
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
{
// Svojstvo `Id` uvodim kako bih spremio jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
    public Guid Id { get; set; }

// Svojstvo `Type` uvodim kako bih spremio vrstu kanala ili entiteta kako bi logika mogla razlikovati scenarije.
    public ContactChannelType Type { get; set; }

// Svojstvo `Value` uvodim kako bih spremio stvarnu vrijednost kanala, npr. email ili URL.
    public string Value { get; set; } = string.Empty;

// Svojstvo `IsVerified` uvodim kako bih spremio oznaku verifikacije kako bismo znali koliko je kontakt pouzdan.
    public bool IsVerified { get; set; }

// Svojstvo `VerifiedAtUtc` uvodim kako bih spremio vrijeme potvrde kanala radi svježine podataka.
    public DateTime? VerifiedAtUtc { get; set; }

// Svojstvo `Source` uvodim kako bih spremio izvor iz kojeg je podatak potvrđen.
    public string Source { get; set; } = string.Empty;

// Svojstvo `ConfidenceScore` uvodim kako bih spremio stupanj sigurnosti kako bismo mogli filtrirati spremnost misije.
    public decimal ConfidenceScore { get; set; }
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
}
