using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RamsElec.Api.Services;
using RamsElec.Shared.Models;

namespace RamsElec.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompanyInfoController : ControllerBase
{
    private readonly CompanyInfoService _companyInfoService;

    public CompanyInfoController(CompanyInfoService companyInfoService)
    {
        _companyInfoService = companyInfoService;
    }

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _companyInfoService.GetOrCreateAsync());

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] CompanyInfo info) =>
        Ok(await _companyInfoService.UpdateAsync(info));
}
