﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TechShopSolution.Data.Entities
{
    public class Contact
    {
        public int id { get; set; }
        public string company_name { get; set; }
        public string adress { get; set; }
        public string phone { get; set; }
        public string hotline { get; set; }
        public string company_logo { get; set; }
        public string fax { get; set; }
        public string email { get; set; }
        public string social_fb { get; set; }
        public string social_instagram { get; set; }
        public string social_youtube { get; set; }
        public string social_twitter{ get; set; }
    }
}
