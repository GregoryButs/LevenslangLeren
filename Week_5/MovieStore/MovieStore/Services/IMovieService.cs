using Movie_Store.Models;
using Movie_Store.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Store.Services
{
    public interface IMovieService
    {
        public IEnumerable<Movie> GetAllMovies();
        public UpdateMovieCommand GetMovieForUpdate(int id);
        public IEnumerable<Genre> GetAllGenres();

        public Movie GetMovieById(int id);

        public void UpdateMovie(UpdateMovieCommand cmd);
        public int CreateMovie(CreateMovieCommand cmd);

        public bool DeleteMovie(int id);
    }
}
