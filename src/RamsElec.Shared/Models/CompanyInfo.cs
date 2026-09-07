namespace RamsElec.Shared.Models;

public class CompanyInfo
{
    public string Name { get; set; } = "RAMS@ELEC (PTY) LTD";
    public string Registration { get; set; } = "2017/525813/07";
    public string TaxNumber { get; set; } = "9486744189";
    public string Address { get; set; } = "Stand No: A276B, Mogaladi Park, Paledi, 0727";
    public string Phone { get; set; } = "071 101 8493";
    public string Email { get; set; } = "invoices@ramsatelec.co.za";
    public string BankName { get; set; } = "FNB";
    public string AccountName { get; set; } = "RAMS@ELEC (PTY) LTD";
    public string AccountNumber { get; set; } = "62816356796";
    public string BranchCode { get; set; } = "210805";
    public bool IsVatRegistered { get; set; } = false;
    public string? VatNumber { get; set; }
    public decimal VatRate { get; set; } = 0.15m;
    public string? LogoPath { get; set; }
}
