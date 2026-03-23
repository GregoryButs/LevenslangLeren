using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Store.Models.ViewModels
{
    public class UpdateMovieCommand : EditMovieBase
    {
        public int Id { get; set; }

        public UpdateMovieCommand()
        {
            //default constructor needed for model-binding
        }

        public UpdateMovieCommand(Movie movie)
        {
            Id = movie.Id;
            Title = movie.Title;
            Price = movie.Price;
            SelectedGenreId = movie.GenreId;
            AgeRestriction = movie.AgeRestriction;
        }

        public void UpdateMovie(Movie movie)
        {
            movie.Title = Title;
            movie.Price = Price;
            movie.GenreId = SelectedGenreId.Value;
            movie.AgeRestriction = AgeRestriction;
        }
    }
}
