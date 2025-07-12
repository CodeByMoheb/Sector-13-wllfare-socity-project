# Quick Email Setup Options

## Option A: Gmail (Most Common)
1. Enable 2-Step Verification on your Google account
2. Generate an App Password
3. Update settings:

```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SmtpUsername": "your-email@gmail.com",
  "SmtpPassword": "your-16-char-app-password",
  "SenderEmail": "your-email@gmail.com",
  "SenderName": "Sector 13 Welfare Society"
}
```

## Option B: Outlook/Hotmail
```json
"EmailSettings": {
  "SmtpServer": "smtp-mail.outlook.com",
  "SmtpPort": 587,
  "SmtpUsername": "your-email@outlook.com",
  "SmtpPassword": "your-normal-password",
  "SenderEmail": "your-email@outlook.com",
  "SenderName": "Sector 13 Welfare Society"
}
```

## Option C: Yahoo
1. Enable 2-Step Verification
2. Generate App Password
3. Update settings:

```json
"EmailSettings": {
  "SmtpServer": "smtp.mail.yahoo.com",
  "SmtpPort": 587,
  "SmtpUsername": "your-email@yahoo.com",
  "SmtpPassword": "your-app-password",
  "SenderEmail": "your-email@yahoo.com",
  "SenderName": "Sector 13 Welfare Society"
}
```

## Steps to Apply:
1. Choose your email provider
2. Update `appsettings.Development.json` with the settings above
3. Replace `your-email@provider.com` with your actual email
4. Replace `your-password` with your actual password
5. Restart the application: `dotnet run`
6. Test forgot password functionality 