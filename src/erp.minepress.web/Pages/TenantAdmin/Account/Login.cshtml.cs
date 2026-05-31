using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using erp.minepress.infrastructure.ErrorLogging;
using erp.minepress.persistence.Context;
using erp.minepress.web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace erp.minepress.web.Pages.TenantAdmin.Account;

public class LoginModel : PageModel
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ISystemErrorLogger _systemErrorLogger;

    public LoginModel(ApplicationDbContext dbContext, ISystemErrorLogger systemErrorLogger)
    {
        _dbContext = dbContext;
        _systemErrorLogger = systemErrorLogger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? ErrorMessage { get; set; }

    public string ReturnUrl { get; set; } = "/TenantAdmin";

    public void OnGet(string? returnUrl = null)
    {
        if (HttpContext.Session.IsTenantAdminAuthenticated())
        {
            Response.Redirect(returnUrl ?? "/TenantAdmin");
            return;
        }

        ReturnUrl = returnUrl ?? "/TenantAdmin";
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? "/TenantAdmin";

        if (!ModelState.IsValid)
            return Page();

        var user = await _dbContext.MstUsers.FirstOrDefaultAsync(x => x.Usercode == Input.Username);
        if (user == null || user.Isactive != true || user.Issystemadmin != true)
        {
            ErrorMessage = "Invalid tenant admin credentials.";
            return Page();
        }

        if (!VerifyPassword(Input.Password, user.Passwordhash))
        {
            ErrorMessage = "Invalid tenant admin credentials.";
            return Page();
        }

        var session = new TenantAdminSessionData
        {
            UserId = user.Userid,
            UserCode = user.Usercode,
            Name = user.Name,
            IsSystemAdmin = user.Issystemadmin ?? false,
            LoginAt = DateTime.UtcNow
        };

        HttpContext.Session.SetTenantAdmin(session);
        return LocalRedirect(ReturnUrl);
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(storedHash))
            return false;

        if (password == storedHash)
            return true;

        var sha256Hash = ComputeSha256Hash(password);
        return string.Equals(sha256Hash, storedHash, StringComparison.OrdinalIgnoreCase);
    }

    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        var sb = new StringBuilder();
        foreach (var b in bytes)
            sb.Append(b.ToString("x2"));

        return sb.ToString();
    }

    public sealed class InputModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
