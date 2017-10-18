using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Test_Rating.Data;
using Microsoft.EntityFrameworkCore;

namespace Test_Rating.Controllers
{

    [Route("Api/[controller]")]
    public class RatingController : Controller
    {

        readonly ApiContext _context;

        public RatingController(ApiContext context)
        {
            _context = context;
        }

        [Route("GetRating")]        
        [HttpPost]
        public IActionResult GetRating()
        {

            var users = _context.UserAdvertisement
                .Include(x=>x.User)
                .Include(x => x.Advertisement)
                .Select(x => x)
                .Where(x => x.User.UserId == 1)
                .ToList();

            var response = users.Select(u => new
            {
                firstName = u.User.Name                
            });

            try
            {
                SVDPP.SVDPP.Main1();

                var t1 = SVDPP.SVDPP.OrigemMatrix;
                var t2 = SVDPP.SVDPP.ReturnMatrix;
                
                t1.Append("<br><br>");
                t1.Append(t2);

                return Ok(t1.ToString());

            }
            catch (Exception ex )
            {

                throw ex;
            }                        
                        
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
