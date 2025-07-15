# Gmail Email Setup Guide

## Step 1: Enable 2-Step Verification
1. Go to https://myaccount.google.com/
2. Click "Security" in the left sidebar
3. Find "2-Step Verification" and click "Get started"
4. Follow the steps to enable 2-Step Verification

## Step 2: Generate App Password
1. Go back to Security page
2. Find "App passwords" (appears after enabling 2-Step Verification)
3. Click "App passwords"
4. Select "Mail" from dropdown
5. Click "Generate"
6. Copy the 16-character password (example: `abcd efgh ijkl mnop`)

## Step 3: Update Configuration
Replace the email settings in `appsettings.Development.json`:

```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SmtpUsername": "YOUR_GMAIL@gmail.com",
  "SmtpPassword": "YOUR_16_CHAR_APP_PASSWORD",
  "SenderEmail": "YOUR_GMAIL@gmail.com",
  "SenderName": "Sector 13 Welfare Society"
}
```

## Step 4: Test
1. Restart the application
2. Try forgot password
3. Check your email inbox

## Troubleshooting
- Make sure you used the App Password, not your regular Gmail password
- Check spam/junk folder
- Ensure 2-Step Verification is enabled 