using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaxiWebApplication.Models;

namespace TaxiWebApplication
{
    
    public class OrderHub : Hub
    {
        public OrderHub(ApplicationContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
            //usersId = _context.Users.ToList();        
        }

        //List usersId = new List<string>();
        private ApplicationContext _context;
        private IMemoryCache _cache;

        [Authorize]
        public async Task Send()
        {
            var user = Context.User;
            var userName = Context.User.Identity.Name;

            //var userName = user.IsInRole("user");

            //var drivers = _context.Users.Where(x => x.UserName != "").ToString();

            //var drivers = _context.Users.Where(x => x.UserName != "");

            var Helper_drivers = _context.Users.Join(_context.UserRoles,
                                    u => u.Id,
                                    r => r.UserId,
                                    (u, r) => new
                                    {
                                        UserId = r.UserId,
                                        NickName = u.UserName,
                                        RoleId = r.RoleId
                                    });

            var drivers = Helper_drivers.Join(_context.Roles,
                                              h => h.RoleId,
                                              ro => ro.Id,
                                              (h, ro) => new
                                              {
                                                  UserName = h.NickName,
                                                  Role = ro.Name
                                              }).Where(x => x.Role == "driver");
                                               

            List<string> names = new List<string>();

            string? coord;

            foreach(var u in drivers)
            {
                if (_cache.TryGetValue(u.UserName, out coord))
                {
                    names.Add((u.UserName + coord));
                }
                continue;
            }


            await Clients.User(Context.UserIdentifier).SendAsync("Receive", string.Join(" ", names));

        }
    }
}