using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Controllers.Shared;

public class DemoController : ApiControllerBase
{
    private readonly IConfiguration _config;
    private readonly IEmailService _email;
    private readonly IBackgroundTaskQueue _queue;
    private readonly ILogger<DemoController> _logger;

    public DemoController(IConfiguration config, IEmailService email, IBackgroundTaskQueue queue, ILogger<DemoController> logger)
    {
        _config = config;
        _email = email;
        _queue = queue;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("schedule-demo")]
    public async Task<ActionResult<ScheduleDemoResponseDto>> ScheduleDemo([FromBody] ScheduleDemoRequestDto dto, CancellationToken ct)
    {
        if (dto == null) return BadRequest(new ScheduleDemoResponseDto { Success = false, Message = "Invalid request." });
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest(new ScheduleDemoResponseDto { Success = false, Message = "Name is required." });
        if (string.IsNullOrWhiteSpace(dto.Phone)) return BadRequest(new ScheduleDemoResponseDto { Success = false, Message = "Phone is required." });
        if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains('@')) return BadRequest(new ScheduleDemoResponseDto { Success = false, Message = "Valid email is required." });
        if (string.IsNullOrWhiteSpace(dto.InstitutionName)) return BadRequest(new ScheduleDemoResponseDto { Success = false, Message = "Institution name is required." });

        var adminEmails = _config.GetSection("AdminEmails").Get<string[]>() ?? Array.Empty<string>();
        var creds = _config.GetSection("Credentials").Get<DemoCredentials>() ?? new DemoCredentials();

        var now = DateTimeOffset.UtcNow;
        var subjectAdmin = $"New Demo Request - {dto.InstitutionName}";
        var subjectUser = "Welcome to Aduora - Demo Scheduled";

        var adminBody = $@"
<div style='font-family:Arial,sans-serif;background:#f6f7fb;padding:30px'>
  <table style='max-width:650px;margin:auto;background:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #eee'>
    
    <tr>
      <td style='background:#1e3a8a;color:white;padding:20px'>
        <h2 style='margin:0'>New Demo Request</h2>
      </td>
    </tr>

    <tr>
      <td style='padding:20px;color:#333'>
        A new user has submitted a demo request from the Aduora website.
      </td>
    </tr>

    <tr>
      <td style='padding:0 20px 20px'>
        <table style='width:100%;border-collapse:collapse;font-size:14px'>
          
          <tr>
            <td style='padding:10px;border:1px solid #eee;font-weight:bold'>Name</td>
            <td style='padding:10px;border:1px solid #eee'>{System.Net.WebUtility.HtmlEncode(dto.Name)}</td>
          </tr>

          <tr>
            <td style='padding:10px;border:1px solid #eee;font-weight:bold'>Phone</td>
            <td style='padding:10px;border:1px solid #eee'>{System.Net.WebUtility.HtmlEncode(dto.Phone)}</td>
          </tr>

          <tr>
            <td style='padding:10px;border:1px solid #eee;font-weight:bold'>Email</td>
            <td style='padding:10px;border:1px solid #eee'>{System.Net.WebUtility.HtmlEncode(dto.Email)}</td>
          </tr>

          <tr>
            <td style='padding:10px;border:1px solid #eee;font-weight:bold'>Institution</td>
            <td style='padding:10px;border:1px solid #eee'>{System.Net.WebUtility.HtmlEncode(dto.InstitutionName)}</td>
          </tr>

          <tr>
            <td style='padding:10px;border:1px solid #eee;font-weight:bold'>Institution Type</td>
            <td style='padding:10px;border:1px solid #eee'>{System.Net.WebUtility.HtmlEncode(dto.InstitutionType)}</td>
          </tr>

          <tr>
            <td style='padding:10px;border:1px solid #eee;font-weight:bold'>Board</td>
            <td style='padding:10px;border:1px solid #eee'>{System.Net.WebUtility.HtmlEncode(dto.BoardType)}</td>
          </tr>

          <tr>
            <td style='padding:10px;border:1px solid #eee;font-weight:bold'>City & State</td>
            <td style='padding:10px;border:1px solid #eee'>{System.Net.WebUtility.HtmlEncode(dto.CityState)}</td>
          </tr>

          <tr>
            <td style='padding:10px;border:1px solid #eee;font-weight:bold'>Students</td>
            <td style='padding:10px;border:1px solid #eee'>{System.Net.WebUtility.HtmlEncode(dto.StudentStrength)}</td>
          </tr>

          <tr>
            <td style='padding:10px;border:1px solid #eee;font-weight:bold'>Agree to Contact</td>
            <td style='padding:10px;border:1px solid #eee'>{(dto.AgreeToContact ? "Yes" : "No")}</td>
          </tr>

          <tr>
            <td style='padding:10px;border:1px solid #eee;font-weight:bold'>Submitted</td>
            <td style='padding:10px;border:1px solid #eee'>{now:yyyy-MM-dd HH:mm:ss} UTC</td>
          </tr>

        </table>
      </td>
    </tr>

    <tr>
      <td style='background:#f9fafc;text-align:center;padding:15px;color:#888;font-size:13px'>
        Aduora School ERP
      </td>
    </tr>

  </table>
</div>";

        var userBody = $@"
<!DOCTYPE html>
<html>
<head>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>

<body style='margin:0;padding:0;background:#f6f7fb;font-family:Arial,sans-serif;'>

<table width='100%' cellpadding='0' cellspacing='0' style='background:#f6f7fb;padding:20px'>
<tr>
<td align='center'>

<table width='100%' cellpadding='0' cellspacing='0' 
style='max-width:650px;background:#ffffff;border-radius:12px;border:1px solid #eee;overflow:hidden'>

<!-- HEADER -->

<tr>
<td style='background:#facc15;padding:25px;text-align:center'>
<h2 style='margin:0;color:#1e3a8a;font-size:24px'>Welcome to Aduora</h2>
</td>
</tr>

<!-- CONTENT -->

<tr>
<td style='padding:25px;color:#333;font-size:15px;line-height:1.6'>

<p>Hi <b>{System.Net.WebUtility.HtmlEncode(dto.Name)}</b>,</p>

<p>
Thank you for scheduling a demo for <b>Aduora School ERP</b>. 
Our team will contact you shortly to guide you through the platform.
</p>

<p>You can start exploring the demo using the credentials below:</p>

<!-- LOGIN TABLE -->

<table width='100%' cellpadding='0' cellspacing='0' style='border-collapse:collapse;margin-top:15px;font-size:14px'>

<tr>
<td style='padding:10px;border:1px solid #eee;background:#fafafa;font-weight:bold;width:160px'>
Login URL
</td>

<td style='padding:10px;border:1px solid #eee'>
<a href='https://school.aduora.com/login' style='color:#1e3a8a;text-decoration:none'>
https://school.aduora.com/login
</a>
</td>
</tr>

<tr>
<td style='padding:10px;border:1px solid #eee;background:#fafafa;font-weight:bold'>
Username
</td>

<td style='padding:10px;border:1px solid #eee'>
{System.Net.WebUtility.HtmlEncode(creds.Username)}
</td>
</tr>

<tr>
<td style='padding:10px;border:1px solid #eee;background:#fafafa;font-weight:bold'>
Password
</td>

<td style='padding:10px;border:1px solid #eee'>
{System.Net.WebUtility.HtmlEncode(creds.Password)}
</td>
</tr>

</table>

<!-- IMPORTANT NOTE -->

<table width='100%' cellpadding='0' cellspacing='0' style='margin-top:20px'>
<tr>
<td style='padding:15px;background:#fff8e1;border-left:4px solid #facc15;border-radius:6px;font-size:14px'>

<b>Important:</b><br>
Admin panel login is supported only on 
<b>Desktop / Laptop browsers</b> or the <b>Aduora mobile application</b>.  
Mobile web browsers are currently not supported for admin access.

</td>
</tr>
</table>

<!-- CONTACT -->

<table width='100%' cellpadding='0' cellspacing='0' style='margin-top:20px'>
<tr>
<td style='padding:15px;background:#f0f7ff;border-left:4px solid #1e3a8a;border-radius:6px;font-size:14px'>

<b>Need Help?</b><br>
If you want to contact our team directly, feel free to call:

<br><br>

<b>Ritesh Deswal</b><br>

<a href='tel:8847336284' style='color:#1e3a8a;font-weight:bold;text-decoration:none'>
📞 8847336284
</a>

</td>
</tr>
</table>

<!-- BUTTON -->

<table width='100%' cellpadding='0' cellspacing='0' style='margin-top:25px'>
<tr>
<td align='center'>

<a href='https://school.aduora.com/login'
style='background:#facc15;color:#1e3a8a;padding:14px 28px;
text-decoration:none;border-radius:6px;font-weight:bold;display:inline-block'>

Login to Demo

</a>

</td>
</tr>
</table>

<p style='margin-top:25px'>
If you have any questions, feel free to reply to this email.
</p>

<p>
Regards,<br>
<b>Aduora Team</b>
</p>

</td>
</tr>

<!-- FOOTER -->

<tr>
<td style='background:#f9fafc;text-align:center;padding:15px;color:#888;font-size:13px'>

© {DateTime.UtcNow.Year} Aduora School ERP

</td>
</tr>

</table>

</td>
</tr>
</table>

</body>
</html>";

        var dtoSnapshot = dto;
        _queue.Enqueue(async (stoppingToken) =>
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(35));
            try
            {
                if (adminEmails.Length > 0)
                {
                    await _email.SendManyAsync(adminEmails, subjectAdmin, adminBody, timeoutCts.Token);
                }
                await _email.SendAsync(dtoSnapshot.Email.Trim(), subjectUser, userBody, timeoutCts.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send schedule demo emails");
            }
        });

        return Ok(new ScheduleDemoResponseDto { Success = true, Message = "Demo request submitted. Please check your email shortly." });
    }
}
