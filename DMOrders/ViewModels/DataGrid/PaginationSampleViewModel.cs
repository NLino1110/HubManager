using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace DMOrders.ViewModels.DataGrid;

public partial class PaginationSampleViewModel : ObservableObject
{
    public ObservableCollection<Product> Products { get; } = new();

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private int currentPage;

    [ObservableProperty]
    private int totalPages;

    public const int limit = 10;

    public PaginationSampleViewModel()
    {
        currentPage = 1;
        LoadPageAsync(currentPage);
    }

    [RelayCommand]
    private void GoNext()
    {
        if (currentPage < currentPage)
        {
            currentPage++;
            LoadPageAsync(currentPage);
        }
    }

    [RelayCommand]
    private void GoPrevious()
    {
        if (currentPage > 1)
        {
            currentPage--;
            LoadPageAsync(currentPage);
        }
    }

    [RelayCommand]
    private void SetPage(int page)
    {
        if (page >= 1 && page <= totalPages)
        {
            currentPage = page;
            LoadPageAsync(currentPage);
        }
    }

    private async void LoadPageAsync(int page)
    {
        try
        {
            isBusy = true;

            var response = await GetProductsAsync(limit, (page - 1) * limit);

            totalPages = (int)Math.Ceiling((double)response.total / limit);

            Products.Clear();
            foreach (var product in response.products)
                Products.Add(product);
        }
        finally
        {
            isBusy = false;
        }
    }

    private async Task<ApiResponse> GetProductsAsync(int limit, int skip = 0)
    {
        using var client = new HttpClient();
        return await client.GetFromJsonAsync<ApiResponse>(
            $"https://dummyjson.com/products?limit={limit}&skip={skip}");
    }
}

public class ApiResponse
{
    public Product[] products { get; set; }
    public int total { get; set; }
    public int skip { get; set; }
    public int limit { get; set; }
}

public class Product
{
    public int id { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public float price { get; set; }
    public float discountPercentage { get; set; }
    public float rating { get; set; }
    public int stock { get; set; }
    public string brand { get; set; }
    public string category { get; set; }
    public string thumbnail { get; set; }
    public string[] images { get; set; }
}
