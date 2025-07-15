# Email Setup Instructions

## Forgot Password Email Configuration

To enable the forgot password functionality, you need to configure email settings in your application.

### Step 1: Gmail Setup (Recommended)

1. **Enable 2-Factor Authentication** on your Gmail account
2. **Generate an App Password**:
   - Go to your Google Account settings
   - Navigate to Security
   - Under "2-Step Verification", click on "App passwords"
   - Generate a new app password for "Mail"
   - Copy the generated password

### Step 2: Update Configuration

Update the email settings in `appsettings.json` or `appsettings.Development.json`:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "SenderEmail": "your-email@gmail.com",
    "SenderName": "Sector 13 Welfare Society"
  }
}
```

Replace:
- `your-email@gmail.com` with your actual Gmail address
- `your-app-password` with the app password generated in Step 1

### Step 3: Alternative Email Providers

If you prefer to use other email providers:

#### Outlook/Hotmail:
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp-mail.outlook.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@outlook.com",
    "SmtpPassword": "your-password",
    "SenderEmail": "your-email@outlook.com",
    "SenderName": "Sector 13 Welfare Society"
  }
}
```

#### Yahoo:
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.mail.yahoo.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@yahoo.com",
    "SmtpPassword": "your-app-password",
    "SenderEmail": "your-email@yahoo.com",
    "SenderName": "Sector 13 Welfare Society"
  }
}
```

### Step 4: Testing

1. Start your application
2. Go to the login page
3. Click "Forgot Password?"
4. Enter a valid email address
5. Click "Send Reset Link"
6. Check your email for the password reset link

### Security Notes

- Never commit your actual email credentials to version control
- Use environment variables or user secrets for production
- The app password is different from your regular Gmail password
- Make sure your email account has SMTP access enabled

### Troubleshooting

If emails are not being sent:

1. Check your email credentials
2. Verify your app password is correct
3. Ensure your email provider allows SMTP access
4. Check your firewall/antivirus settings
5. Verify the SMTP server and port settings

### For Production

For production environments, consider using:
- SendGrid
- Mailgun
- Amazon SES
- Or other professional email services

These services provide better deliverability and monitoring capabilities. 