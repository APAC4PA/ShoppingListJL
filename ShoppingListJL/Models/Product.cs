using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;

namespace ShoppingListJL.Models
{
    internal class Product : ObservableObject
    {
        private string _name = string.Empty;
        private string _quantity = string.Empty;
        private string _unit = string.Empty;
        private bool _optional = false;
        private string _store = string.Empty;
        private bool _bought = false;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public string Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }
        public string Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }
        public bool Optional
        {
            get => _optional;
            set => SetProperty(ref _optional, value);
        }
        public string Store
        {
            get => _store;
            set => SetProperty(ref _store, value);
        }
        public bool Bought
        {
            get => _bought;
            set => SetProperty(ref _bought, value);
        }

        public void MarkProductAsBought(string listName, Category category, string productName)
        {
            Bought = !Bought;
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
                    string.Equals(x.Element("Name")?.Value, category.Name, StringComparison.OrdinalIgnoreCase));

            if (categoryEl == null)
                throw new Exception($"Category '{category.Name}' not found.");

            var productEl = categoryEl
                .Element("Products")?
                .Elements("Product")
                .FirstOrDefault(x =>
                    string.Equals(x.Element("Name")?.Value, productName, StringComparison.OrdinalIgnoreCase));

            if (productEl == null)
                throw new Exception($"Product '{productName}' not found.");

            var boughtEl = productEl.Element("Bought");
            boughtEl.Value = Bought.ToString();

            doc.Save(filePath);
        }
        public void DeleteProduct(string listName, Category category, Product product)
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
                    string.Equals(x.Element("Name")?.Value, category.Name, StringComparison.OrdinalIgnoreCase));
            if (categoryEl == null)
                throw new Exception($"Category '{category.Name}' not found.");
            var productEl = categoryEl
                .Element("Products")?
                .Elements("Product")
                .FirstOrDefault(x =>
                    string.Equals(x.Element("Name")?.Value, product.Name, StringComparison.OrdinalIgnoreCase));
            if (productEl == null)
                throw new Exception($"Product '{product.Name}' not found.");
            productEl.Remove();
            category.Products.Remove(product);
            doc.Save(filePath);
        }
    }
}
