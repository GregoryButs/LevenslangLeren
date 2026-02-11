namespace MusicApp.Models
{
    public class Album
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public double Rating { get; set; }
        public string ImageURL { get; set; }
        public List<Song> Songs { get; set; }
    }
}
