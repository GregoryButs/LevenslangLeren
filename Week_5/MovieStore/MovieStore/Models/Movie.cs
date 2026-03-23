using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Store.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public double Price { get; set; }
        public AgeRestriction? AgeRestriction { get; set; }
        public int GenreId { get; set; }
        public Genre Genre { get; set; }

        public bool IsDeleted { get; set; }
    }

    public enum AgeRestriction
    {
        Plus12, Plus16, Plus18
    }
}
