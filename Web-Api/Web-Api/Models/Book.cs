using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.WindowsAzure.Storage.Table;

namespace Web_Api.Models
{
    public class Book
    {
        public string id { get; set; }

        public string Titel { get; set; }

        public string Autor { get; set; }

        public string ISBN { get; set; }

        public DateTime Erscheinungsdatum { get; set; }

        public int Seitenanzahl { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
