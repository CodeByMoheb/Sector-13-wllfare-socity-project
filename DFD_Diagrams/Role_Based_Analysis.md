# Role-Based Functionality Analysis
## Sector 13 Welfare Society - Digital Management System

### **MEMBER ROLE FUNCTIONALITIES:**

#### **Core Functions:**
1. **Dashboard Access** - Personal dashboard with profile information
2. **Profile Management** - Update personal information
3. **Notice Viewing** - View public notices and announcements
4. **Member Directory** - Browse member directory
5. **Donation Access** - View donation information and make donations
6. **Event Information** - Access society events and activities

#### **Employee-Member Hybrid:**
- If member is also an employee (EmployeeID as username):
  - **Self Attendance** - Check-in/Check-out functionality
  - **Leave Requests** - Apply for leave
  - **Attendance History** - View personal attendance records
  - **Leave Balance** - Check leave balances

---

### **MANAGER ROLE FUNCTIONALITIES:**

#### **Employee Management:**
1. **Employee Registration** - Add new employees
2. **Employee Profile Management** - Update employee information
3. **Shift Assignment** - Assign work shifts to employees
4. **Salary Management** - Manage employee compensation

#### **Attendance Management:**
1. **Daily Attendance** - View and manage daily attendance
2. **Attendance Records** - Browse complete attendance history
3. **Attendance Reports** - Generate attendance reports
4. **Leave Approval** - Approve/reject leave requests

#### **Notice Management:**
1. **Notice Creation** - Create notices for approval
2. **Notice Management** - Manage notices (with Manager tools)

#### **Operational Management:**
1. **Manager Tools** - SMS notifications and communications
2. **Donation Reports** - View donation reports
3. **Permanent Member Management** - View member lists

---

### **SECRETARY ROLE FUNCTIONALITIES:**

#### **Administrative Management:**
1. **Notice Management** - Create, approve, and publish notices
2. **Record Keeping** - Maintain organizational records
3. **Member Verification** - Verify new member applications
4. **Meeting Management** - Manage meeting records and minutes

#### **Notice Workflow:**
1. **Notice Creation** - Draft notices and announcements
2. **Notice Approval** - Review and approve notices for publication
3. **Notice Publishing** - Publish approved notices
4. **Notice Archive** - Maintain notice archives

#### **Content Management:**
1. **Leadership Messages** - Manage leadership communications
2. **Elected Candidates** - Manage candidate information
3. **Previous Candidates** - Maintain historical candidate records

#### **Communication:**
1. **Member Communication** - Send notifications to members
2. **Public Communication** - Manage public announcements

---

### **EMPLOYEE ROLE FUNCTIONALITIES:**

#### **Self-Service Functions:**
1. **Employee Login** - Login using EmployeeID and password
2. **Personal Dashboard** - View employee dashboard with shift info
3. **Profile Management** - Update personal information
4. **Password Management** - Change password

#### **Attendance Management:**
1. **Check-in/Check-out** - Mark attendance with timestamps
2. **Attendance History** - View personal attendance records
3. **Location Tracking** - Optional location-based attendance
4. **Real-time Status** - View current attendance status

#### **Leave Management:**
1. **Leave Application** - Apply for different types of leave
2. **Leave History** - View leave application status
3. **Leave Balance** - Check available leave balances
4. **Leave Types** - Access to Casual, Sick, Paid, Unpaid leave

#### **Shift Information:**
1. **Shift Details** - View assigned shift timings
2. **Schedule Management** - View work schedule
3. **Time Validation** - System validates against shift times
4. **Late Detection** - Automatic late marking system

#### **Salary Information:**
1. **Salary Calculation** - View calculated salary based on attendance
2. **Attendance Impact** - See how attendance affects salary
3. **Leave Integration** - Paid leave counts as working days

---

### **CROSS-ROLE INTERACTIONS:**

#### **Manager ↔ Employee:**
- Manager approves employee leave requests
- Manager manages employee attendance
- Manager assigns shifts to employees
- Manager views employee performance

#### **Secretary ↔ Member:**
- Secretary verifies member applications
- Secretary publishes notices for members
- Secretary manages member communications

#### **Manager ↔ Secretary:**
- Both can manage notices (different approval levels)
- Both have access to member management
- Both can generate reports

#### **Employee ↔ Member:**
- Same person can have both roles
- Employee functionality extends member functionality
- Shared profile management

This analysis forms the basis for creating detailed DFDs for each role and their interactions.







