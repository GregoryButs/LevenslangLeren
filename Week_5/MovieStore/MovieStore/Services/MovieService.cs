using Microsoft.EntityFrameworkCore;
using Movie_Store.Models;
using Movie_Store.Models.ViewModels;
using MovieStore_StartHier_OK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Store.Services
{
    public class MovieService : IMovieService
    {
        private readonly ApplicationDbContext _context;

        public MovieService(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }

        public IEnumerable<Genre> GetAllGenres()
        {
            return _context.Genres.ToList();
        }

        public IEnumerable<Movie> GetAllMovies()
        {
            return _context.Movies.Include(x => x.Genre).Where(x => !x.IsDeleted).ToList();
        }

        public UpdateMovieCommand GetMovieForUpdate(int id)
        {
            Movie movie = _context.Movies.Include(x => x.Genre).Where(x => x.Id == id && !x.IsDeleted).FirstOrDefault();
            if(movie == null)
            {
                return null;
            }

            return new UpdateMovieCommand(movie);
        }

        public Movie GetMovieById(int id)
        {
            return _context.Movies.Include(x => x.Genre).Where(x => x.Id == id && !x.IsDeleted).SingleOrDefault();
        }

        public void UpdateMovie(UpdateMovieCommand cmd)
        {
            Movie toEdit = _context.Movies.Find(cmd.Id);

            if(toEdit != null)
            {
                cmd.UpdateMovie(toEdit);
                _context.SaveChanges();
            }
        }

        public int CreateMovie(CreateMovieCommand cmd)
        {
            Movie movie = cmd.ToMovie();
            _context.Movies.Add(movie);

            _context.SaveChanges();

            return movie.Id;
        }

        public bool DeleteMovie(int id)
        {
            Movie toDelete = _context.Movies.Find(id);

            if(toDelete == null)
            {
                return false;
            }

            toDelete.IsDeleted = true;

            _context.SaveChanges();

            return true;
        }
    }
}
