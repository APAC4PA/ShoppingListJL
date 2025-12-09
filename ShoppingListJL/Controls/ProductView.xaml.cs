using System.Windows.Input;
using ShoppingListJL.Models;
using Microsoft.Maui.Controls;

namespace ShoppingListJL.Controls;

public partial class ProductView : ContentView
{
	public ProductView()
	{
		InitializeComponent();
	}

	public static readonly BindableProperty ProductProperty =
		BindableProperty.Create(nameof(Product), typeof(Product), typeof(ProductView), default(Product));

	public static readonly BindableProperty CheckCommandProperty =
		BindableProperty.Create(nameof(CheckCommand), typeof(ICommand), typeof(ProductView), default(ICommand));

	public ICommand CheckCommand
	{
		get => (ICommand)GetValue(CheckCommandProperty);
		set => SetValue(CheckCommandProperty, value);
	}

	public static readonly BindableProperty DeleteCommandProperty =
		BindableProperty.Create(nameof(DeleteCommand), typeof(ICommand), typeof(ProductView), default(ICommand));

	public ICommand DeleteCommand
	{
		get => (ICommand)GetValue(DeleteCommandProperty);
		set => SetValue(DeleteCommandProperty, value);
	}

	public static readonly BindableProperty IncrementQuantityCommandProperty =
		BindableProperty.Create(nameof(IncrementQuantityCommand), typeof(ICommand), typeof(ProductView));

	public ICommand IncrementQuantityCommand
	{
		get => (ICommand)GetValue(IncrementQuantityCommandProperty);
		set => SetValue(IncrementQuantityCommandProperty, value);
	}

	public static readonly BindableProperty DecrementQuantityCommandProperty =
		BindableProperty.Create(nameof(DecrementQuantityCommand), typeof(ICommand), typeof(ProductView));

	public ICommand DecrementQuantityCommand
    {
		get => (ICommand)GetValue(DecrementQuantityCommandProperty);
		set => SetValue(DecrementQuantityCommandProperty, value);
	}
    public static readonly BindableProperty OptionalItemCommandProperty =
        BindableProperty.Create( // Tworzy w³aœciwoœæ wi¹zaln¹
            nameof(OptionalItemCommand), // Nazwa w³aœciwoœci
            typeof(ICommand), // Typ w³aœciwoœci (komenda)
            typeof(ProductView), // W³aœciciel w³aœciwoœci (ta kontrolka)
            default(ICommand)); // Wartoœæ domyœlna (null)

    public ICommand OptionalItemCommand
    {
        get => (ICommand)GetValue(OptionalItemCommandProperty); // Pobiera komendê z OptionalItemCommandProperty(ProductView)
        set => SetValue(OptionalItemCommandProperty, value); // Ustawia komendê w OptionalItemCommandProperty(ProductView)
    }
}