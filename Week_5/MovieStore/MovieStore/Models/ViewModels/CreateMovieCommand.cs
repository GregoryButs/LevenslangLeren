using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Store.Models.ViewModels
{
    public class CreateMovieCommand : EditMovieBase
    {
        public Movie ToMovie()
        {
            return new Movie()
            {
                Title = Title,
                Price = Price,
                GenreId = SelectedGenreId.Value,
                AgeRestriction = AgeRestriction,
                IsDeleted = false                
            };
        }
    }
}
