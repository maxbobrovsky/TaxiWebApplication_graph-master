using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaxiWebApplication.Models;

namespace TaxiWebApplication
{
    
    public class OrderHub : Hub
    {
        public OrderHub(ApplicationContext context)
        {
            _context = context;
            //usersId = _context.Users.ToList();        
        }

        //List usersId = new List<string>();
        private ApplicationContext _context;

      

        [Authorize]
        public async Task Send()
        {
            var user = Context.User;
            var userName = Context.User.Identity.Name;

            //var userName = user.IsInRole("user");

            //var drivers = _context.Users.Where(x => x.UserName != "").ToString();

            var drivers = _context.Users.Where(x => x.UserName != "");

            List<string> names = new List<string>();

            foreach(var u in drivers)
            {
                names.Add(u.UserName);
            }


            await Clients.User(Context.UserIdentifier).SendAsync("Receive", string.Join(" ", names));

        }
    }
}