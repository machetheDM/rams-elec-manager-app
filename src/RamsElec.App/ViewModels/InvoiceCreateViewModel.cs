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
    public ObservableCollection<CreateLineItemDto> LineItems { get; } = [];

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
        LineItems.Add(new CreateLineItemDto
        {
            Description = "",
            Quantity = 1,
            UnitPrice = 0,
            Category = "service",
            SortOrder = LineItems.Count
        });
    }

    [RelayCommand]
    private void RemoveLineItem(CreateLineItemDto item)
    {
        LineItems.Remove(item);
        RecalculateTotal();
    }

    public void RecalculateTotal()
    {
        Total = LineItems.Sum(li => li.Quantity * li.UnitPrice);
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

        IsBusy = true;
        try
        {
            var dto = new CreateInvoiceDto
            {
                CustomerId = SelectedCustomer.Id,
                JobId = SelectedJob?.Id,
                DueDate = DueDate,
                Notes = Notes,
                LineItems = LineItems.ToList()
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
        }
    }
}
