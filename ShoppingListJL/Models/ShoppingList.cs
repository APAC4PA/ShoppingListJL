using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Maui.Storage;

namespace ShoppingListJL.Models
{
    internal class ShoppingList
    {
        public string Name { get; set; } = string.Empty;

        public ObservableCollection<Category> Categories { get; } = new();

        public void AddCategory(Category c)
        {
            if (c == null) return;
            int copyNr = 0;
            foreach (var cat in Categories)
            {
                if (cat.Name.Contains(c.Name) || (cat.Name.Contains(c.Name) && (cat.Name.Substring(cat.Name.Length - 4)).Contains(" ")))
                {
                    copyNr++;
                    Trace.WriteLine($"Existing category: {cat.Name}");
                    Trace.WriteLine($"Category: {c.Name}");
                }
            }
            if (copyNr > 0)
            {
                c.Name += $" ({copyNr})";
            }
            c.ParentList = this;
            Categories.Add(c);
            Save();
        }
        public void AddProduct(string categoryName, Product p)
        {
            if (p == null) return;

            var cat = Categories.FirstOrDefault(c => c.Name == categoryName);
            if (cat == null)
                return;

            cat.Products.Add(p);
            Save();
        }
        public void Save()
        {
            string filePath = Path.Combine(FileSystem.AppDataDirectory, "shoppinglists.xml");

            XDocument doc = File.Exists(filePath)
                ? XDocument.Load(filePath)
                : new XDocument(new XElement("ShoppingLists"));

            XElement root = doc.Root!;

            root.Elements("ShoppingList")
                .FirstOrDefault(x => x.Element("Name")?.Value == Name)
                ?.Remove();

            var newList = new XElement("ShoppingList",
                new XElement("Name", Name),
                new XElement("Categories",
                    from c in Categories
                    select new XElement("Category",
                        new XElement("Name", c.Name),
                        new XElement("Products",
                            from p in c.Products
                            select new XElement("Product",
                                new XElement("Name", p.Name),
                                new XElement("Quantity", p.Quantity),
                                new XElement("Unit", p.Unit),
                                new XElement("Optional", p.Optional),
                                new XElement("Store", p.Store ?? string.Empty)
                            )
                        )
                    )
                )
            );
            Trace.WriteLine($"{filePath}");
            root.Add(newList);
            doc.Save(filePath);
        }
        public static ShoppingList Load(string name)
        {
            string filePath = Path.Combine(FileSystem.AppDataDirectory, "shoppinglists.xml");
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            XDocument doc = XDocument.Load(filePath);
            XElement root = doc.Root ?? throw new FileNotFoundException(filePath);
            name = Uri.UnescapeDataString(name);
            var el = root.Elements("ShoppingList")
                         .FirstOrDefault(x => x.Element("Name")?.Value == name)
                         ?? throw new FileNotFoundException($"ShoppingList '{name}' not found");

            var list = new ShoppingList { Name = name };

            foreach (var cEl in el.Element("Categories")?.Elements("Category") ?? Enumerable.Empty<XElement>())
            {
                var category = new Category
                {
                    Name = cEl.Element("Name")?.Value ?? "",
                    ParentList = list
                };

                foreach (var pEl in cEl.Element("Products")?.Elements("Product") ?? Enumerable.Empty<XElement>())
                {
                    category.Products.Add(new Product
                    {
                        Name = pEl.Element("Name")?.Value ?? "",
                        Quantity = pEl.Element("Quantity")?.Value ?? "",
                        Unit = pEl.Element("Unit")?.Value ?? "",
                        Optional = pEl.Element("Optional")?.Value ?? "",
                        Store = pEl.Element("Store")?.Value ?? ""
                    });
                }

                list.Categories.Add(category);
            }

            return list;
        }
        public static IEnumerable<ShoppingList> LoadAll()
        {
            string filePath = Path.Combine(FileSystem.AppDataDirectory, "shoppinglists.xml");
            if (!File.Exists(filePath))
                return Enumerable.Empty<ShoppingList>();

            XDocument doc = XDocument.Load(filePath);
            XElement root = doc.Root ?? new XElement("ShoppingLists");

            var lists = new List<ShoppingList>();

            foreach (var el in root.Elements("ShoppingList"))
            {
                var sl = new ShoppingList { Name = el.Element("Name")?.Value ?? "" };

                foreach (var cEl in el.Element("Categories")?.Elements("Category") ?? Enumerable.Empty<XElement>())
                {
                    var c = new Category
                    {
                        Name = cEl.Element("Name")?.Value ?? "",
                        ParentList = sl
                    };

                    foreach (var pEl in cEl.Element("Products")?.Elements("Product") ?? Enumerable.Empty<XElement>())
                    {
                        c.Products.Add(new Product
                        {
                            Name = pEl.Element("Name")?.Value ?? "",
                            Quantity = pEl.Element("Quantity")?.Value ?? "",
                            Unit = pEl.Element("Unit")?.Value ?? "",
                            Optional = pEl.Element("Optional")?.Value ?? "",
                            Store = pEl.Element("Store")?.Value ?? ""
                        });
                    }

                    sl.Categories.Add(c);
                }

                lists.Add(sl);
            }
            
            return lists;
        }
        public void Delete(string name)
        {
            string filePath = Path.Combine(FileSystem.AppDataDirectory, "shoppinglists.xml");
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);
            XDocument doc = XDocument.Load(filePath);
            XElement root = doc.Root ?? throw new FileNotFoundException(filePath);
            var listEl = root.Elements("ShoppingList")
                             .FirstOrDefault(x => x.Element("Name")?.Value == name);
            if (listEl != null)
            {
                listEl.Remove();
                doc.Save(filePath);
            }
        }
    }
}
