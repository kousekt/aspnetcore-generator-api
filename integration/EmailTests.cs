using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace integration
{
    public class EmailTests
    {
        // the "docker.compose" file in the "api" project has a service called "generator".
        // that is the DNS name that we use for this.
        public const string GeneratorApiRoot = "http://generator";

        // the "docker.compose" file in the "api" project has a service called "mail".
        // that is the DNS name that we use for this.
        public const string MailHogApiV2Root = "http://mail:8025/api/v2";

        [Fact]
        public async Task SendEmailWithNames_IsFromGenerator()
        {
            // send email
            var client = new HttpClient();
            var sendEmail = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{GeneratorApiRoot}/EmailRandomNames")
            };
            Console.WriteLine($"Sending email: {sendEmail.RequestUri}");
            using (var response = await client.SendAsync(sendEmail))
            {
                // this EnsureSeccessStatusCode() will throw an exception if the response
                // was not successful.
                response.EnsureSuccessStatusCode();
            }

            // check if email (the generate above will generate the emails)
            var checkEmails = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{MailHogApiV2Root}/messages")
            };
            Console.WriteLine($"Checking emails: {checkEmails.RequestUri}");
            using (var response = await client.SendAsync(checkEmails))
            {
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var messages = JObject.Parse(content);
                // should be just 1 message
                messages.Should().HaveElement("total").Which.Should().Be(1);
                messages.Should().HaveElement("items")
                    .Which.Should().BeOfType<JArray>()
                    // first message should have a Raw element
                    .Which.First.Should().HaveElement("Raw")
                    .Which.Should().HaveElement("From")
                    .Which.Should().Be("generator@generate.com");
            }
        }
    }
}