using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Test_Rating.Model;

namespace Test_Rating.Data
{

    public class ApiContext : DbContext
    {
        public ApiContext(DbContextOptions<ApiContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Advertisement> Advertisements { get; set; }

        public DbSet<UserAdvertisement> UserAdvertisement { get; set; }

        public DbSet<AdvertisementType> AdvertisementType { get; set; }



    }


}


