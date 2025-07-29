# SMS Setup Guide for Bangladesh

## Overview
This guide will help you set up SMS notifications for the Sector 13 Welfare Society Digital Management System using popular Bangladesh SMS gateways.

## Supported SMS Providers

### 1. BulkSmsBD (Recommended)
- **Website**: https://bulksmsbd.net
- **API Documentation**: https://bulksmsbd.net/api
- **Features**: Reliable, good delivery rates, competitive pricing

### 2. SSL Wireless
- **Website**: https://sslwireless.com
- **API Documentation**: https://sslwireless.com/api
- **Features**: Enterprise-grade, high delivery rates

### 3. BulkSmsBD Alternative
- **Website**: https://bulksmsbd.net
- **Features**: Alternative API format for the same provider

## Step 1: Choose Your SMS Provider

### Option A: BulkSmsBD (Most Popular)
1. Visit https://bulksmsbd.net
2. Create an account
3. Add funds to your account
4. Get your API credentials:
   - API Key
   - API Secret
   - Sender ID (pre-approved)

### Option B: SSL Wireless
1. Visit https://sslwireless.com
2. Contact sales for enterprise account
3. Get your API credentials:
   - API Token
   - Sender ID (SID)

## Step 2: Update Configuration

### Update `appsettings.json`:

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

### For SSL Wireless:
```json
{
  "SmsSettings": {
    "ApiKey": "your-ssl-wireless-api-token",
    "ApiSecret": "",
    "SenderId": "your-approved-sender-id",
    "ApiUrl": "https://smsplus.sslwireless.com/api/v3/send-sms",
    "IsEnabled": true,
    "Provider": "SSLWireless"
  }
}
```

## Step 3: Test SMS Configuration

### Test Single SMS
1. Go to your application
2. Navigate to Admin Dashboard
3. Use the SMS test functionality
4. Send a test SMS to your phone number

### Test Notice Approval
1. Create a test notice as a Manager
2. Approve the notice as a Secretary
3. Check if SMS notifications are sent to members

## Step 4: SMS Message Format

The system automatically formats SMS messages:

```
নোটিশ: [Notice Title]
[Notice Content - truncated to 100 characters]
সেক্টর ১৩ ওয়েলফেয়ার সোসাইটি
```

**Message Length**: Maximum 160 characters (SMS limit)

## Step 5: Phone Number Format

The system automatically formats Bangladesh phone numbers:
- `01712345678` → `+8801712345678`
- `8801712345678` → `+8801712345678`
- `+8801712345678` → `+8801712345678`

## Troubleshooting

### Common Issues:

1. **SMS Not Sending**
   - Check if SMS service is enabled (`IsEnabled: true`)
   - Verify API credentials
   - Check account balance
   - Review API response logs

2. **Invalid Sender ID**
   - Contact your SMS provider to approve your sender ID
   - Use a pre-approved sender ID

3. **Phone Number Issues**
   - Ensure members have valid phone numbers
   - Check phone number format
   - Verify country code (+880 for Bangladesh)

4. **Rate Limiting**
   - The system includes 100ms delay between SMS
   - Contact provider for higher rate limits

### Debug Information:

Check application logs for:
- SMS API responses
- Phone number formatting
- Success/failure counts
- Error messages

## SMS Provider Comparison

| Provider | Pros | Cons | Best For |
|----------|------|------|----------|
| BulkSmsBD | Reliable, good rates, easy setup | Limited features | Small to medium organizations |
| SSL Wireless | High delivery, enterprise features | Higher cost, complex setup | Large organizations |

## Cost Estimation

### BulkSmsBD Pricing (Approximate):
- **Local SMS**: ৳0.50 - ৳1.00 per SMS
- **International SMS**: ৳2.00 - ৳5.00 per SMS
- **Bulk Discounts**: Available for large volumes

### Example Cost Calculation:
- 100 members = 100 SMS per notice
- 10 notices per month = 1,000 SMS
- Cost: ৳500 - ৳1,000 per month

## Security Considerations

1. **API Credentials**: Never commit API keys to version control
2. **Environment Variables**: Use environment variables for production
3. **Rate Limiting**: Implement proper rate limiting
4. **Logging**: Monitor SMS usage and costs

## Production Setup

### Environment Variables:
```bash
# Set these in your production environment
SmsSettings__ApiKey=your-production-api-key
SmsSettings__ApiSecret=your-production-api-secret
SmsSettings__SenderId=SEC13
SmsSettings__IsEnabled=true
```

### Monitoring:
- Set up alerts for SMS failures
- Monitor SMS costs
- Track delivery rates
- Review usage patterns

## Support

For technical support:
1. Check application logs
2. Review SMS provider documentation
3. Contact your SMS provider
4. Check system configuration

## Testing Checklist

- [ ] SMS service enabled in configuration
- [ ] Valid API credentials
- [ ] Approved sender ID
- [ ] Account has sufficient balance
- [ ] Test SMS sent successfully
- [ ] Notice approval triggers SMS
- [ ] Members receive notifications
- [ ] Phone numbers formatted correctly
- [ ] Message content within limits
- [ ] Error handling works properly 