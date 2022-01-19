using AutoMapper;
using ExpnesesManager.Models;

namespace ExpnesesManager.Services
{
    public class AutoMapperProfiles : Profile
    {

        public AutoMapperProfiles()
        {
            CreateMap<Account, CreateAccountViewModel>();
            CreateMap<UpdateTransactionViewModel, Transaction>().ReverseMap();

        }

    }
}
