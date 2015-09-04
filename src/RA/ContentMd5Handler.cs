using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RA
{
    public class ContentMd5Handler : DelegatingHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            if (request.Content == null)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            var content = await request.Content.ReadAsByteArrayAsync();
            var md5 = MD5.Create();
            var md5Hash = md5.ComputeHash(content);
            request.Content.Headers.ContentMD5 = md5Hash;
            var response = await base.SendAsync(request, cancellationToken);
            return response;
        }
    }
}
