using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ShoppingListJL.Models
{
    internal class Product : ObservableObject
    {
        private string _name = string.Empty;
        private string _quantity = string.Empty;
        private string _unit = string.Empty;
        private string _optional = "False";
        private string _store = string.Empty;

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
        public string Optional
        {
            get => _optional;
            set => SetProperty(ref _optional, value);
        }
        public string Store
        {
            get => _store;
            set => SetProperty(ref _store, value);
        }

        public void MarkProductAsBought(string listName, string categoryName, string productName)
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

            var productEl = categoryEl
                .Element("Products")?
                .Elements("Product")
                .FirstOrDefault(x =>
                    string.Equals(x.Element("Name")?.Value, productName, StringComparison.OrdinalIgnoreCase));

            if (productEl == null)
                throw new Exception($"Product '{productName}' not found.");

            var nameEl = productEl.Element("Name");

            if (nameEl != null && !nameEl.Value.Contains("✓"))
            {
                nameEl.Value = $"{nameEl.Value} | ✓ |";
                doc.Save(filePath);
            }
            else if (nameEl.Value.Contains("✓"))
            {
                nameEl.Value = nameEl.Value.Replace(" | ✓ |", string.Empty);
                doc.Save(filePath);
            }
        }
        public void DeleteProduct(string listName, string categoryName, string productName)
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
            var productEl = categoryEl
                .Element("Products")?
                .Elements("Product")
                .FirstOrDefault(x =>
                    string.Equals(x.Element("Name")?.Value, productName, StringComparison.OrdinalIgnoreCase));
            if (productEl == null)
                throw new Exception($"Product '{productName}' not found.");
            productEl.Remove();
            doc.Save(filePath);
        }
    }
}
