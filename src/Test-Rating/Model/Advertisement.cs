﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Test_Rating.Model
{
    public class Advertisement
    {
        [Key]
        public int Id { get; set; }

        public string Description { get; set; }

    }
}