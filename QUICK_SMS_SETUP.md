# Quick SMS Setup Guide

## 🚀 Quick Start (5 Minutes)

### Step 1: Choose SMS Provider
**Recommended: BulkSmsBD**
- Visit: https://bulksmsbd.net
- Create account and add funds
- Get API credentials

### Step 2: Update Configuration
Edit `appsettings.json`:

```json
{
  "SmsSettings": {
    "ApiKey": "your-actual-api-key",
    "ApiSecret": "your-actual-api-secret", 
    "SenderId": "SEC13",
    "ApiUrl": "https://bulksmsbd.net/api/smsapi",
    "IsEnabled": true,
    "Provider": "BulkSmsBD"
  }
}
```

### Step 3: Test SMS
1. Login as Admin
2. Go to Notice Management → Test SMS
3. Send test SMS to your phone
4. Verify delivery

### Step 4: Test Notice Approval
1. Create notice as Manager
2. Approve notice as Secretary
3. Check if members receive SMS

## 📱 SMS Features

✅ **Automatic SMS on Notice Approval**
- All members get SMS when notice is approved
- Bengali message format
- Phone number auto-formatting

✅ **Test SMS Panel**
- Single SMS testing
- Bulk SMS testing
- Member phone number display

✅ **Multiple Provider Support**
- BulkSmsBD (Recommended)
- SSL Wireless
- Alternative APIs

## 💰 Cost Estimation

**BulkSmsBD Pricing:**
- Local SMS: ৳0.50 - ৳1.00 per SMS
- 100 members = 100 SMS per notice
- 10 notices/month = ৳500 - ৳1,000

## 🔧 Troubleshooting

**SMS Not Sending?**
1. Check `IsEnabled: true`
2. Verify API credentials
3. Check account balance
4. Review application logs

**Phone Number Issues?**
- System auto-formats: `01712345678` → `+8801712345678`
- Ensure members have valid phone numbers

## 📞 Support

- Check `SMS_SETUP_GUIDE.md` for detailed instructions
- Review application logs for errors
- Contact SMS provider for API issues

## 🎯 Next Steps

1. Configure SMS credentials
2. Test single SMS
3. Test notice approval workflow
4. Monitor SMS delivery rates
5. Set up cost monitoring 