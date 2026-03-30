using Movie_Store.Models;
using MovieStore_StartHier_OK.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Store.Services
{
    public interface IOrderService
    {
        int CreateOrder(CheckOutModel checkOut, string userId);

    }
}
