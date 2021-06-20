using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web_Api.Models
{
    public class Request
    {
        public string Id { get; set; }
    }

    public class UserRequest : Request
    {
        public string Vorname { get; set; }
        public string Nachname { get; set; }
        public string Email { get; set; }
        public string Hash { get; set; }
    }

    public class BookRequest : Request
    {
        public string Titel { get; set; }
        public string Autor { get; set; }
        public string ISBN { get; set; }
        public DateTime Erscheinungsdatum { get; set; }
        public int Seitenanzahl { get; set; }
    }


}
