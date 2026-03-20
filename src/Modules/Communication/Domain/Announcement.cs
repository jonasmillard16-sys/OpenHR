namespace RegionHR.Communication.Domain;

public enum AnnouncementStatus { Utkast, Publicerad, Arkiverad }
public enum AnnouncementPriority { Normal, Viktig, Kritisk }

public sealed class Announcement
{
    public Guid Id { get; private set; }
    public string Titel { get; private set; } = default!;
    public string Innehall { get; private set; } = default!;
    public AnnouncementStatus Status { get; private set; }
    public AnnouncementPriority Prioritet { get; private set; }
    public DateTime SkapadVid { get; private set; }
    public string SkapadAv { get; private set; } = default!;
    public DateTime? PubliceradVid { get; private set; }

    private Announcement() { }

    public static Announcement Skapa(string titel, string innehall, AnnouncementPriority prioritet, string skapadAv)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(titel);
        ArgumentException.ThrowIfNullOrWhiteSpace(innehall);
        return new Announcement
        {
            Id = Guid.NewGuid(), Titel = titel, Innehall = innehall,
            Prioritet = prioritet, Status = AnnouncementStatus.Utkast,
            SkapadVid = DateTime.UtcNow, SkapadAv = skapadAv
        };
    }

    public void Publicera() { if (Status != AnnouncementStatus.Utkast) throw new InvalidOperationException("Kan bara publicera utkast."); Status = AnnouncementStatus.Publicerad; PubliceradVid = DateTime.UtcNow; }
    public void Arkivera() { if (Status != AnnouncementStatus.Publicerad) throw new InvalidOperationException("Kan bara arkivera publicerad."); Status = AnnouncementStatus.Arkiverad; }
}
