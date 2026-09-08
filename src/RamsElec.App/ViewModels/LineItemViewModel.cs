using CommunityToolkit.Mvvm.ComponentModel;
using RamsElec.Shared.DTOs;

namespace RamsElec.App.ViewModels;

public partial class LineItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _quantity = "1";

    [ObservableProperty]
    private string _unitPrice = "0.00";

    [ObservableProperty]
    private string _category = "service";

    [ObservableProperty]
    private int _sortOrder;

    public decimal ParsedQuantity
        => decimal.TryParse(Quantity, out var q) ? q : 0;

    public decimal ParsedUnitPrice
        => decimal.TryParse(UnitPrice, out var p) ? p : 0;

    public decimal LineTotal => ParsedQuantity * ParsedUnitPrice;

    public CreateLineItemDto ToDto() => new()
    {
        Description = Description,
        Quantity = ParsedQuantity,
        UnitPrice = ParsedUnitPrice,
        Category = Category,
        SortOrder = SortOrder
    };

    public static LineItemViewModel FromDto(CreateLineItemDto dto) => new()
    {
        Description = dto.Description,
        Quantity = dto.Quantity.ToString(),
        UnitPrice = dto.UnitPrice.ToString("0.00"),
        Category = dto.Category,
        SortOrder = dto.SortOrder
    };
}
