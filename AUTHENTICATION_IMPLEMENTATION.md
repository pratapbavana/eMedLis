# eMedLis - Authentication Implementation
## Secure Login & Authentication with Auto-Redirect

**Objective:** Every page requires login. Unauthenticated users redirected to login page.

---

## STEP 1: Protect All Controllers (30 minutes)

### 1.1 Update HomeController

**File:** `Controllers/HomeController.cs`

**REPLACE entire file with:**

```csharp
using System.Web.Mvc;

namespace eMedLis.Controllers
{
    [Authorize]  // ADD THIS - Requires authentication
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}
```

**What changed:**
- Added `[Authorize]` attribute to class
- Now ALL actions require authentication
- Unauthenticated users get redirected to login

### Your Action:
1. Open `Controllers/HomeController.cs`
2. Add `[Authorize]` above `public class HomeController`
3. Save the file

---

### 1.2 Protect All Other Controllers

**ALL controllers should have `[Authorize]` except AccountController**

**Files to update:**
- `Controllers/SampleCollectionController.cs` → Add `[Authorize]` above class
- `Controllers/PatientBillingController.cs` → Add `[Authorize]` above class
- `Controllers/DuePaymentController.cs` → Add `[Authorize]` above class
- `Controllers/ManageController.cs` → Add `[Authorize]` above class
- `Controllers/DepartmentController.cs` → Add `[Authorize]` above class
- `Controllers/SubDepartmentController.cs` → Add `[Authorize]` above class
- `Controllers/InvestigationController.cs` → Add `[Authorize]` above class
- `Controllers/LookupController.cs` → Add `[Authorize]` above class

**Example pattern:**

```csharp
using System.Web.Mvc;

namespace eMedLis.Controllers
{
    [Authorize]  // ADD THIS LINE
    public class SampleCollectionController : Controller
    {
        // Rest of code...
    }
}
```

### Your Action:
1. Go through each controller file
2. Find `public class [ControllerName]`
3. Add `[Authorize]` above it
4. Save each file

⏱️ **Time:** 15 minutes for all controllers

---

## STEP 2: Configure Web.config (15 minutes)

**File:** `Web.config`

**Find the `<authentication>` section in `<system.web>`:**

```xml
<system.web>
  <authentication mode="None" />
  <!-- other settings... -->
</system.web>
```

**ADD THIS after `<authentication>` line:**

```xml
<system.web>
  <authentication mode="None" />
  
  <!-- Authorization: Deny all by default, allow specific -->
  <authorization>
    <deny users="?" />  <!-- ? = unauthenticated users -->
  </authorization>
  
  <!-- other settings... -->
</system.web>
```

**What this does:**
- `deny users="?"` = Deny unauthenticated users from all pages
- `[AllowAnonymous]` attributes override this setting
- Login, Register, etc. will use `[AllowAnonymous]`

### Your Action:
1. Open `Web.config`
2. Find `<system.web>` section
3. Find `<authentication mode="None" />`
4. Add the `<authorization>` block after it
5. Save the file

⏱️ **Time:** 5 minutes

---

## STEP 3: Configure Route Redirection (15 minutes)

**File:** `App_Start/RouteConfig.cs`

**FIND the RegisterRoutes method, it should look like:**

```csharp
public class RouteConfig
{
    public static void RegisterRoutes(RouteCollection routes)
    {
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

        routes.MapRoute(
            name: "Default",
            url: "{controller}/{action}/{id}",
            defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
        );
    }
}
```

**CHANGE the default controller to Account/Login:**

```csharp
public class RouteConfig
{
    public static void RegisterRoutes(RouteCollection routes)
    {
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

        routes.MapRoute(
            name: "Default",
            url: "{controller}/{action}/{id}",
            defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional }  // CHANGED
        );
    }
}
```

**What changed:**
- Default controller changed from `Home` to `Account`
- Default action changed from `Index` to `Login`
- When user goes to home page, they're redirected to login first

### Your Action:
1. Open `App_Start/RouteConfig.cs`
2. Change `controller = "Home"` to `controller = "Account"`
3. Change `action = "Index"` to `action = "Login"`
4. Save the file

⏱️ **Time:** 5 minutes

---

## STEP 4: Update Web.config Authentication Settings (15 minutes)

**File:** `Web.config`

**Find the `<runtime>` or near the end, find `<system.webServer>` section**

**Inside `<system.webServer>`, ADD this:**

```xml
<system.webServer>
  <!-- ... existing content ... -->
  
  <!-- Remove default FormsAuthentication module -->
  <modules>
    <remove name="FormsAuthentication" />
  </modules>
  
  <!-- Redirect unauthorized requests to login -->
  <httpErrors errorMode="DetailedLocalOnly">
    <error statusCode="401" path="/Account/Login" />
  </httpErrors>
</system.webServer>
```

**What this does:**
- Removes old FormsAuthentication (we use OWIN)
- Redirects 401 Unauthorized to login page
- Ensures unauthenticated users see login page

### Your Action:
1. Open `Web.config`
2. Find `<system.webServer>` section
3. Add the modules and httpErrors sections
4. Save the file

⏱️ **Time:** 10 minutes

---

## STEP 5: Verify Login Views (10 minutes)

**File:** `Views/Account/Login.cshtml`

**Should exist and look like:**

```html
@model eMedLis.Models.LoginViewModel

@{
    ViewBag.Title = "Log in";
}

<div class="row">
    <div class="col-md-8">
        <section id="loginForm">
            @using (Html.BeginForm("Login", "Account", new { ReturnUrl = ViewBag.ReturnUrl }, FormMethod.Post, new { @class = "form-horizontal", role = "form" }))
            {
                @Html.AntiForgeryToken()
                <h4>Use a local account to log in.</h4>
                <hr />
                @Html.ValidationSummary(true, "", new { @class = "text-danger" })
                <div class="form-group">
                    @Html.LabelFor(m => m.Email, new { @class = "col-md-2 control-label" })
                    <div class="col-md-10">
                        @Html.TextBoxFor(m => m.Email, new { @class = "form-control" })
                        @Html.ValidationMessageFor(m => m.Email, "", new { @class = "text-danger" })
                    </div>
                </div>
                <div class="form-group">
                    @Html.LabelFor(m => m.Password, new { @class = "col-md-2 control-label" })
                    <div class="col-md-10">
                        @Html.PasswordFor(m => m.Password, new { @class = "form-control" })
                        @Html.ValidationMessageFor(m => m.Password, "", new { @class = "text-danger" })
                    </div>
                </div>
                <div class="form-group">
                    <div class="col-md-offset-2 col-md-10">
                        <div class="checkbox">
                            @Html.CheckBoxFor(m => m.RememberMe)
                            @Html.LabelFor(m => m.RememberMe)
                        </div>
                    </div>
                </div>
                <div class="form-group">
                    <div class="col-md-offset-2 col-md-10">
                        <button type="submit" class="btn btn-primary">Log in</button>
                    </div>
                </div>
                <p>
                    @Html.ActionLink("Register as a new user", "Register")
                </p>
            }
        </section>
    </div>
</div>
```

### Your Action:
1. Check if `Views/Account/Login.cshtml` exists
2. If NOT, create it with the code above
3. If YES, verify it looks similar
4. Save the file

⏱️ **Time:** 5 minutes

---

## STEP 6: Create Layout with Logout Link (10 minutes)

**File:** `Views/Shared/_Layout.cshtml`

**Find the navigation/header section and ADD logout button:**

**Add this to your navbar/menu:**

```html
<!-- In your navbar, add: -->
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
else
{
    <ul class="nav navbar-nav navbar-right">
        <li>@Html.ActionLink("Login", "Login", "Account")</li>
    </ul>
}
```

### Your Action:
1. Open `Views/Shared/_Layout.cshtml`
2. Find the navbar/header section
3. Add the code above for logout button
4. Save the file

⏱️ **Time:** 10 minutes

---

## STEP 7: Update AccountController Settings (15 minutes)

**File:** `Controllers/AccountController.cs`

**IMPORTANT: The login function should NOT have `[Authorize]` at class level**

**Current code has:** `[Authorize]` on the class

**CHANGE TO:**

```csharp
namespace eMedLis.Controllers
{
    public class AccountController : Controller  // REMOVE [Authorize] from here
    {
        // ... rest of code stays the same ...
    }
}
```

**Keep these as `[AllowAnonymous]`** (they already are):
- Login GET/POST
- Register GET/POST
- ForgotPassword GET/POST
- ResetPassword GET/POST
- VerifyCode GET/POST
- SendCode GET/POST
- All External login methods

### Your Action:
1. Open `Controllers/AccountController.cs`
2. Find `[Authorize]` above `public class AccountController`
3. **DELETE it** (all Account methods have their own `[AllowAnonymous]`)
4. LogOff method should stay as is (protected, only authenticated users can logout)
5. Save the file

⏱️ **Time:** 5 minutes

---

## STEP 8: Compile & Test (15 minutes)

### Compile:
1. Visual Studio → Build → Clean Solution
2. Visual Studio → Build → Rebuild Solution
3. ✅ Should compile with NO errors

### Test:

1. **Run application (F5)**
   - Should start at login page
   - Not at home page

2. **Try to access Home without login:**
   - Go to http://localhost:XXXX/
   - Should redirect to http://localhost:XXXX/Account/Login
   - ✅ CORRECT BEHAVIOR

3. **Try to access any page without login:**
   - Go to http://localhost:XXXX/SampleCollection/Index
   - Should redirect to login
   - ✅ CORRECT BEHAVIOR

4. **Create test user:**
   - Go to login page
   - Click "Register as a new user"
   - Enter: 
     - Email: `test@lab.com`
     - Password: `Test@123456` (must have uppercase, number, special char)
   - Click Register
   - Should be logged in
   - Should see "Hello, test@lab.com" in top right

5. **Try to access Home:**
   - Go to http://localhost:XXXX/Home/Index
   - Should load successfully
   - ✅ CORRECT BEHAVIOR

6. **Test logout:**
   - Click "Log off" button
   - Should go to login page
   - Try to access any page
   - Should redirect to login
   - ✅ CORRECT BEHAVIOR

---

## STEP 9: Create Test Users (Optional)

**SQL Script to create test users in your database:**

```sql
-- Check existing users
SELECT Id, UserName, Email FROM AspNetUsers

-- You can also create users through the UI by registering
```

---

## ✅ SUCCESS CRITERIA

Your authentication is working when:

- ✅ Visiting home page redirects to login
- ✅ Cannot access ANY page without logging in
- ✅ Can create new user account
- ✅ Can login with created account
- ✅ After login, can access home page
- ✅ After logout, cannot access any page
- ✅ All other controllers redirect to login when not authenticated
- ✅ No compilation errors
- ✅ No runtime errors

---

## FILE CHECKLIST

**Files to modify:**
- [ ] `Controllers/HomeController.cs` - Add `[Authorize]`
- [ ] `Controllers/SampleCollectionController.cs` - Add `[Authorize]`
- [ ] `Controllers/PatientBillingController.cs` - Add `[Authorize]`
- [ ] `Controllers/DuePaymentController.cs` - Add `[Authorize]`
- [ ] `Controllers/ManageController.cs` - Add `[Authorize]`
- [ ] `Controllers/DepartmentController.cs` - Add `[Authorize]`
- [ ] `Controllers/SubDepartmentController.cs` - Add `[Authorize]`
- [ ] `Controllers/InvestigationController.cs` - Add `[Authorize]`
- [ ] `Controllers/LookupController.cs` - Add `[Authorize]`
- [ ] `Controllers/AccountController.cs` - Remove class-level `[Authorize]`
- [ ] `App_Start/RouteConfig.cs` - Change default to Account/Login
- [ ] `Web.config` - Add authorization and httpErrors sections
- [ ] `Views/Shared/_Layout.cshtml` - Add logout button
- [ ] `Views/Account/Login.cshtml` - Verify exists

---

## TOTAL TIME: ~1.5 hours

- Step 1: 15 min (Add [Authorize])
- Step 2: 5 min (Web.config auth)
- Step 3: 5 min (RouteConfig)
- Step 4: 10 min (Web.config webServer)
- Step 5: 5 min (Verify Login view)
- Step 6: 10 min (Logout button)
- Step 7: 5 min (AccountController cleanup)
- Step 8: 15 min (Test)
- Step 9: 5 min (Optional test users)

---

## COMMON ISSUES & FIXES

**Issue:** Still going to Home instead of Login
- **Fix:** Check RouteConfig.cs - verify controller is "Account" and action is "Login"
- **Fix:** Clean solution and rebuild
- **Fix:** Clear browser cache (Ctrl+Shift+Delete)

**Issue:** Login button redirects to 404
- **Fix:** Check Views/Account/Login.cshtml exists
- **Fix:** Make sure AccountController is NOT marked `[Authorize]` at class level

**Issue:** Can access pages without login
- **Fix:** Verify `[Authorize]` is added above each controller class
- **Fix:** Verify Web.config has `<deny users="?" />`
- **Fix:** Rebuild and restart application

**Issue:** Logout button doesn't work
- **Fix:** Check Form method is POST
- **Fix:** Check @Html.AntiForgeryToken() is included
- **Fix:** Check LogOff method exists in AccountController

---

## NEXT: Weeks 2-4 (When Ready)

Once authentication is working, we'll add:
- Week 2: Audit logging for all actions
- Week 3: Role-based access control
- Week 4: Input validation & security

**But first: Let's get authentication solid! 🚀**
