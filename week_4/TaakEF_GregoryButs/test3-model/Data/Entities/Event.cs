using test4_start.Data.Entities;

namespace test3_model.Data.Entities

{
    public class Event : BaseEntity
    {
        public string Titel { get; set; } 
        public string Beschrijving { get; set; }
        public DateTime StartDatumTijd { get; set; }
        public DateTime EindDatumTijd { get; set; }
        public bool IsGeannuleerd { get; set; }
       

        // Relatie naar Begeleider
        public int BegeleiderId { get; set; }
        public Begeleider Begeleider { get; set; }

       

        //Relatie naar Locatie  
        public int LocatieId { get; set; }
        public Locatie Locatie { get; set; }
    }
}
