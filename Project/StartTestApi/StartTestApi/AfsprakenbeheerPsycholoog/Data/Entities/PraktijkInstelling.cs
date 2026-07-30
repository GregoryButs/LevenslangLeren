using System;

namespace AfsprakenbeheerPsycholoog.Data.Entities
{
    public class PraktijkInstelling : BaseEntity
    {
        public string GoogleCalendarId { get; set; } = "primary";

        // Werkuren per dag (uu:mm) & Actief-vlaggen (Interval 1 & 2)
        public bool MaandagActief { get; set; } = true;
        public string MaandagStart { get; set; } = "09:00";
        public string MaandagEinde { get; set; } = "12:00";
        public bool Maandag2Actief { get; set; } = true;
        public string MaandagStart2 { get; set; } = "13:00";
        public string MaandagEinde2 { get; set; } = "17:00";

        public bool DinsdagActief { get; set; } = true;
        public string DinsdagStart { get; set; } = "09:00";
        public string DinsdagEinde { get; set; } = "12:00";
        public bool Dinsdag2Actief { get; set; } = true;
        public string DinsdagStart2 { get; set; } = "13:00";
        public string DinsdagEinde2 { get; set; } = "17:00";

        public bool WoensdagActief { get; set; } = true;
        public string WoensdagStart { get; set; } = "09:00";
        public string WoensdagEinde { get; set; } = "12:00";
        public bool Woensdag2Actief { get; set; } = true;
        public string WoensdagStart2 { get; set; } = "13:00";
        public string WoensdagEinde2 { get; set; } = "17:00";

        public bool DonderdagActief { get; set; } = true;
        public string DonderdagStart { get; set; } = "09:00";
        public string DonderdagEinde { get; set; } = "12:00";
        public bool Donderdag2Actief { get; set; } = true;
        public string DonderdagStart2 { get; set; } = "13:00";
        public string DonderdagEinde2 { get; set; } = "17:00";

        public bool VrijdagActief { get; set; } = true;
        public string VrijdagStart { get; set; } = "09:00";
        public string VrijdagEinde { get; set; } = "12:00";
        public bool Vrijdag2Actief { get; set; } = true;
        public string VrijdagStart2 { get; set; } = "13:00";
        public string VrijdagEinde2 { get; set; } = "17:00";

        public bool ZaterdagActief { get; set; } = false;
        public string ZaterdagStart { get; set; } = "10:00";
        public string ZaterdagEinde { get; set; } = "12:00";
        public bool Zaterdag2Actief { get; set; } = false;
        public string ZaterdagStart2 { get; set; } = "13:00";
        public string ZaterdagEinde2 { get; set; } = "17:00";

        public bool ZondagActief { get; set; } = false;
        public string ZondagStart { get; set; } = "10:00";
        public string ZondagEinde { get; set; } = "12:00";
        public bool Zondag2Actief { get; set; } = false;
        public string ZondagStart2 { get; set; } = "13:00";
        public string ZondagEinde2 { get; set; } = "17:00";

        // Rusttijd & Slotduur
        public int SlotDuurMinuten { get; set; } = 60;
        public int BufferMinuten { get; set; } = 15;

        // Aangeboden Locatie-opties
        public bool LocatiePraktijk { get; set; } = true;
        public bool LocatieGoogleMeet { get; set; } = true;
        public bool LocatieTelefoon { get; set; } = true;

        // Limieten (Planning Window)
        public int MinimaalVoorafUren { get; set; } = 12;
        public int MaximaleToekomstDagen { get; set; } = 30;
    }
}
