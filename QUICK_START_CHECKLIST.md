# ✅ eMedLis Implementation - Quick Start Checklist

## TODAY (Day 1) - Get Started Now!

### Morning Session (1-2 hours) - Configuration & Security

- [ ] **STEP 1.1** Edit Web.config - Remove passwords
  - [ ] Find `<connectionStrings>` section
  - [ ] Delete `password=torrent` line
  - [ ] Add `Integrated Security=true`
  - [ ] Add connection pooling parameters
  - [ ] Save file
  - ⏱️ Time: 15 minutes

- [ ] **STEP 1.2** Edit Web.Release.config - Production config
  - [ ] Add secure connection string transform
  - [ ] Use placeholders instead of real passwords
  - [ ] Save file
  - ⏱️ Time: 15 minutes

- [ ] **STEP 1.3** Verify changes
  - [ ] Search Web.config for "torrent" - should find NOTHING
  - [ ] Search for "Integrated Security" - should find it
  - [ ] ✅ STEP 1 COMPLETE
  - ⏱️ Time: 5 minutes

### Afternoon Session (1-2 hours) - Web Server Security

- [ ] **STEP 2.1** Keep debug="true" for now
  - [ ] Don't change the debug setting yet
  - [ ] We'll handle it in Web.Release.config
  - ⏱️ Time: 2 minutes

- [ ] **STEP 2.2** Add Security Headers to Web.config
  - [ ] Find `<system.webServer>` section
  - [ ] Add `<httpProtocol>` section with headers
  - [ ] Headers include: XSS, Clickjacking, MIME, HSTS, Referrer protection
  - [ ] Save file
  - ⏱️ Time: 10 minutes

- [ ] **STEP 2.3** Add Web.Release.config transforms
  - [ ] Disable debug for production
  - [ ] Enable custom error pages
  - [ ] Save file
  - ✅ STEP 2 COMPLETE
  - ⏱️ Time: 10 minutes

---

## DAY 2 (Morning) - Database Models (2-3 hours)

### Create Audit Logging System

- [ ] **STEP 3.1** Create Models/AuditLog.cs
  - [ ] Right-click Models folder → Add → New Item → Class
  - [ ] Name: AuditLog.cs
  - [ ] See CODE_SNIPPETS_REFERENCE.md for content
  - [ ] This creates two models: AuditLog + ResultAmendment
  - [ ] Save file
  - ⏱️ Time: 10 minutes

- [ ] **STEP 3.2** Update IdentityModels.cs (DbContext)
  - [ ] Find Models/IdentityModels.cs
  - [ ] Find ApplicationDbContext class
  - [ ] Add two new DbSet lines (see CODE_SNIPPETS_REFERENCE.md)
  - [ ] Save file
  - ⏱️ Time: 5 minutes

- [ ] **STEP 3.3** Create Services/AuditService.cs
  - [ ] Create Services folder if it doesn't exist
  - [ ] Right-click Services → Add → New Item → Class
  - [ ] Name: AuditService.cs
  - [ ] See CODE_SNIPPETS_REFERENCE.md for content
  - [ ] Save file
  - ⏱️ Time: 10 minutes

- [ ] **STEP 3.4** Create Database Migration
  - [ ] Tools → NuGet Package Manager → Package Manager Console
  - [ ] Paste: `Add-Migration AddAuditLogging`
  - [ ] Press Enter (wait for completion)
  - [ ] Paste: `Update-Database`
  - [ ] Press Enter (wait for completion)
  - [ ] ✅ You should see success message
  - ⏱️ Time: 10 minutes

- [ ] **STEP 3.5** Add Audit Logging to SampleCollectionController
  - [ ] Open Controllers/SampleCollectionController.cs
  - [ ] Add using: `using eMedLis.Services;`
  - [ ] Add field: `private readonly AuditService _auditService = new AuditService();`
  - [ ] Update SaveCollection method (see CODE_SNIPPETS_REFERENCE.md)
  - [ ] Save file
  - [ ] ✅ STEP 3 COMPLETE
  - ⏱️ Time: 15 minutes

---

## DAY 2 (Afternoon) - Input Validation (2-3 hours)

### Prevent SQL Injection & XSS

- [ ] **STEP 4.1** Create Services/ValidationService.cs
  - [ ] Right-click Services → Add → New Item → Class
  - [ ] Name: ValidationService.cs
  - [ ] See CODE_SNIPPETS_REFERENCE.md for content
  - [ ] This includes patterns for: Patient ID, Sample ID, Phone, Email, etc.
  - [ ] Save file
  - ⏱️ Time: 10 minutes

- [ ] **STEP 4.2** Update SampleCollectionController GetCollectionData method
  - [ ] Find GetCollectionData method in SampleCollectionController
  - [ ] Replace entire method (see CODE_SNIPPETS_REFERENCE.md)
  - [ ] Key changes:
    - [ ] Add range check: `if (billId > 99999999)`
    - [ ] Use `ValidationService.EscapeHtml()` on all strings
    - [ ] Don't expose error details to users
    - [ ] Log errors to audit system
  - [ ] Save file
  - [ ] ✅ STEP 4 COMPLETE
  - ⏱️ Time: 15 minutes

---

## DAY 3 (Morning) - Error Handling (1-2 hours)

### Friendly Error Pages

- [ ] **STEP 5.1** Update Web.config custom errors
  - [ ] Find `<system.web>` section
  - [ ] Add `<customErrors>` block for 404 and 500 errors
  - [ ] See CODE_SNIPPETS_REFERENCE.md for exact code
  - [ ] Save file
  - ⏱️ Time: 10 minutes

- [ ] **STEP 5.2** Create Controllers/ErrorController.cs
  - [ ] Right-click Controllers → Add → New Item → Class
  - [ ] Name: ErrorController.cs
  - [ ] See CODE_SNIPPETS_REFERENCE.md for content
  - [ ] Save file
  - ⏱️ Time: 5 minutes

- [ ] **STEP 5.3** Create Error Views
  - [ ] Create Views/Error folder if needed
  - [ ] Right-click Views/Error → Add → New Item → MVC View Page
  - [ ] Create NotFound.cshtml (see CODE_SNIPPETS_REFERENCE.md)
  - [ ] Create ServerError.cshtml (see CODE_SNIPPETS_REFERENCE.md)
  - [ ] Save both files
  - [ ] ✅ STEP 5 COMPLETE
  - ⏱️ Time: 10 minutes

---

## DAY 3 (Afternoon) - Compilation & Testing (1-2 hours)

### Build & Verify

- [ ] **Compile Solution**
  - [ ] Visual Studio → Build → Clean Solution
  - [ ] Visual Studio → Build → Rebuild Solution
  - [ ] ✅ Should compile with NO errors
  - [ ] ✅ Should have NO warnings (if possible)
  - ⏱️ Time: 5 minutes

- [ ] **Run Locally**
  - [ ] Press F5 to start debugging
  - [ ] Wait for application to load
  - [ ] Should see home page without errors
  - ⏱️ Time: 2 minutes

- [ ] **Test Sample Collection**
  - [ ] Navigate to Sample Collection page
  - [ ] Create a test sample collection
  - [ ] Should save successfully
  - [ ] Check browser console for errors (F12)
  - [ ] Should be no errors
  - ⏱️ Time: 5 minutes

- [ ] **Verify Database Changes**
  - [ ] Open SQL Server Management Studio
  - [ ] Connect to your database
  - [ ] Look for new tables:
    - [ ] `dbo.AuditLogs` - should exist
    - [ ] `dbo.ResultAmendments` - should exist
  - [ ] Right-click → View data
  - [ ] Should see your test audit entry
  - ⏱️ Time: 5 minutes

- [ ] **Verify No Credentials in Code**
  - [ ] Search solution for "torrent"
  - [ ] Should find NOTHING
  - [ ] Search for "sa" (SQL Admin)
  - [ ] Should find NOTHING
  - [ ] Search for "password="
  - [ ] Should find NOTHING in production config
  - ⏱️ Time: 3 minutes

---

## ✅ WEEK 1 COMPLETE!

### Summary of Changes

**Files Modified:**
- ✅ Web.config
- ✅ Web.Release.config
- ✅ Models/IdentityModels.cs
- ✅ Controllers/SampleCollectionController.cs

**Files Created:**
- ✅ Models/AuditLog.cs
- ✅ Services/AuditService.cs
- ✅ Services/ValidationService.cs
- ✅ Controllers/ErrorController.cs
- ✅ Views/Error/NotFound.cshtml
- ✅ Views/Error/ServerError.cshtml

**Database Changes:**
- ✅ Migration: AddAuditLogging
- ✅ New tables: AuditLogs, ResultAmendments

---

## Total Time Investment: ~10-12 hours

- Day 1 (Config & Security): 2-3 hours
- Day 2 (Models & Validation): 4-5 hours
- Day 3 (Error Handling & Testing): 2-3 hours

---

## Next: Follow STEP_BY_STEP_IMPLEMENTATION.md for detailed instructions
