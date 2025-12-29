# eMedLis - Code Snippets Reference
## Copy-Paste Ready Code for Each Step

---

## STEP 1: Web.config Credentials

### Replace connectionStrings section with:

```xml
<connectionStrings>
  <add name="DefaultConnection" 
       connectionString="Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=aspnet-eMedLis-dev;Integrated Security=true;Min Pool Size=5;Max Pool Size=100;" 
       providerName="System.Data.SqlClient" />
  
  <add name="emeddb" 
       connectionString="Data Source=.;Initial Catalog=eMedLis;Integrated Security=true;Min Pool Size=5;Max Pool Size=100;" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

---

## STEP 2: Web.config Security Headers

### Add this inside <system.webServer> (as first child):

```xml
<httpProtocol>
  <customHeaders>
    <add name="X-Content-Type-Options" value="nosniff" />
    <add name="X-Frame-Options" value="DENY" />
    <add name="X-XSS-Protection" value="1; mode=block" />
    <add name="Strict-Transport-Security" value="max-age=31536000; includeSubDomains" />
    <add name="Referrer-Policy" value="strict-origin-when-cross-origin" />
  </customHeaders>
</httpProtocol>
```

---

## STEP 2: Web.Release.config Transforms

### Add at end (before </configuration>):

```xml
<system.web>
  <compilation debug="false" targetFramework="4.8" 
               xdt:Transform="SetAttributes" />
  <customErrors mode="On"
                xdt:Transform="SetAttributes" />
</system.web>

<connectionStrings>
  <add name="emeddb" 
       connectionString="Data Source=YOUR_SERVER;Initial Catalog=eMedLis;User Id=__USERNAME__;Password=__PASSWORD__;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;"
       xdt:Transform="SetAttributes" 
       xdt:Locator="Match(name)" />
</connectionStrings>
```

---

## STEP 3: Models/AuditLog.cs

### Create new file and paste:

```csharp
using System;
using System.ComponentModel.DataAnnotations;

namespace eMedLis.Models
{
    public class AuditLog
    {
        [Key]
        public int AuditLogId { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        [StringLength(128)]
        public string UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; }

        [Required]
        [StringLength(50)]
        public string EntityType { get; set; }

        [Required]
        [StringLength(50)]
        public string EntityId { get; set; }

        public string OldValue { get; set; }
        public string NewValue { get; set; }

        [StringLength(50)]
        public string IPAddress { get; set; }

        [StringLength(500)]
        public string Reason { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        public string ErrorDetails { get; set; }
    }

    public class ResultAmendment
    {
        [Key]
        public int AmendmentId { get; set; }

        [Required]
        public int InvestigationId { get; set; }

        [Required]
        public DateTime AmendmentDate { get; set; }

        [Required]
        [StringLength(128)]
        public string AmendedBy { get; set; }

        [Required]
        [StringLength(1000)]
        public string Reason { get; set; }

        [Required]
        public string OldResult { get; set; }

        [Required]
        public string NewResult { get; set; }

        public DateTime? ApprovedDate { get; set; }

        [StringLength(128)]
        public string ApprovedBy { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        public string ApprovalComments { get; set; }
    }
}
```

---

## STEP 3: Add to IdentityModels.cs

### Find ApplicationDbContext and add these DbSets:

```csharp
public DbSet<AuditLog> AuditLogs { get; set; }
public DbSet<ResultAmendment> ResultAmendments { get; set; }
```

---

## STEP 3: Services/AuditService.cs

### Create new file and paste:

```csharp
using eMedLis.Models;
using System;
using System.Threading.Tasks;
using System.Web;

namespace eMedLis.Services
{
    public interface IAuditService
    {
        Task LogAction(string userId, string action, string entityType, string entityId,
                      string oldValue = null, string newValue = null, string reason = null);
        Task LogError(string userId, string action, string entityType, string entityId,
                     string errorDetails, string reason = null);
    }

    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService()
        {
            _context = new ApplicationDbContext();
        }

        public async Task LogAction(string userId, string action, string entityType, string entityId,
                                   string oldValue = null, string newValue = null, string reason = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    Timestamp = DateTime.UtcNow,
                    UserId = userId ?? "Unknown",
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    OldValue = oldValue,
                    NewValue = newValue,
                    IPAddress = HttpContext.Current?.Request.UserHostAddress ?? "Unknown",
                    Reason = reason ?? "Standard operation",
                    Status = "Success"
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AuditService Error: {ex.Message}");
            }
        }

        public async Task LogError(string userId, string action, string entityType, string entityId,
                                  string errorDetails, string reason = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    Timestamp = DateTime.UtcNow,
                    UserId = userId ?? "Unknown",
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    IPAddress = HttpContext.Current?.Request.UserHostAddress ?? "Unknown",
                    Reason = reason ?? "Error occurred",
                    Status = "Failure",
                    ErrorDetails = errorDetails
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch { }
        }
    }
}
```

---

## STEP 3: Update SampleCollectionController

### Add using statement:

```csharp
using eMedLis.Services;
```

### Add field in class:

```csharp
private readonly AuditService _auditService = new AuditService();
```

### Replace SaveCollection method:

```csharp
[HttpPost]
public JsonResult SaveCollection(SampleCollectionModel sampleCollection, List<SampleCollectionDetail> sampleDetails)
{
    try
    {
        var dbResult = _db.SaveSampleCollection(sampleCollection, sampleDetails);

        if (dbResult.Success)
        {
            _db.CalculateAndUpdateCollectionStatus(dbResult.SampleCollectionId);

            var userId = User.Identity.GetUserId() ?? "Unknown";
            _auditService.LogAction(
                userId,
                "SAMPLE_COLLECTION_CREATED",
                "SampleCollection",
                dbResult.SampleCollectionId.ToString(),
                null,
                $"Barcode: {dbResult.CollectionBarcode}, Items: {sampleDetails.Count}",
                $"Sample collection created for bill {sampleCollection.BillSummaryId}"
            ).Wait();

            return Json(new
            {
                success = true,
                message = "Sample collection saved successfully",
                sampleCollectionId = dbResult.SampleCollectionId,
                collectionBarcode = dbResult.CollectionBarcode
            });
        }

        return Json(new { success = false, message = dbResult.Message });
    }
    catch (Exception ex)
    {
        var userId = User.Identity.GetUserId() ?? "Unknown";
        _auditService.LogError(
            userId,
            "SAMPLE_COLLECTION_SAVE_ERROR",
            "SampleCollection",
            sampleCollection.BillSummaryId.ToString(),
            ex.Message,
            "Failed to save sample collection"
        ).Wait();

        return Json(new { success = false, message = ex.Message });
    }
}
```

---

## STEP 4: Services/ValidationService.cs

### Create new file and paste:

```csharp
using System;
using System.Text.RegularExpressions;

namespace eMedLis.Services
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }

        public static ValidationResult Success() => new ValidationResult { IsValid = true };
        public static ValidationResult Failure(string message) =>
            new ValidationResult { IsValid = false, ErrorMessage = message };
    }

    public class ValidationService
    {
        private const string PatientIdPattern = @"^[A-Z0-9\-]{3,20}$";
        private const string SampleIdPattern = @"^[A-Z0-9\-]{5,30}$";
        private const string NumericPattern = @"^[0-9]+(\.[0-9]{1,4})?$";
        private const string EmailPattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";

        public static ValidationResult ValidatePatientId(string patientId)
        {
            if (string.IsNullOrWhiteSpace(patientId))
                return ValidationResult.Failure("Patient ID is required");
            if (!Regex.IsMatch(patientId, PatientIdPattern))
                return ValidationResult.Failure("Invalid Patient ID format");
            return ValidationResult.Success();
        }

        public static ValidationResult ValidateSampleId(string sampleId)
        {
            if (string.IsNullOrWhiteSpace(sampleId))
                return ValidationResult.Failure("Sample ID is required");
            if (!Regex.IsMatch(sampleId, SampleIdPattern))
                return ValidationResult.Failure("Invalid Sample ID format");
            return ValidationResult.Success();
        }

        public static ValidationResult ValidateNumeric(string value, string fieldName, decimal? minValue = null, decimal? maxValue = null)
        {
            if (string.IsNullOrWhiteSpace(value))
                return ValidationResult.Failure($"{fieldName} is required");
            if (!Regex.IsMatch(value, NumericPattern))
                return ValidationResult.Failure($"{fieldName} must be a valid number");
            if (decimal.TryParse(value, out var numValue))
            {
                if (minValue.HasValue && numValue < minValue)
                    return ValidationResult.Failure($"{fieldName} cannot be less than {minValue}");
                if (maxValue.HasValue && numValue > maxValue)
                    return ValidationResult.Failure($"{fieldName} cannot be more than {maxValue}");
            }
            return ValidationResult.Success();
        }

        public static ValidationResult ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return ValidationResult.Failure("Email is required");
            if (!Regex.IsMatch(email, EmailPattern))
                return ValidationResult.Failure("Invalid email format");
            return ValidationResult.Success();
        }

        public static string EscapeHtml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            return System.Web.HttpUtility.HtmlEncode(input);
        }

        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            input = Regex.Replace(input, "<[^>]*>", "");
            input = input.Replace(";", "").Replace("'", "''");
            return input.Trim();
        }
    }
}
```

---

## STEP 4: Update GetCollectionData Method

### Replace entire GetCollectionData method:

```csharp
[HttpGet]
public JsonResult GetCollectionData(int? billId)
{
    try
    {
        if (!billId.HasValue || billId <= 0 || billId > 99999999)
        {
            return Json(new { success = false, message = "Invalid Bill ID" }, JsonRequestBehavior.AllowGet);
        }

        var viewModel = _db.GetBillForCollection(billId.Value);

        if (viewModel == null || viewModel.BillSummary == null)
        {
            return Json(new { success = false, message = "Bill not found" }, JsonRequestBehavior.AllowGet);
        }

        return Json(new
        {
            success = true,
            data = new
            {
                billSummaryId = viewModel.BillSummary.BillSummaryId,
                billNo = ValidationService.EscapeHtml(viewModel.BillSummary.BillNo),
                billDate = viewModel.BillSummary.BillDate.ToString("dd/MM/yyyy"),
                netAmount = viewModel.BillSummary.NetAmount.ToString("F2"),
                collectionBarcode = "",
                sampleCollectionId = 0,
                patientInfo = new
                {
                    patientInfoId = viewModel.PatientInfo.PatientInfoId,
                    patName = ValidationService.EscapeHtml(viewModel.PatientInfo.PatName),
                    uhid = ValidationService.EscapeHtml(viewModel.PatientInfo.UHID),
                    mobileNo = ValidationService.EscapeHtml(viewModel.PatientInfo.MobileNo),
                    age = viewModel.PatientInfo.Age,
                    gender = viewModel.PatientInfo.Gender
                },
                billDetails = viewModel.BillDetails.Select(d => new
                {
                    invId = d.InvId,
                    invName = ValidationService.EscapeHtml(d.InvName),
                    rate = d.Rate.ToString("F2"),
                    specimenType = ValidationService.EscapeHtml(d.SpecimenType ?? "Serum"),
                    fastingRequired = d.FastingRequired
                }).ToList()
            }
        }, JsonRequestBehavior.AllowGet);
    }
    catch (Exception ex)
    {
        var userId = User.Identity.GetUserId() ?? "Unknown";
        _auditService.LogError(userId, "GET_COLLECTION_DATA_ERROR", "SampleCollection",
                              billId.ToString(), ex.Message).Wait();
        return Json(new { success = false, message = "An error occurred." }, JsonRequestBehavior.AllowGet);
    }
}
```

---

## STEP 5: Web.config Custom Errors

### Add to <system.web>:

```xml
<customErrors mode="RemoteOnly" defaultRedirect="~/Error/ServerError">
  <error statusCode="404" redirect="~/Error/NotFound" />
  <error statusCode="500" redirect="~/Error/ServerError" />
</customErrors>
```

---

## STEP 5: Controllers/ErrorController.cs

### Create new file and paste:

```csharp
using System.Web.Mvc;

namespace eMedLis.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult NotFound()
        {
            return View();
        }

        public ActionResult ServerError()
        {
            return View();
        }
    }
}
```

---

## STEP 5: Views/Error/NotFound.cshtml

### Create and paste:

```html
@{
    ViewBag.Title = "Page Not Found";
    Layout = "~/Views/Shared/_Layout.cshtml";
}

<div class="container mt-5">
    <div class="row">
        <div class="col-md-6 offset-md-3 text-center">
            <h1>404 - Page Not Found</h1>
            <p>The page you're looking for doesn't exist.</p>
            <a href="@Url.Action("Index", "Home")" class="btn btn-primary">Go Home</a>
        </div>
    </div>
</div>
```

---

## STEP 5: Views/Error/ServerError.cshtml

### Create and paste:

```html
@{
    ViewBag.Title = "Server Error";
    Layout = "~/Views/Shared/_Layout.cshtml";
}

<div class="container mt-5">
    <div class="row">
        <div class="col-md-6 offset-md-3 text-center">
            <h1>500 - Server Error</h1>
            <p>An unexpected error occurred. Our team has been notified.</p>
            <a href="@Url.Action("Index", "Home")" class="btn btn-primary">Go Home</a>
        </div>
    </div>
</div>
```

---

## Package Manager Console Commands

```powershell
# Add migration
Add-Migration AddAuditLogging

# Update database
Update-Database

# If error occurs
Update-Database -Verbose
```

---

## SQL Verification Queries

```sql
-- Check if tables exist
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('AuditLogs', 'ResultAmendments')

-- View recent audit entries
SELECT TOP 10 * FROM AuditLogs ORDER BY Timestamp DESC

-- Check for errors in audit log
SELECT * FROM AuditLogs WHERE Status = 'Failure' ORDER BY Timestamp DESC
```

---

## Common Errors & Solutions

**Error:** "The name 'ValidationService' does not exist"  
**Solution:** Add `using eMedLis.Services;` at top

**Error:** "'User' does not contain a definition for 'Identity'"  
**Solution:** Add `using Microsoft.AspNet.Identity;`

**Error:** "Cannot convert async call to sync"  
**Solution:** Use `.Wait()` at end of async call

**Error:** "Invalid column name"  
**Solution:** Run `Update-Database` in Package Manager Console
