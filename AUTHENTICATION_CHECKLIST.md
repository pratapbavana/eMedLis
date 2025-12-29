# ✅ Authentication Implementation Checklist

## QUICK START - Do This TODAY

### STEP 1: Protect HomeController (5 min)

- [ ] Open `Controllers/HomeController.cs`
- [ ] Add `[Authorize]` above `public class HomeController`
- [ ] Save

```csharp
[Authorize]  // ADD THIS
public class HomeController : Controller
{
    // ...
}
```

---

### STEP 2: Protect All Other Controllers (10 min)

Add `[Authorize]` above each controller class:

- [ ] `Controllers/SampleCollectionController.cs`
- [ ] `Controllers/PatientBillingController.cs`
- [ ] `Controllers/DuePaymentController.cs`
- [ ] `Controllers/ManageController.cs`
- [ ] `Controllers/DepartmentController.cs`
- [ ] `Controllers/SubDepartmentController.cs`
- [ ] `Controllers/InvestigationController.cs`
- [ ] `Controllers/LookupController.cs`

**Tip:** Copy from one, paste to others

---

### STEP 3: Fix AccountController (2 min)

- [ ] Open `Controllers/AccountController.cs`
- [ ] **REMOVE** `[Authorize]` above class
- [ ] Keep individual `[AllowAnonymous]` on methods
- [ ] Save

---

### STEP 4: Update RouteConfig (2 min)

- [ ] Open `App_Start/RouteConfig.cs`
- [ ] Change line:
  - FROM: `controller = "Home"`
  - TO: `controller = "Account"`
- [ ] Change line:
  - FROM: `action = "Index"`
  - TO: `action = "Login"`
- [ ] Save

---

### STEP 5: Update Web.config (5 min)

- [ ] Open `Web.config`
- [ ] Find `<system.web>` section
- [ ] After `<authentication mode="None" />` ADD:

```xml
<authorization>
  <deny users="?" />
</authorization>
```

- [ ] Find `<system.webServer>` section
- [ ] Add:

```xml
<modules>
  <remove name="FormsAuthentication" />
</modules>
<httpErrors errorMode="DetailedLocalOnly">
  <error statusCode="401" path="/Account/Login" />
</httpErrors>
```

- [ ] Save

---

### STEP 6: Add Logout Button (5 min)

- [ ] Open `Views/Shared/_Layout.cshtml`
- [ ] Find navbar/header section
- [ ] Add:

```html
@if (Request.IsAuthenticated)
{
    <ul class="nav navbar-nav navbar-right">
        <li>
            <span class="navbar-text">Hello, @User.Identity.Name!</span>
        </li>
        <li>
            @using (Html.BeginForm("LogOff", "Account", FormMethod.Post, new { id = "logoutForm" }))
            {
                @Html.AntiForgeryToken()
                <a href="#" onclick="document.getElementById('logoutForm').submit();return false;">Log off</a>
            }
        </li>
    </ul>
}
```

- [ ] Save

---

### STEP 7: Verify Login View (2 min)

- [ ] Check `Views/Account/Login.cshtml` exists
- [ ] If NOT, create it (see AUTHENTICATION_IMPLEMENTATION.md)
- [ ] Save

---

### STEP 8: Compile & Test (10 min)

- [ ] Build → Clean Solution
- [ ] Build → Rebuild Solution
- [ ] ✅ No compilation errors
- [ ] Press F5 to run
- [ ] Should go to login page (NOT home page)
- [ ] ✅ STEP 1 PASS

---

## TESTING

### Test 1: Login Page Shows
- [ ] Press F5
- [ ] App starts at login page
- [ ] ✅ PASS

### Test 2: Cannot Access Home
- [ ] Try to go to /Home/Index
- [ ] Redirects to /Account/Login
- [ ] ✅ PASS

### Test 3: Create User
- [ ] Click "Register as a new user"
- [ ] Email: `test@lab.com`
- [ ] Password: `Test@123456`
- [ ] Submit
- [ ] Should be logged in
- [ ] ✅ PASS

### Test 4: Access Protected Page
- [ ] Go to /Home/Index
- [ ] Should load successfully
- [ ] ✅ PASS

### Test 5: Logout Works
- [ ] Click "Log off"
- [ ] Should go to login page
- [ ] Try to access any page
- [ ] Redirects to login
- [ ] ✅ PASS

### Test 6: Other Controllers Protected
- [ ] Try to access /SampleCollection/Index
- [ ] Not logged in? Redirect to login
- [ ] Logged in? Should load
- [ ] ✅ PASS

---

## ✅ COMPLETE WHEN ALL TESTS PASS

**Total Time:** ~1 hour

**Files Modified:** 8

**Result:** 
- ✅ Every page requires login
- ✅ Unauthenticated users redirected to login
- ✅ Users can register and login
- ✅ Users can logout
- ✅ All controllers protected

---

## NEXT STEP

Once complete: Plan WEEKS 2-4 improvements

---

## REFERENCE

See **AUTHENTICATION_IMPLEMENTATION.md** for:
- Detailed explanations
- Code examples
- Troubleshooting
- Common issues
