using CommeChesSwa.Models;

namespace CommeChesSwa.Repository
{
    public static class ReservatieRepository
    {
        private static List<Reservatie> _reservaties = new List<Reservatie>();

      
        public static IEnumerable<Reservatie> GetAll()
        {
            return _reservaties.ToList();
        }

        public static Reservatie GetById(int id)
        {
            return _reservaties.Find(r => r.Id == id);
        }

        public static int Add(Reservatie reservatie)
        {
            int id = _reservaties.Count > 0 ? _reservaties.Max(r => r.Id) + 1 : 1;
            reservatie.Id = id;

            _reservaties.Add(reservatie);

            return id;
        }

        public static void Update(Reservatie reservatie)
        {
            Reservatie existing = GetById(reservatie.Id);
            _reservaties.Remove(existing);
            _reservaties.Add(reservatie);
        }

        public static bool Delete(Reservatie reservatie)
        {
            return _reservaties.Remove(reservatie);
        }



    }
}
