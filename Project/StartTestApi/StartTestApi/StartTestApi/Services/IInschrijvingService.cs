using StartTestApi.DTO;

namespace StartTestApi.Services
{
    public interface IInschrijvingService
    {
        InschrijvingDTO? GetInschrijvingById(int id);
        void CreateInschrijving(CreateInschrijvingDTO createInschrijvingDto);
        void DeleteInschrijving(int id);
    }
}
