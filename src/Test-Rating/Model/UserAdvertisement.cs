using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Test_Rating.Model
{

    public class UserAdvertisement
    {

        [Key]
        public int UserAdvertisementId { get; set; }
        
        public User User { get; set; }

        public Advertisement Advertisement { get; set; }

        public int Rating { get; set; }


    }
}
