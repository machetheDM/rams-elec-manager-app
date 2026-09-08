using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RamsElec.App.Services;
using RamsElec.Shared.DTOs;
using RamsElec.Shared.Models;

namespace RamsElec.App.ViewModels;

public partial class QuoteCreateViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly LocalDatabase _localDb;

    public QuoteCreateViewModel(ApiClient apiClient, LocalDatabase localDb)
    {
        _apiClient = apiClient;
        _localDb = localDb;
    }

    public ObservableCollection<Customer> Customers { get; } = [];
    public ObservableCollection<LineItemViewModel> LineItems { get; } = [];

    [ObservableProperty]
    private Customer? _selectedCustomer;

    [ObservableProperty]
    private DateTime _expiryDate = DateTime.Today.AddDays(14);

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private string _depositAmount = string.Empty;

    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private bool _isBusy;

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        var customers = await _localDb.GetCustomersAsync();
        Customers.Clear();
        foreach (var c in customers) Customers.Add(c);
    }

    [RelayCommand]
    private void AddLineItem()
    {
        var item = new LineItemViewModel
        {
            Quantity = "1",
            UnitPrice = "0.00",
            Category = "service",
            SortOrder = LineItems.Count
        };
        item.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName is nameof(LineItemViewModel.Quantity) or nameof(LineItemViewModel.UnitPrice))
                RecalculateTotal();
        };
        LineItems.Add(item);
        RecalculateTotal();
    }

    [RelayCommand]
    private void RemoveLineItem(LineItemViewModel item)
    {
        LineItems.Remove(item);
        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        Total = LineItems.Sum(li => li.ParsedQuantity * li.ParsedUnitPrice);
    }

    [RelayCommand]
    private async Task CreateQuoteAsync()
    {
        if (SelectedCustomer == null)
        {
            await Shell.Current.DisplayAlert("Error", "Please select a customer", "OK");
            return;
        }

        if (LineItems.Count == 0)
        {
            await Shell.Current.DisplayAlert("Error", "Please add at least one line item", "OK");
            return;
        }

        decimal? deposit = null;
        if (!string.IsNullOrWhiteSpace(DepositAmount) && decimal.TryParse(DepositAmount, out var d))
            deposit = d;

        IsBusy = true;
        try
        {
            var dto = new CreateQuoteDto
            {
                CustomerId = SelectedCustomer.Id,
                ExpiryDate = ExpiryDate,
                Notes = Notes,
                DepositAmount = deposit,
                LineItems = LineItems.Select(li => new CreateQuoteLineItemDto
                {
                    Description = li.Description,
                    Quantity = li.ParsedQuantity,
                    UnitPrice = li.ParsedUnitPrice,
                    Category = li.Category,
                    SortOrder = li.SortOrder
                }).ToList()
            };

            var result = await _apiClient.CreateQuoteAsync(dto);
            if (result != null)
            {
                await Shell.Current.DisplayAlert("Success", $"Quote {result.QuoteNumber} created", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Failed to create quote", "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
