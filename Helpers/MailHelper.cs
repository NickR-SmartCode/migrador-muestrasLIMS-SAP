using DTO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Helpers
{
    public class MailHelper
    {
        private readonly List<string> _cc = new List<string>();
        private readonly List<string> _cco = new List<string>();
        private readonly List<string> _adjuntos = new List<string>();

        private bool _reemplazarHTML = false;
        private string _EstilosAdicionales = "";

        public MailHelper ConCC(List<string> lista)
        {
            _cc.AddRange(lista);
            return this;
        }
        public MailHelper ConCCO(List<string> lista)
        {
            _cco.AddRange(lista);
            return this;
        }

        public MailHelper ConAdjunto(string ruta)
        {
            _adjuntos.Add(ruta);
            return this;
        }
        public MailHelper AgregarEstilosAdicionales(string css)
        {
            _EstilosAdicionales = (css);
            return this;
        }

        public async Task<bool> EnviarEmail(ConfiguracionDTO config, string Asunto, List<string> Destinatarios, string Cuerpo)
        {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(config.EmailSMTP, Asunto);

                // Destinatarios
                foreach (var p in Destinatarios)
                    mail.To.Add(p);

                // Con Copia
                foreach (var c in _cc)
                    mail.CC.Add(c);

                // Con Copia Oculta
                foreach (var cco in _cco)
                    mail.Bcc.Add(cco);

                mail.Subject = Asunto;
                mail.IsBodyHtml = true;

                if (_reemplazarHTML)
                {
                    mail.Body = Cuerpo;
                }
                else
                {
                    mail.Body = TemplateCuerpoHTML(Cuerpo);

                }

                // Adjuntos
                foreach (string archivo in _adjuntos)
                {
                    if (File.Exists(archivo))
                        mail.Attachments.Add(new Attachment(archivo));
                }

                // Configuración SMTP
                SmtpClient client = new SmtpClient(config.ServidorSMTP, config.PuertoSMTP);
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(config.EmailSMTP, config.PasswordSMTP);
                client.EnableSsl = true;

                //ServicePointManager.ServerCertificateValidationCallback =
                //    (s, cert, chain, sslPolicyErrors) => true;

                await client.SendMailAsync(mail);

                mail.Dispose();
                client.Dispose();

                return true;
           
        }

        private string TemplateCuerpoHTML(string Complemento)
        {
            string Plantilla = $@"
                <!DOCTYPE html>
                <html lang=""es"">
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <link rel=""preconnect"" href=""https://fonts.googleapis.com"">
                    <link rel=""preconnect"" href=""https://fonts.gstatic.com"" crossorigin>
                    <link href=""https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;700&display=swap"" rel=""stylesheet"">
                    <title>Confirmación de Recepción de Documentos</title>
                    <style>
                        body {{
                            margin: 0;
                            padding: 0;
                        }}
                        .email-container {{
                            max-width: 600px;
                            margin: 20px auto;
                            border: 1px solid #dddddd;
                            border-radius: 8px;
                            overflow: hidden;
                 background-color: #f4f7f6;
                            font-family: 'Outfit', Arial, sans-serif;
                            color: #333333;
                        }}
                        .header {{
                            background-color: #004682;
                            color: #ffffff;
                            text-align: center;
                            padding: 40px 20px;
                        }}
                        .header img {{
                           height: 90px;
                        }}
                        .header h1 {{
                            margin: 0;
                            font-size: 28px;
                            font-weight: 700;
                        }}
                        .header p {{
                            margin: 5px 0 0;
                            font-size: 16px;
                            font-weight: 300;
                        }}
                        .content {{
                            padding: 30px 40px;
                            line-height: 1.6;
                            text-align: left;
                        }}
                        .content p {{
                            margin: 0 0 15px;
                        }}
                       
                        .footer {{
                            background-color: #004682;
                            color: #d1e3f3;
                            padding: 25px 30px;
                            font-size: 13px;
                            text-align: center;
                        }}
                        .footer p {{
                            margin: 0;
                        }}
                        .footer .auto-reply-notice {{
                            font-size: 12px;
                            margin-top: 10px;
                            opacity: 0.8;
                        }}
                        {{ESTILOS}}
                    </style>
                </head>
                <body>
                    <div class=""email-container"">
                         <div class=""header"">
                           <a><img src='https://smartcode.pe/demo/img/smartcode-logo.png' alt='logo'/></a>
                            </div>
                        <div class=""content"">
                            {{CUERPO}}
                        </div>
                        <div class=""footer"">
                            <p>&copy; " + DateTime.Now.Year + @" SmartCode. Todos los derechos reservados.</p>
                            <p class=""auto-reply-notice"">Este es un correo electrónico generado automáticamente, por favor no responda a este mensaje.</p>
                        </div>
                    </div>
                </body>
                </html>";

            Plantilla = Plantilla.Replace("{CUERPO}", Complemento);
            Plantilla = Plantilla.Replace("{ESTILOS}", _EstilosAdicionales);

            return Plantilla;

        }
    }
}
