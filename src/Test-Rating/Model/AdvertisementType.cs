using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Test_Rating.Model
{
    public class AdvertisementType
    {

        [Key]
        public int AdvertisementTypeId { get; set; }

        public string description { get; set; }

    }
}
