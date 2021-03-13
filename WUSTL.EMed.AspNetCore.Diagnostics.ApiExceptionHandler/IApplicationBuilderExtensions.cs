// <copyright file="IApplicationBuilderExtensions.cs" company="Washington University in St. Louis">
// Copyright (c) 2021 Washington University in St. Louis. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// </copyright>

namespace WUSTL.EMed.AspNetCore.Diagnostics.ApiExceptionHandler
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using static ExceptionDetailsProvider;

    /// <summary>
    /// A static class of <see cref="IApplicationBuilder"/> extension methods.
    /// </summary>
    public static class IApplicationBuilderExtensions
    {
        // TODO: Options?
        // TODO: Support xml serialization?
        // TODO: Can IExceptionHandlerPathFeature.Error ever be null?
        private const string ContentTypeApplicationJson = "application/json";

        /// <summary>
        /// Delegate for constructing the response object for an exception.
        /// </summary>
        private static readonly Func<HttpContext, Exception, object> DeveloperExceptionDelegate = (context, ex) => new
        { // Problem Details for HTTP APIs https://tools.ietf.org/html/rfc7807
            type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            title = "An unhandled exception has occurred while executing the request.",
            status = 500,
            requestId = context.TraceIdentifier,
            exceptions = FlattenAndReverseExceptionTree(ex),
        };

        /// <summary>
        /// Delegate for constructing the response object when there is no exception available.
        /// </summary>
        private static readonly Func<HttpContext, object> DeveloperDelegate = (context) => new
        { // Problem Details for HTTP APIs https://tools.ietf.org/html/rfc7807
            type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            title = "An unhandled exception has occurred while executing the request.",
            status = 500,
            requestId = context.TraceIdentifier,
        };

        /// <summary>
        /// Json serializer settings for converting objects.
        /// </summary>
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            // ReferenceLoopHandling = ReferenceLoopHandling.Ignore, // PreserveReferencesHandling already handles this?
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            Error = (serializer, err) =>
            {
                // TODO: Only ignore certain errors and/or certain objects?
                err.ErrorContext.Handled = true;
            },
        };

        /// <summary>
        /// Captures <see cref="Exception"/> instances from the pipeline with the given request path prefix and generates a JSON error response.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="apiPathStartsWith">A prefix for api request paths (e.g. /api).</param>
        /// <returns>A reference to the app after the operation has completed.</returns>
        public static IApplicationBuilder UseDeveloperApiExceptionResult(this IApplicationBuilder app, string apiPathStartsWith)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseWhen(context => context.Request.Path.StartsWithSegments(new PathString(apiPathStartsWith), StringComparison.OrdinalIgnoreCase), apiApp =>
            {
                apiApp.UseExceptionHandler(builder =>
                {
                    builder.Run(async context =>
                    {
                        var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        context.Response.ContentType = ContentTypeApplicationJson;

                        if (exceptionFeature?.Error is null)
                        {
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(DeveloperExceptionDelegate(context, exceptionFeature.Error), JsonSerializerSettings), Encoding.UTF8).ConfigureAwait(false);
                        }
                        else
                        {
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(DeveloperDelegate(context), JsonSerializerSettings), Encoding.UTF8).ConfigureAwait(false);
                        }
                    });
                });
            });
        }

        /// <summary>
        /// Captures <see cref="Exception"/> instances from the pipeline with the given request path prefix and generates a JSON error response generated with the given delegate.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="apiPathStartsWith">A prefix for api request paths (e.g. /api).</param>
        /// <param name="responseObjectDelegate">A delegate to construct a response object from the <see cref="HttpContext"/> and <see cref="Exception"/>.</param>
        /// <returns>A reference to the app after the operation has completed.</returns>
        public static IApplicationBuilder UseApiExceptionHandler(this IApplicationBuilder app, string apiPathStartsWith, Func<HttpContext, Exception, object> responseObjectDelegate)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseWhen(context => context.Request.Path.StartsWithSegments(new PathString(apiPathStartsWith), StringComparison.OrdinalIgnoreCase), apiApp =>
            {
                apiApp.UseExceptionHandler(builder =>
                {
                    builder.Run(async context =>
                    {
                        var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        context.Response.ContentType = ContentTypeApplicationJson;
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(responseObjectDelegate(context, exceptionFeature.Error), JsonSerializerSettings), Encoding.UTF8).ConfigureAwait(false);
                    });
                });
            });
        }

        /// <summary>
        /// Captures <see cref="Exception"/> instances from the pipeline with the given request path prefix and generates a JSON error response generated with the given delegate.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="apiPathStartsWith">A prefix for api request paths (e.g. /api).</param>
        /// <param name="responseObjectDelegate">A delegate to construct a response object from the <see cref="HttpContext"/> and <see cref="Exception"/>.</param>
        /// <returns>A reference to the app after the operation has completed.</returns>
        public static IApplicationBuilder UseApiExceptionHandler(this IApplicationBuilder app, string apiPathStartsWith, Func<HttpContext, Exception, Task<object>> responseObjectDelegate)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseWhen(context => context.Request.Path.StartsWithSegments(new PathString(apiPathStartsWith), StringComparison.OrdinalIgnoreCase), apiApp =>
            {
                apiApp.UseExceptionHandler(builder =>
                {
                    builder.Run(async context =>
                    {
                        var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        context.Response.ContentType = ContentTypeApplicationJson;
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(await responseObjectDelegate(context, exceptionFeature.Error).ConfigureAwait(false), JsonSerializerSettings), Encoding.UTF8).ConfigureAwait(false);
                    });
                });
            });
        }

        /// <summary>
        /// Captures <see cref="Exception"/> instances from the pipeline with the given request path prefix and generates a JSON error response generated with the given delegate.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="apiPathStartsWith">A prefix for api request paths (e.g. /api).</param>
        /// <param name="responseObjectDelegate">A delegate to construct a response object from the <see cref="HttpContext"/>.</param>
        /// <returns>A reference to the app after the operation has completed.</returns>
        public static IApplicationBuilder UseApiExceptionHandler(this IApplicationBuilder app, string apiPathStartsWith, Func<HttpContext, object> responseObjectDelegate)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            /* var isApiEndpoint = context.GetEndpoint()?.Metadata?.Any(_ => _.GetType().Equals(typeof(ApiControllerAttribute))) ?? false; */

            return app.UseWhen(context => context.Request.Path.StartsWithSegments(new PathString(apiPathStartsWith), StringComparison.OrdinalIgnoreCase), apiApp =>
            {
                apiApp.UseExceptionHandler(builder =>
                {
                    builder.Run(async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        context.Response.ContentType = ContentTypeApplicationJson;
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(responseObjectDelegate(context), JsonSerializerSettings), Encoding.UTF8).ConfigureAwait(false);
                    });
                });
            });
        }

        /// <summary>
        /// Captures <see cref="Exception"/> instances from the pipeline with the given request path prefix and generates a JSON error response generated with the given delegate.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="apiPathStartsWith">A prefix for api request paths (e.g. /api).</param>
        /// <param name="responseObjectDelegate">A delegate to construct a response object from the <see cref="HttpContext"/>.</param>
        /// <returns>A reference to the app after the operation has completed.</returns>
        public static IApplicationBuilder UseApiExceptionHandler(this IApplicationBuilder app, string apiPathStartsWith, Func<HttpContext, Task<object>> responseObjectDelegate)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseWhen(context => context.Request.Path.StartsWithSegments(new PathString(apiPathStartsWith), StringComparison.OrdinalIgnoreCase), apiApp =>
            {
                apiApp.UseExceptionHandler(builder =>
                {
                    builder.Run(async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        context.Response.ContentType = ContentTypeApplicationJson;
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(await responseObjectDelegate(context).ConfigureAwait(false), JsonSerializerSettings), Encoding.UTF8).ConfigureAwait(false);
                    });
                });
            });
        }
    }
}
