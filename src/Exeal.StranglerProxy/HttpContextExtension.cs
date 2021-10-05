﻿using System;
using System.IO;
using System.Net.Http;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Exeal.StranglerProxy
{
    internal static class HttpContextExtension
    {
        public static HttpRequestMessage CloneRequestFor(this HttpContext context, Uri targetUri)
        {
            context.Features.Get<IHttpBodyControlFeature>().AllowSynchronousIO = true;

            var actualRequest = context.Request;

            using (var bodyReader = new StreamReader(actualRequest.Body))
            {
                var remoteRequest = new HttpRequestMessage
                {
                    Method = new HttpMethod(actualRequest.Method),
                    RequestUri = targetUri,
                };

                var actualContent = bodyReader.ReadToEnd();

                if (!String.IsNullOrEmpty(actualContent))
                {
                    var contentType = new ContentType(actualRequest.ContentType);

                    remoteRequest.Content = new StringContent(actualContent, bodyReader.CurrentEncoding, contentType.MediaType);
                }

                foreach (var header in actualRequest.Headers)
                {
                    var headerName = header.Key;
                    var headerValue = header.Value.ToArray();

                    if (!remoteRequest.Headers.TryAddWithoutValidation(headerName, headerValue))
                        remoteRequest.Content?.Headers.TryAddWithoutValidation(headerName, headerValue);
                }

                remoteRequest.Headers.Host = targetUri.Host;

                return remoteRequest;
            }

        }
    }
}