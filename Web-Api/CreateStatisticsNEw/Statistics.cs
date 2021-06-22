using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Web_Api.Models
{
    public class Statistics : TableEntity
    {
        public string AnzahlBuecher { get; set; }
        public string AnzahlAutoren { get; set; }
        public string AnzahlBenutzer { get; set; }

    }
}
