using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Test_Rating.Model;

namespace Test_Rating.Data
{
    static public class AddTestData
    {
        public static void SetTestData(ApiContext context)
        {
            var ListUserAdvertisement = new UserAdvertisement();
            ListUserAdvertisement.Advertisement = new Advertisement();
            ListUserAdvertisement.User = new User();

            var UserCount = 200;
            var AdvertisementCount = 300;

            for (int i = 1; i < UserCount; i++)
            {
                var User = new User
                {
                    UserId = i,
                    Name = "User" + i,
                };

                context.Users.Add(User);
            }

            for (int i = 1; i < AdvertisementCount; i++)
            {
                var Advertisement = new Advertisement
                {
                    Description = "Advertisement"+i,
                    Id = i
                };

                context.Advertisements.Add(Advertisement);
            }

            for (int i = 1; i < 10; i++)
            {
                var rnd = new Random();

                var user = new User();
                user.UserId = i;
                var advertisement = new Advertisement();
                advertisement.Id = i;

                var UserAdvertisement = new UserAdvertisement
                {
                    UserAdvertisementId = i,
                    Advertisement = advertisement,
                    User = user,
                    Rating = rnd.Next(1, 6)
                };

                context.UserAdvertisement.Add(UserAdvertisement);

            }
                        
            context.SaveChanges();
        }
    }
}
