namespace FamilyTree.Services.Email;

/// <summary>
/// Builds branded HTML email bodies for all Rodoslov email types.
///
/// To add a new email template:
///   1. Add a public static method returning (string Subject, string HtmlBody).
///   2. Call Layout(body, footerReason) for the outer wrapper.
///   3. Add the corresponding method to IEmailService + EmailService.
///
/// All user-provided values must go through Encode() before insertion into HTML.
/// </summary>
internal static class EmailTemplates
{
    // -------------------------------------------------------------------------
    // Public template methods
    // -------------------------------------------------------------------------

    public static (string Subject, string HtmlBody) Invitation(
        string familyName,
        string? inviterName,
        string roleName,
        string acceptUrl)
    {
        string subject = $"Pozivnica za porodično stablo — {familyName}";

        string inviterLine = inviterName != null
            ? $@"<p style=""margin:0 0 16px;font-size:15px;color:#716A62;line-height:1.6;"">
                    Pozivnicu vam je uputio/la
                    <strong style=""color:#1C1C1C;"">{Encode(inviterName)}</strong>.
                 </p>"
            : string.Empty;

        string body = $@"
            <h1 style=""margin:0 0 8px;font-size:22px;font-weight:bold;color:#2D4739;
                        font-family:'Georgia',serif;line-height:1.3;"">
                Pozvani ste u porodično stablo
            </h1>
            <div style=""width:48px;height:3px;background-color:#B8965A;margin:0 0 24px;""></div>

            <p style=""margin:0 0 16px;font-size:15px;line-height:1.7;color:#1C1C1C;"">
                Pozvani ste da se pridružite porodici
                <strong style=""color:#2D4739;"">{Encode(familyName)}</strong>
                na platformi Rodoslov.
            </p>

            {inviterLine}

            <p style=""margin:0 0 32px;font-size:15px;color:#1C1C1C;"">
                Vaša uloga:&nbsp;
                <span style=""display:inline-block;padding:4px 14px;
                              background-color:#F8F4EE;border:1px solid #E5DDD4;
                              border-radius:4px;color:#2D4739;font-weight:bold;font-size:13px;"">
                    {Encode(roleName)}
                </span>
            </p>

            {Button("Prihvati pozivnicu", acceptUrl)}

            <p style=""margin:28px 0 0;font-size:12px;color:#716A62;line-height:1.7;"">
                Ako dugme ne radi, kopirajte ovaj link u pretraživač:<br>
                <a href=""{acceptUrl}"" style=""color:#2D4739;word-break:break-all;"">{acceptUrl}</a>
            </p>
            <p style=""margin:12px 0 0;font-size:12px;color:#B8965A;"">
                Pozivnica ističe za 7 dana.
            </p>";

        string footerReason =
            $"Dobili ste ovaj email jer je vaša adresa korišćena pri slanju pozivnice " +
            $"za porodicu <strong>{Encode(familyName)}</strong> na platformi Rodoslov. " +
            $"Ako niste očekivali ovu poruku, možete je ignorisati.";

        return (subject, Layout(body, footerReason));
    }

    public static (string Subject, string HtmlBody) Welcome(string firstName)
    {
        string subject = "Dobrodošli na Rodoslov";

        string body = $@"
            <h1 style=""margin:0 0 8px;font-size:22px;font-weight:bold;color:#2D4739;
                        font-family:'Georgia',serif;line-height:1.3;"">
                Dobrodošli, {Encode(firstName)}!
            </h1>
            <div style=""width:48px;height:3px;background-color:#B8965A;margin:0 0 24px;""></div>

            <p style=""margin:0 0 16px;font-size:15px;line-height:1.7;color:#1C1C1C;"">
                Vaš nalog na platformi <strong>Rodoslov</strong> je uspješno kreiran.
            </p>

            <p style=""margin:0 0 16px;font-size:15px;line-height:1.7;color:#1C1C1C;"">
                Rodoslov vam omogućava da istražujete, čuvate i dijelite istoriju vaše porodice.
                Kreirajte porodično stablo, dodajte članove, povežite generacije i sačuvajte priče
                koje ne smiju biti zaboravljene.
            </p>

            <p style=""margin:0 0 32px;font-size:15px;line-height:1.7;color:#1C1C1C;"">
                Počnite tako što ćete kreirati svoju prvu porodicu.
            </p>

            {Button("Otvori Rodoslov", "#")}

            <p style=""margin:28px 0 0;font-size:13px;color:#716A62;line-height:1.7;"">
                Ako imate pitanja, slobodno nas kontaktirajte.
            </p>";

        string footerReason =
            "Dobili ste ovaj email jer je upravo kreiran nalog na platformi Rodoslov " +
            "sa ovom email adresom. Ako niste vi otvorili nalog, kontaktirajte nas.";

        return (subject, Layout(body, footerReason));
    }

    // -------------------------------------------------------------------------
    // Shared building blocks
    // -------------------------------------------------------------------------

    private static string Layout(string body, string footerReason) => $@"<!DOCTYPE html>
<html lang=""sr"">
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
  <title>Rodoslov</title>
</head>
<body style=""margin:0;padding:0;background-color:#F8F4EE;"">

  <!-- Outer wrapper -->
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0""
         style=""background-color:#F8F4EE;padding:48px 16px;"">
    <tr>
      <td align=""center"" valign=""top"">

        <!-- Card (max 600px, 100% on mobile) -->
        <table cellpadding=""0"" cellspacing=""0"" border=""0""
               style=""max-width:600px;width:100%;background-color:#FFFFFF;
                      border-radius:12px;border:1px solid #E5DDD4;"">

          <!-- Brand header -->
          <tr>
            <td style=""background-color:#2D4739;border-radius:12px 12px 0 0;
                        padding:28px 40px;text-align:center;"">
              <p style=""margin:0;font-size:11px;letter-spacing:4px;
                        color:#B8965A;font-family:Arial,sans-serif;"">
                ✦&nbsp;&nbsp;✦&nbsp;&nbsp;✦
              </p>
              <p style=""margin:8px 0 0;font-size:28px;font-weight:bold;letter-spacing:2px;
                        color:#D4B07A;font-family:'Georgia',serif;"">
                Rodoslov
              </p>
              <p style=""margin:6px 0 0;font-size:11px;letter-spacing:3px;text-transform:uppercase;
                        color:#B8965A;font-family:Arial,sans-serif;"">
                Porodično stablo
              </p>
            </td>
          </tr>

          <!-- Body -->
          <tr>
            <td style=""padding:40px 40px 36px;font-family:Arial,sans-serif;"">
              {body}
            </td>
          </tr>

          <!-- Divider -->
          <tr>
            <td style=""padding:0 40px;"">
              <div style=""height:1px;background-color:#E5DDD4;""></div>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style=""padding:20px 40px 32px;text-align:center;font-family:Arial,sans-serif;"">
              <p style=""margin:0 0 10px;font-size:11px;color:#B8965A;line-height:1.6;"">
                {footerReason}
              </p>
              <p style=""margin:0;font-size:12px;color:#716A62;"">
                © 2026 Rodoslov &nbsp;·&nbsp; Čuvamo istoriju vaše porodice
              </p>
            </td>
          </tr>

        </table>
        <!-- End card -->

      </td>
    </tr>
  </table>

</body>
</html>";

    private static string Button(string label, string url) => $@"
        <table cellpadding=""0"" cellspacing=""0"" border=""0"">
          <tr>
            <td style=""background-color:#2D4739;border-radius:8px;
                        box-shadow:0 2px 8px rgba(45,71,57,0.25);"">
              <a href=""{url}""
                 target=""_blank""
                 style=""display:inline-block;padding:15px 36px;color:#FFFFFF;
                        text-decoration:none;font-size:15px;font-weight:bold;
                        font-family:Arial,sans-serif;letter-spacing:0.5px;
                        border-radius:8px;"">
                {label} &rarr;
              </a>
            </td>
          </tr>
        </table>";

    /// <summary>
    /// Basic HTML encoding for user-provided values rendered inside email HTML.
    /// </summary>
    private static string Encode(string value) =>
        value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
}
