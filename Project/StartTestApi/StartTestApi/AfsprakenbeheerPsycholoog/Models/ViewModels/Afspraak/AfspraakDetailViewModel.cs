namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak
{
    public class AfspraakDetailViewModel
    {
        public int Id { get; set; }
        public string PatientVolledigeNaam { get; set; }
        public string PatientEmail { get; set; }
        public string PatientTelefoon { get; set; }
        public string AfspraakTypeNaam { get; set; }
        public string Kleurcode { get; set; }
        public DateTime Starttijd { get; set; }
        public DateTime Eindtijd { get; set; }
        public string Status { get; set; }
        public string? Opmerkingen { get; set; }

        public string StatusLabel => Status switch
        {
            "Gepland" => "bg-primary",
            "Voltooid" => "bg-success",
            "Geannuleerd" => "bg-danger",
            _ => "bg-secondary"
        };
    }
}
