namespace WebsiteComputer.Models
{
    public class ClientDtos
    {
       
        public record ClientDetail
        {
            public int accountID { get; set; } 
            public string clientCode { get; set; } = "";
            public string username { get; set; } = "";
            public string password { get; set; } = "";
            public string clientName { get; set; } = "";
            public string phoneNumber { get; set; } = "";
            public string clientAddress { get; set; } = "";
            public decimal totalMoney { get; set; } = 0;

        }
        public record ClientLogin(
            string username, 
            string password
            );
        public class ClientInformation
        {
            public string clientCode { get; set; } = "";
            public string clientName { get; set; } = "";
            public string phoneNumber { get; set; } = "";
            public string clientAddress { get; set; } = "";
            public decimal totalMoney { get; set; } = 0;
        }

        public record Cart
        {
            public int cartID { get; init; }
            public int clientID { get; set; }
            public int cartItemID { get; set; }
            public int productID { get; set; }
            public decimal price { get; set; }

        }
       
    }
}
