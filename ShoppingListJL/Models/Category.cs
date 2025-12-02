using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Maui.Storage;

namespace ShoppingListJL.Models
{
    internal class Category
    {
        public string Name { get; set; } = string.Empty;
        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();

        public ShoppingList ParentList { get; set; }

        public void AddProduct(Product p)
        {
            int copyNr = 0;
            foreach (var item in Products)
            {
                if (item.Name.Contains(p.Name) || (item.Name.Contains(p.Name) && (item.Name.Substring(item.Name.Length - 4)).Contains(" ")))
                    copyNr++;
            }
            if (copyNr > 0)
            {
                p.Name += $" ({copyNr})";
            }
            Products.Add(p);
            ParentList?.Save();
        }
        public void DeleteCategory(string listName, string categoryName)
        {
            string filePath = Path.Combine(FileSystem.AppDataDirectory, "shoppinglists.xml");
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);
            XDocument doc = XDocument.Load(filePath);
            XElement root = doc.Root ?? throw new Exception("Invalid XML");
            var listEl = root.Elements("ShoppingList")
                .FirstOrDefault(x =>
                    string.Equals(x.Element("Name")?.Value, listName, StringComparison.OrdinalIgnoreCase));
            if (listEl == null)
                throw new Exception($"List '{listName}' not found.");
            var categoryEl = listEl
                .Element("Categories")?
                .Elements("Category")
                .FirstOrDefault(x =>
                    string.Equals(x.Element("Name")?.Value, categoryName, StringComparison.OrdinalIgnoreCase));
            if (categoryEl == null)
                throw new Exception($"Category '{categoryName}' not found.");
            categoryEl.Remove();
            doc.Save(filePath);
        }
    }
}
