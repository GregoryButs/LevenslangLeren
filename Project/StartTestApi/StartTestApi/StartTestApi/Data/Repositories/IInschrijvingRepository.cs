using StartTestApi.Data.Entities;

namespace StartTestApi.Data.Repositories
{
    public interface IInschrijvingRepository: IRepository<Inschrijving>
    {
        Inschrijving? GetByIdWithEvent(int id);
        void AddInschrijving(Inschrijving inschrijving);

    }
}
