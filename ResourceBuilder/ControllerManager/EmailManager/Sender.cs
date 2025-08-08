
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ResourceBuilder.Shared.Modal;

//using RazorEngine.Compilation.ImpromptuInterface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ResourceBuilder.ControllerManager.EmailManager
{
    [Obsolete("Eliminar")]
    public class Sender
    {
        static bool mailSent = false;
        //private void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        //{
        //    // Get the unique identifier for this asynchronous operation.
        //    //String token = (string)e.UserState;
        //    dynamic dataParams = (ExpandoObject)e.UserState;
        //    String token = dataParams.email_id.ToString();
        //    EmailQueue<object> dynamicEntity = new EmailQueue<object>();

        //    if (e.Cancelled)
        //    {
        //        Console.WriteLine("[{0}] Send canceled.", token);
        //    }
        //    if (e.Error != null)
        //    {
        //        Console.WriteLine("[{0}] {1}", token, e.Error.ToString());
        //        LogManager.Log().LogError(e.Error.ToString());

        //        dataParams.status = 30;
        //        dynamicEntity.SetStatus(dataParams);
        //    }
        //    else
        //    {
        //        Console.WriteLine("Message sent.");
        //        //LogManager.Log().LogError("");

        //        dataParams.status = 20;
        //        dynamicEntity.SetStatus(dataParams);
        //        //Change email database value
        //    }
        //    mailSent = true;
        //}
        //public async Task<bool> Send(dynamic data, dynamic dataEmail, List<AttachItem> forAttach)
        //{
        //    //string host = "smtp.gmail.com";
        //    //int port = 587;
        //    //string userName = "ronald.chonillo.store@gmail.com";
        //    //string password = "YESenia666666";
        //    //bool EnableSsl = true;
        //    //bool UseDefaultCredentials = false;

        //    //// Command-line argument must be the SMTP host.
        //    //SmtpClient client = new SmtpClient(host, port)
        //    //{
        //    //    EnableSsl = EnableSsl,
        //    //    DeliveryMethod = SmtpDeliveryMethod.Network,
        //    //    UseDefaultCredentials = UseDefaultCredentials,
        //    //    Credentials = new NetworkCredential(userName, password)
        //    //};
            
        //    data.tax_id = data.ruc;
        //    SmtpClient client = await CreateSmtpClient(data);
            
        //    if (client == null)
        //    {
        //        LogManager.Log().LogError("No smtp settings found for " + $"{data.tax_id}");
        //        return false;
        //    }
            
        //    // Specify the email sender.
        //    // Create a mailing address that includes a UTF8 character
        //    // in the display name.
        //    MailAddress from = new MailAddress(
        //        client.Credentials.GetCredential(client.Host, client.Port, "").UserName, //"ronald.chonillo.store@gmail.com",
        //        data.razonSocial,
        //        System.Text.Encoding.UTF8);

        //    // Set destinations for the email message.
        //    MailAddress to = new MailAddress(data.to_email);

        //    // Specify the message content.
        //    MailMessage message = new MailMessage(from, to);

        //    string message_id = Guid.NewGuid().ToString();

        //    //#Fix Bug 33
        //    message.Headers.Add("Message-Id", String.Format("<{0}@{1}>", message_id.ToString(), "mail.beebtech.net"));
        //    //#

        //    if (!(data.cc_email is DBNull) && data.cc_email != "")
        //    {
        //        MailAddress cc = new MailAddress(data.cc_email);
        //        message.CC.Add(cc);
        //    }
            
        //    foreach (AttachItem itemAttach in forAttach)
        //    {
        //        Stream stream = new MemoryStream(itemAttach.data);
        //        message.Attachments.Add(new Attachment(stream, itemAttach.name, itemAttach.mediaType));
        //    }

        //    //string strBody = HtmlMaker.Make(Template.GetForBody(0), 
        //    //    "emailBody",
        //    //    data
        //    //    );

        //    string strBody = HtmlMaker.Make(Template.GetDocument(data.ruc, Template.DocType.email, Template.DocName.html_body_fac),
        //        BeebTools.StringTools.RandomString(10),
        //        data
        //        );

        //    message.Body = strBody;
        //    message.IsBodyHtml = true;

        //    //message.Attachments.Add(new Attachment(), "");

        //    // Include some non-ASCII characters in body and subject.
        //    //string someArrows = new string(new char[] { '\u2190', '\u2191', '\u2192', '\u2193' });
        //    message.Body += Environment.NewLine; // + someArrows;
        //    message.BodyEncoding = System.Text.Encoding.UTF8;

        //    string FacNo = data.estab + "-" + data.ptoEmi + "-" + data.secuencial.ToString().PadLeft(9, '0');
        //    message.Subject = "Factura Electrónica " + FacNo;

        //    message.SubjectEncoding = System.Text.Encoding.UTF8;
        //    // Set the method that is called back when the send operation ends.
        //    client.SendCompleted += new
        //    SendCompletedEventHandler(SendCompletedCallback);

        //    // The userState can be any object that allows your callback
        //    // method to identify this send operation.
        //    // For this example, the userToken is a string constant.
        //    //string userState = data.email_id.ToString();
            
        //    client.SendAsync(message, dataEmail);

        //    //System.Threading.CancellationToken ct = new System.Threading.CancellationToken();            
        //    //await client.SendMailAsync(message, dataEmail);
        //    //client.SendAsync(message, data);
        //    //client.SendMailAsync(message);
        //    //Console.WriteLine("Sending message... press c to cancel mail. Press any other key to exit.");
        //    //string answer = Console.ReadLine();
        //    //// If the user canceled the send, and mail hasn't been sent yet,
        //    //// then cancel the pending operation.
        //    //if (answer.StartsWith("c") && mailSent == false)
        //    //{
        //    //    client.SendAsyncCancel();
        //    //}
        //    // Clean up.
        //    //message.Dispose();
        //    //Console.WriteLine("Goodbye.");
        //    return true;
        //}

        public async Task<SmtpClient> CreateSmtpClient(dynamic itemData)
        {
            SmtpClient clientResult = null;

            //EmailQueue<dynamic> emailQueue = new EmailQueue<dynamic>();
            //CoreResponse response = await emailQueue.GetSettingByStatus(itemData);
            EmailSettings setting = new EmailSettings();
            
            string host = setting.host;
            int port = setting.port;
            string userName = setting.userName;
            string password = setting.password;
            bool EnableSsl = Convert.ToBoolean(setting.EnableSsl);
            bool UseDefaultCredentials = Convert.ToBoolean(setting.UseDefaultCredentials);

            // Command-line argument must be the SMTP host.
            clientResult = new SmtpClient(host, port)
            {
                EnableSsl = EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = UseDefaultCredentials,
                Credentials = new NetworkCredential(userName, password)
            };            

            return clientResult;
        }

        public bool TestSmtpV2(EmailSettings setting, string emailTo)
        {
            string host = setting.host;
            int port = setting.port;
            string userName = setting.userName;
            string password = setting.password;
            bool EnableSsl = Convert.ToBoolean(setting.EnableSsl);
            bool UseDefaultCredentials = Convert.ToBoolean(setting.UseDefaultCredentials);

            // Command-line argument must be the SMTP host.
            SmtpClient client = new SmtpClient(host, port)
            {
                EnableSsl = EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = UseDefaultCredentials,
                Credentials = new NetworkCredential(userName, password)
            };


            try
            {
                client.Send(setting.userName,
                emailTo,
                "test",
                "testbody");

                //await client.SendMailAsync(setting.userName,
                //    emailTo,
                //    "test",
                //    "testbody");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        [Obsolete]
        public bool TestSmtp(dynamic setting, string emailTo)
        {            
            string host = setting.host;
            int port = setting.port;
            string userName = setting.userName;
            string password = setting.password;
            bool EnableSsl = Convert.ToBoolean(setting.EnableSsl);
            bool UseDefaultCredentials = Convert.ToBoolean(setting.UseDefaultCredentials);

            // Command-line argument must be the SMTP host.
            SmtpClient client = new SmtpClient(host, port)
            {
                EnableSsl = EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = UseDefaultCredentials,
                Credentials = new NetworkCredential(userName, password)
            };

            
            try
            {
                client.Send(setting.userName,
                emailTo,
                "test",
                "testbody");

                //await client.SendMailAsync(setting.userName,
                //    emailTo,
                //    "test",
                //    "testbody");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }
    }
 }


