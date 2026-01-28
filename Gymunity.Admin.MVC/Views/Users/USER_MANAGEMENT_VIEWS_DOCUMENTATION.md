# User Management Views Documentation

## Overview
This document describes the views created for the User Management module in the ITI.Gymunity.FP.Admin.MVC project.

## Views Structure

### Main Views

#### 1. **Index.cshtml** - User List View
**Path:** `ITI.Gymunity.FP.Admin.MVC/Views/Users/Index.cshtml`

**Purpose:** Displays all users in a paginated table with search and filter functionality.

**Model:** `UsersListViewModel`

**Features:**
- Paginated user list (default 10 per page)
- Search functionality by email, username, or name
- Quick filter buttons for Client, Trainer, Admin roles
- User statistics cards showing total users and pagination info
- Responsive data table with sortable columns
- Bulk action support (change role)
- Individual user actions (view, edit, delete)
- Checkbox selection for bulk operations
- Status indicators for each user (locked, suspended, verified)

**Key Elements:**
- Search form with quick filters
- Statistics cards showing totals
- Responsive table with hover effects
- Pagination controls
- Bulk action dropdown and apply button
- Action buttons for each row (Details, Edit, Delete)

**Supported Actions:**
- View user details
- Edit user information
- Delete user account
- Change role (bulk)
- Navigate to Statistics view

---

#### 2. **Details.cshtml** - User Detail View
**Path:** `ITI.Gymunity.FP.Admin.MVC/Views/Users/Details.cshtml`

**Purpose:** Displays comprehensive user information with management options.

**Model:** `UserDetailViewModel`

**Features:**
- Complete user account information
- Role and security settings
- Email verification status
- Account verification status
- Trainer-specific information (if applicable)
- Quick action buttons
- Status summary panel
- Account lockout information
- Multiple modal dialogs for bulk operations

**Key Sections:**
1. **Account Information Card**
   - Full Name, Email, Username
   - Phone Number, Created At, Last Login

2. **Role & Security Card**
   - Current role badge
   - Current roles list
   - Email confirmation status
   - Account verification status
   - Login attempt counter

3. **Trainer Information Card** (Conditional)
   - Only shown for Trainer role
   - Verify trainer button

4. **Quick Actions Panel**
   - Change Role
   - Reset Password
   - Lock/Unlock Account
   - Suspend/Reactivate Account
   - Delete User

5. **Status Summary Card**
   - Email Status
   - Account Status
   - Verification Status

**Modals Included:**
- Change Role Modal
- Lock Account Modal (with duration)
- Suspend Account Modal (with reason)
- Delete User Modal (with confirmation)

**Supported Actions:**
- Back to users list
- Edit user
- Change role
- Reset password
- Lock/Unlock account
- Suspend/Reactivate account
- Delete user

---

#### 3. **Edit.cshtml** - User Edit View
**Path:** `ITI.Gymunity.FP.Admin.MVC/Views/Users/Edit.cshtml`

**Purpose:** Allows editing of user profile and account settings.

**Model:** `UserEditViewModel`

**Features:**
- Edit personal information (first name, last name, email, username)
- Phone number input
- Profile photo upload
- Role selection dropdown
- Status checkboxes (email confirmed, account verified)
- Bio/description field
- Read-only timestamps (created, updated)
- Sidebar help information
- Account overview panel
- Dangerous actions section

**Form Sections:**
1. **Personal Information**
   - First Name
   - Last Name
   - Email Address
   - Username
   - Phone Number
   - Profile Photo Upload

2. **Account Settings**
   - Role dropdown (Client, Trainer, Admin)
   - Email Confirmed checkbox
   - Account Verified checkbox
   - Account Status alert

3. **Additional Information**
   - Bio/Description textarea
   - Created At (read-only)
   - Last Updated (read-only)

**Sidebar Elements:**
- Help card with role information
- Account overview showing current settings
- Dangerous actions (Reset Password, Delete Account)

**Validation:**
- Required fields marked
- Email format validation
- Custom validation messages

**Supported Actions:**
- Back to details
- Save changes
- Cancel editing
- Reset password
- Delete account

---

#### 4. **Statistics.cshtml** - User Statistics View
**Path:** `ITI.Gymunity.FP.Admin.MVC/Views/Users/Statistics.cshtml`

**Purpose:** Provides analytics and insights about users.

**Model:** `UserStatisticsViewModel`

**Features:**
- Key metrics cards
- User distribution by role (pie chart)
- Account status distribution (bar chart)
- Email verification status (pie chart)
- User activity metrics
- Most active users table
- Export functionality (CSV, Excel, PDF)

**Key Metrics Displayed:**
- Total Users count
- Active Users count
- Verified Users count
- Locked Accounts count
- New users this month
- Active last 7 days
- Inactive 30+ days
- Email confirmed count

**Charts:**
1. **Role Distribution (Doughnut):** Shows Client, Trainer, Admin counts
2. **Account Status (Bar):** Shows Active, Locked, Verified, Unverified counts
3. **Email Verification (Pie):** Shows Confirmed vs Pending email status

**Tables:**
- Most Active Users with login count and last login date

**Export Options:**
- CSV format
- Excel format
- PDF format

**Required Libraries:**
- Chart.js v3.9.1 (for charts)

---

### Partial Views

#### 1. **_Navigation.cshtml** - Navigation Component
**Path:** `ITI.Gymunity.FP.Admin.MVC/Views/Users/_Navigation.cshtml`

**Purpose:** Reusable navigation tabs and quick links for user management.

**Features:**
- Navigation tabs for Users, Statistics, Roles
- Quick filter buttons for each role
- Quick access to all main views

---

#### 2. **_UserCard.cshtml** - User Card Component
**Path:** `ITI.Gymunity.FP.Admin.MVC/Views/Users/_UserCard.cshtml`

**Purpose:** Displays user information in a card format.

**Model:** `UserItemViewModel`

**Features:**
- Profile photo or default avatar
- User name and role badge
- Email address
- Status badges (verified, locked, email status)
- Join date and last login
- Action buttons (View, Edit, Delete)

**Used In:** Grid/card layouts for users

---

#### 3. **_BulkActionsModal.cshtml** - Bulk Actions Modal
**Path:** `ITI.Gymunity.FP.Admin.MVC/Views/Users/_BulkActionsModal.cshtml`

**Purpose:** Modal dialog for performing bulk operations on multiple users.

**Features:**
- Selected user count display
- Action selection dropdown
- Conditional fields based on action:
  - Role selection (for change-role)
  - Duration input (for lock action)
- Confirmation message
- Form submission for bulk operations

**Supported Actions:**
- Change Role
- Verify Accounts
- Confirm Email
- Lock Accounts
- Unlock Accounts
- Delete Users

---

#### 4. **_Messages.cshtml** - Message Display Component
**Path:** `ITI.Gymunity.FP.Admin.MVC/Views/Users/_Messages.cshtml`

**Purpose:** Displays success, error, warning, and info messages.

**Supported Message Types:**
- Success (green badge)
- Error (red badge)
- Warning (yellow badge)
- Info (blue badge)

**ViewData Keys:**
- `SuccessMessage`
- `ErrorMessage`
- `WarningMessage`
- `InfoMessage`

---

#### 5. **_StatusBadges.cshtml** - Status Badges Component
**Path:** `ITI.Gymunity.FP.Admin.MVC/Views/Users/_StatusBadges.cshtml`

**Purpose:** Displays role and status badges for user.

**ViewBag Properties Required:**
- `Role` - User role (Admin, Trainer, Client)
- `IsVerified` - Account verification status
- `IsLockedOut` - Account lockout status
- `EmailConfirmed` - Email confirmation status

**Features:**
- Color-coded role badges with icons
- Status indicators (verified, locked, active)
- Email verification status

---

## View Models Used

The views require the following ViewModels:

1. **UsersListViewModel** - For Index view
2. **UserDetailViewModel** - For Details view
3. **UserEditViewModel** - For Edit view
4. **UserStatisticsViewModel** - For Statistics view
5. **UserItemViewModel** - For _UserCard partial

## Styling & Dependencies

### CSS Framework
- Bootstrap 5 (assumed from project)
- Custom CSS for table hover effects and card layouts

### JavaScript Libraries
- jQuery (for modals and form handling)
- Bootstrap JS (for modals and tooltips)
- Chart.js v3.9.1 (for statistics charts)

### Icons
- FontAwesome (for all icons used)

## Key Features Summary

### Search & Filter
- Full-text search across email, username, name
- Quick role-based filters
- Pagination support

### User Actions
- View detailed user information
- Edit user profile and settings
- Change user role
- Reset password
- Lock/Unlock accounts
- Suspend/Reactivate accounts
- Delete user accounts

### Bulk Operations
- Select multiple users
- Change role for bulk users
- Verify accounts in bulk
- Confirm emails in bulk
- Lock/Unlock accounts in bulk
- Delete multiple users

### Statistics & Analytics
- Visual charts for user distribution
- Activity metrics and trends
- User activity history
- Export data in multiple formats

### Responsive Design
- Mobile-friendly tables
- Responsive cards and panels
- Touch-friendly buttons
- Accessible form inputs

## JavaScript Functions

Key JavaScript functions available in the views:

### In Index.cshtml
```javascript
- selectAll() - Select/deselect all users
- performBulkAction() - Execute bulk actions
- deleteUser(userId, userName) - Delete single user with confirmation
```

### In Details.cshtml
```javascript
- toggleActionFields() - Show/hide fields based on selected action
- openBulkActionsModal(selectedIds) - Open bulk actions modal
```

### In Statistics.cshtml
```javascript
- Chart initialization for role, status, and email charts
```

## Integration Notes

1. All views expect authenticated admin users
2. Authorization should be enforced at the controller level
3. Form posts should include CSRF tokens (handled by ASP.NET Core)
4. Images should be served through proper image hosting/CDN
5. Charts require Chart.js library to be loaded

## Future Enhancements

1. Add Excel/PDF export functionality
2. Add advanced search filters
3. Add user activity logs view
4. Add role-based permission management UI
5. Add user import functionality
6. Add bulk email sending
7. Add user deactivation (soft delete)
8. Add audit trail for admin actions

---

**Last Updated:** @DateTime.Now.ToShortDateString()
**Version:** 1.0
**Status:** Complete - Ready for Integration
