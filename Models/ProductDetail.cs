using System;
using WebsiteComputer.Models;
using System.Collections.Generic;
using System.Text;

namespace WebsiteComputer.Models
{
    public record ProductMainInfo
    {
        public string Name { get; init; } = "";
        public decimal Price { get; init; }
        public int Stock { get; init; }
        public string Brand { get; init; } = "";
        public string? Thumbnail { get; init; }
    }

    public record ProductDetail
    {
        public string ProductId { get; init; } = "";
        public string Name { get; init; } = "";
        public decimal Price { get; init; }
        public int Stock { get; init; }
        public string Brand { get; init; } = "";
        public string? Thumbnail { get; init; }
        public List<string> Images { get; init; } = new();
        public List<ProductSpec?> Specs { get; init; }
    }

    public record CreateUpdateProduct
    {
        public string ProductCode { get; init; } = "";
        public string Name { get; init; } = "";
        public decimal Price { get; init; }
        public string Brand { get; init; } = "";
        public string Category { get; init; } = "";
        public string? description { get; set; } = "";
        public int Stock { get; init; }
        public DateTime Time { get; init; }// use to Create or update
        public string? Thumbnail { get; set; }
        public List<string?> image { get; set; } = [];
    }
    public record ProductSpec
    {
        public string SpecKey { get; set; } = "a";
        public string SpecValue { get; set; } = "a";

    }

    
}
