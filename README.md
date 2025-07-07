# Sector 13 Welfare Society – Digital Management System

A comprehensive web-based digital management system for Sector 13 Welfare Society, built with ASP.NET Core MVC. This system provides complete management capabilities for welfare society operations including member management, donations, user roles, and administrative functions.

## 🌟 Features

### 👥 User Management & Authentication
- **Multi-Role User System**: Super Admin, Admin, President, Secretary, Manager, and Member roles
- **Secure Authentication**: ASP.NET Core Identity with password hashing
- **User Registration & Login**: Custom registration and login forms
- **Role-Based Access Control**: Different dashboards and permissions for each role
- **User Profile Management**: Update user information and manage accounts

### 📊 Dashboard System
- **Role-Specific Dashboards**: 
  - **Super Admin**: Complete system overview and management
  - **Admin**: Administrative functions and user management
  - **President**: Executive overview and decision-making tools
  - **Secretary**: Administrative tasks and record keeping
  - **Manager**: Operational management and reporting
  - **Member**: Basic member information and access

### 💰 Donation Management
- **Multiple Payment Methods**:
  - SSL Commerz integration for online payments
  - Manual payment instructions
  - Test donation functionality
- **Donation Tracking**: Complete donation history and records
- **Payment Processing**: Secure payment gateway integration
- **Donation Success Handling**: Confirmation pages and receipts
- **Donor Information Management**: Store and manage donor details

### 📋 Member Directory
- **Interactive PDF Viewer**: 
  - Side-by-side page viewing
  - Responsive design for all devices
  - Navigation controls with keyboard support
  - Download functionality
- **Member Information**: Complete member database and directory
- **Search & Browse**: Easy navigation through member listings

### 🏢 Organization Information
- **About Us**: Detailed information about the welfare society
- **Contact Information**: Multiple ways to reach the organization
- **Gallery**: Photo gallery showcasing activities and events
- **Message from Chairman**: Leadership communication
- **How We Work**: Operational procedures and processes
- **Partnerships**: Information about organizational partnerships

### 🛠️ Administrative Tools
- **Database Management**: 
  - Database reset functionality
  - User data management
  - System maintenance tools
- **Test User Creation**: Development and testing utilities
- **User Management**: 
  - View all users
  - Manage user roles
  - User account administration

### 📱 Responsive Design
- **Mobile-First Approach**: Optimized for all device sizes
- **Cross-Browser Compatibility**: Works on all modern browsers
- **Touch-Friendly Interface**: Optimized for touch devices
- **Responsive Navigation**: Adaptive menu system

### 🔒 Security Features
- **Authentication & Authorization**: Secure login system
- **Role-Based Security**: Access control based on user roles
- **Data Protection**: Secure handling of sensitive information
- **Session Management**: Secure user sessions

## 🚀 Technology Stack

- **Backend**: ASP.NET Core 8.0 MVC
- **Database**: Entity Framework Core with SQL Server
- **Frontend**: HTML5, CSS3, JavaScript, Bootstrap 5
- **Authentication**: ASP.NET Core Identity
- **Payment Gateway**: SSL Commerz Integration
- **PDF Processing**: PDF.js for client-side PDF rendering
- **Icons**: Font Awesome 6.4.2

## 📁 Project Structure

```
Sector 13 Welfare Society – Digital Management System/
├── Controllers/                 # MVC Controllers
│   ├── AccountController.cs    # Authentication & User Management
│   ├── DashboardController.cs  # Dashboard Logic
│   ├── DatabaseController.cs   # Database Operations
│   ├── DonationController.cs   # Donation Processing
│   ├── HomeController.cs       # Main Pages
│   ├── SetupController.cs      # System Setup
│   └── UserManagementController.cs # User Administration
├── Models/                     # Data Models
│   ├── ApprovalRequest.cs
│   ├── Donor.cs
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   └── SSLCommerzResponse.cs
├── Views/                      # Razor Views
│   ├── Account/               # Authentication Views
│   ├── Dashboard/             # Role-Specific Dashboards
│   ├── Database/              # Database Management Views
│   ├── Donation/              # Donation Related Views
│   ├── Home/                  # Main Website Pages
│   ├── Setup/                 # System Setup Views
│   └── UserManagement/        # User Management Views
├── Data/                      # Database Context & Migrations
│   ├── ApplicationDbContext.cs
│   └── Migrations/
├── wwwroot/                   # Static Files
│   ├── css/                   # Stylesheets
│   ├── js/                    # JavaScript Files
│   ├── lib/                   # Third-party Libraries
│   ├── Photos/                # Images
│   ├── memberDirectory/       # PDF Files
│   └── partnerships/          # Partnership Logos
└── Areas/                     # Identity Area
    └── Identity/
        └── Pages/
```

## 🛠️ Installation & Setup

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### Installation Steps

1. **Clone the Repository**
   ```bash
   git clone https://github.com/yourusername/sector13-welfare-society.git
   cd sector13-welfare-society
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure Database**
   - Update connection string in `appsettings.json`
   - Run Entity Framework migrations:
   ```bash
   dotnet ef database update
   ```

4. **Run the Application**
   ```bash
   dotnet run
   ```

5. **Access the Application**
   - Open browser and navigate to `https://localhost:5001`
   - Default admin credentials will be created during first run

## 🔧 Configuration

### Database Configuration
Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=Sector13WelfareSociety;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### Payment Gateway Configuration
Configure SSL Commerz settings in `appsettings.json`:
```json
{
  "SSLCommerz": {
    "StoreId": "your_store_id",
    "StorePassword": "your_store_password",
    "IsSandbox": true
  }
}
```

## 👥 User Roles & Permissions

### Super Admin
- Complete system access
- User management
- Database administration
- System configuration

### Admin
- User management
- Administrative functions
- Report generation

### President
- Executive overview
- Decision-making tools
- High-level reporting

### Secretary
- Administrative tasks
- Record keeping
- Member management

### Manager
- Operational management
- Activity coordination
- Basic reporting

### Member
- View member directory
- Access basic information
- Limited functionality

## 💳 Payment Integration

The system integrates with SSL Commerz payment gateway for secure online donations:

- **Secure Payment Processing**: SSL-encrypted transactions
- **Multiple Payment Methods**: Credit cards, mobile banking, etc.
- **Transaction Tracking**: Complete payment history
- **Receipt Generation**: Automatic receipt creation
- **Error Handling**: Comprehensive error management

## 📱 Responsive Features

- **Mobile Optimization**: Touch-friendly interface
- **Tablet Support**: Optimized layouts for tablets
- **Desktop Experience**: Full-featured desktop interface
- **Cross-Device Sync**: Consistent experience across devices

## 🔒 Security Features

- **Password Hashing**: Secure password storage
- **Session Management**: Secure user sessions
- **Role-Based Access**: Granular permission system
- **Data Validation**: Input validation and sanitization
- **SQL Injection Protection**: Parameterized queries

## 📊 Database Schema

The system uses Entity Framework Core with the following main entities:

- **ApplicationUser**: User accounts and profiles
- **Donor**: Donor information and history
- **ApprovalRequest**: Request management system
- **SSLCommerzResponse**: Payment transaction records

## 🚀 Deployment

### Local Development
```bash
dotnet run --environment Development
```

### Production Deployment
```bash
dotnet publish -c Release
```

### Docker Support
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY bin/Release/net8.0/publish/ App/
WORKDIR /App
ENTRYPOINT ["dotnet", "Sector 13 Welfare Society – Digital Management System.dll"]
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

For support and questions:
- Create an issue in the GitHub repository
- Contact the development team
- Check the documentation in the `/docs` folder

## 🔄 Version History

- **v1.0.0** - Initial release with basic functionality
- **v1.1.0** - Added donation management system
- **v1.2.0** - Enhanced user management and role system
- **v1.3.0** - Improved responsive design and mobile support
- **v1.4.0** - Added PDF viewer and member directory
- **v1.5.0** - Enhanced security and performance optimizations

## 🙏 Acknowledgments

- Sector 13 Welfare Society for the project requirements
- ASP.NET Core team for the excellent framework
- Bootstrap team for the responsive UI framework
- Font Awesome for the icon library
- SSL Commerz for payment gateway integration

---

**Built with ❤️ for Sector 13 Welfare Society** 
