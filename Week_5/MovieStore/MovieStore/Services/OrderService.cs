using Movie_Store.Models;
using MovieStore_StartHier_OK.Data;
using MovieStore_StartHier_OK.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Store.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public int CreateOrder(CheckOutModel checkOut, string userId)
        {
            Order order = checkOut.ToOrder(userId);
            _context.Orders.Add(order);
            _context.SaveChanges();
            return order.Id;
        }
    }
}
