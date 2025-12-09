using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShoppingListJL.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using System.IO;
using System.Linq;

namespace ShoppingListJL.ViewModels
{
    internal class ShoppingListsViewModel : ObservableObject
    {
        public ICommand SelectList { get; }
        public ICommand NewShoppingList { get; }
        public ICommand DeleteListCommand { get; }
        public ICommand ExportSelectedListCommand { get; }
        public ICommand ImportListsCommand { get; }

        public ObservableCollection<Models.ShoppingList> AllShoppingLists { get; }

        private Models.ShoppingList? _selectedList;
        public Models.ShoppingList? SelectedList
        {
            get => _selectedList;
            set => SetProperty(ref _selectedList, value);
        }

        public ShoppingListsViewModel()
        {
            AllShoppingLists = new ObservableCollection<Models.ShoppingList>(Models.ShoppingList.LoadAll());
            DeleteListCommand = new AsyncRelayCommand<ShoppingList?>(DeleteList);
            SelectList = new AsyncRelayCommand<ShoppingList?>(SelectListAsync);
            NewShoppingList = new AsyncRelayCommand(AddNewShoppingList);

            ExportSelectedListCommand = new AsyncRelayCommand(ExportSelectedList);
            ImportListsCommand = new AsyncRelayCommand(ImportLists);
        }

        private async Task DeleteList(ShoppingList? list)
        {
            if (list == null) return;
            var page = App.Current?.MainPage;
            if (page == null) return;

            bool confirm = await page.DisplayAlert(
                "Usuñ listê",
                $"Czy na pewno chcesz usun¹æ listê '{list.Name}'?",
                "Tak",
                "Nie");

            if (!confirm) return;
            list.Delete(list.Name);
            AllShoppingLists.Remove(list);
        }

        private async Task SelectListAsync(ShoppingList? list)
        {
            if (list == null) return;
            await Shell.Current.GoToAsync($"{nameof(Views.CategoryPage)}?load={list.Name}");
        }

        private async Task AddNewShoppingList()
        {
            var page = App.Current?.MainPage;
            if (page == null) return;

            string? listName = await page.DisplayPromptAsync(
                "New Shopping List", "Enter the name of the new shopping list:");
            if (string.IsNullOrWhiteSpace(listName))
                return;

            var newList = new Models.ShoppingList { Name = listName };
            newList.Save();

            AllShoppingLists.Add(newList);
        }

        private async Task ExportSelectedList()
        {
            var page = App.Current?.MainPage;
            if (page == null) return;

            if (SelectedList == null)
            {
                await page.DisplayAlert("Export", "Wybierz listê do eksportu.", "OK");
                return;
            }

            try
            {
                var list = SelectedList;
                var doc = new XDocument(
                    new XElement("ShoppingLists",
                        new XElement("ShoppingList",
                            new XElement("Name", list.Name),
                            new XElement("Categories",
                                from c in list.Categories
                                select new XElement("Category",
                                    new XElement("Name", c.Name),
                                    new XElement("Products",
                                        from p in c.Products
                                        select new XElement("Product",
                                            new XElement("Name", p.Name),
                                            new XElement("Quantity", p.Quantity),
                                            new XElement("Unit", p.Unit),
                                            new XElement("Optional", p.Optional)
                                        )
                                    )
                                )
                            )
                        )
                    )
                );

                string tempPath = Path.Combine(FileSystem.CacheDirectory, $"{SanitizeFileName(list.Name)}.xml");
                doc.Save(tempPath);

                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = $"Export listy: {list.Name}",
                    File = new ShareFile(tempPath)
                });
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex);
                var page2 = App.Current?.MainPage;
                if (page2 != null)
                    await page2.DisplayAlert("Export error", ex.Message, "OK");
            }
        }

        private async Task ImportLists()
        {
            var page = App.Current?.MainPage;
            if (page == null) return;

            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Wybierz plik XML z listami"
                });

                if (result == null) return;

                using var stream = await result.OpenReadAsync();
                var importedDoc = XDocument.Load(stream);
                var root = importedDoc.Root;
                IEnumerable<XElement> importedElements;

                if (root == null)
                {
                    importedElements = Enumerable.Empty<XElement>();
                }
                else if (root.Name.LocalName == "ShoppingList")
                {
                    importedElements = new[] { root };
                }
                else
                {
                    importedElements = root.Elements("ShoppingList");
                }

                int count = 0;

                foreach (var el in importedElements)
                {
                    var sl = new Models.ShoppingList
                    {
                        Name = el.Element("Name")?.Value ?? string.Empty
                    };

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
                                Optional = bool.Parse(pEl.Element("Optional")?.Value),
                                Store = pEl.Element("Store")?.Value ?? "",
                                Bought = bool.Parse(pEl.Element("Bought")?.Value)
                            });
                        }

                        sl.Categories.Add(c);
                    }

                    sl.Save();
                    count++;
                }

                AllShoppingLists.Clear();
                foreach (var s in Models.ShoppingList.LoadAll())
                    AllShoppingLists.Add(s);

                await page.DisplayAlert("Import", $"Zaimportowano {count} list(ê/y).", "OK");
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex);
                var page2 = App.Current?.MainPage;
                if (page2 != null)
                    await page2.DisplayAlert("Import error", ex.Message, "OK");
            }
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}