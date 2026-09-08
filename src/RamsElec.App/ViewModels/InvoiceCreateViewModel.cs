using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RamsElec.App.Services;
using RamsElec.Shared.DTOs;
using RamsElec.Shared.Models;

namespace RamsElec.App.ViewModels;

public partial class InvoiceCreateViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly LocalDatabase _localDb;

    public InvoiceCreateViewModel(ApiClient apiClient, LocalDatabase localDb)
    {
        _apiClient = apiClient;
        _localDb = localDb;
    }

    public ObservableCollection<Customer> Customers { get; } = [];
    public ObservableCollection<Job> CompletedJobs { get; } = [];
    public ObservableCollection<LineItemViewModel> LineItems { get; } = [];

    [ObservableProperty]
    private Customer? _selectedCustomer;

    [ObservableProperty]
    private Job? _selectedJob;

    [ObservableProperty]
    private DateTime _dueDate = DateTime.Today.AddDays(14);

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        var customers = await _localDb.GetCustomersAsync();
        Customers.Clear();
        foreach (var c in customers) Customers.Add(c);

        var jobs = await _localDb.GetCompletedJobsAsync();
        CompletedJobs.Clear();
        foreach (var j in jobs) CompletedJobs.Add(j);
    }

    [RelayCommand]
    private void AddLineItem()
    {
        var item = new LineItemViewModel
        {
            Description = "",
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
    private async Task CreateInvoiceAsync()
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

        if (LineItems.Any(li => li.ParsedQuantity <= 0 || li.ParsedUnitPrice < 0))
        {
            await Shell.Current.DisplayAlert("Error", "Please enter valid quantities and prices", "OK");
            return;
        }

        IsBusy = true;
        StatusMessage = "Creating invoice...";
        try
        {
            var dto = new CreateInvoiceDto
            {
                CustomerId = SelectedCustomer.Id,
                JobId = SelectedJob?.Id,
                DueDate = DueDate,
                Notes = Notes,
                LineItems = LineItems.Select(li => li.ToDto()).ToList()
            };

            var result = await _apiClient.CreateInvoiceAsync(dto);
            if (result != null)
            {
                await Shell.Current.DisplayAlert("Success",
                    $"Invoice {result.InvoiceNumber} created", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error",
                    "Failed to create invoice. Check connection.", "OK");
            }
        }
        finally
        {
            IsBusy = false;
            StatusMessage = string.Empty;
        }
    }
}
