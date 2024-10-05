using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.Products
{
    public class ProductDetailVariationMongoDbModel
    {
        public string _id { get; set; }

        public List<ProductDetailVariationAttributesMongoDbModel> variation_attributes { get; set; }
        public double price { get; set; }
        public double profit { get; set; }
        public double amount { get; set; }
        public int quanity_of_stock { get; set; }
        public string sku { get; set; }
    }
    public class ProductDetailVariationAttributesMongoDbModel
    {
        public string id { get; set; }
        public string name { get; set; }
    }
}
