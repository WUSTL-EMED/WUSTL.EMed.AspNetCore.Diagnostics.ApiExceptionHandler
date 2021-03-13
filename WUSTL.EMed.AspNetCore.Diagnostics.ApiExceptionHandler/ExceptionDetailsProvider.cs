// <copyright file="ExceptionDetailsProvider.cs" company="Washington University in St. Louis">
// Copyright (c) 2021 Washington University in St. Louis. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// </copyright>
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified from: https://raw.githubusercontent.com/dotnet/aspnetcore/master/src/Shared/StackTrace/ExceptionDetails/ExceptionDetailsProvider.cs

namespace WUSTL.EMed.AspNetCore.Diagnostics.ApiExceptionHandler
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// A static class of <see cref="Exception"/> methods.
    /// </summary>
    public static class ExceptionDetailsProvider
    {
        /// <summary>
        /// Flattens and reverses a set of nested exceptions into an <see cref="IEnumerable{T}"/> of <see cref="Exception"/>.
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/>.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Exception"/>.</returns>
        public static IEnumerable<Exception> FlattenAndReverseExceptionTree(Exception ex)
        {
            // ReflectionTypeLoadException is special because the details are in
            // the LoaderExceptions property
            var typeLoadException = ex as ReflectionTypeLoadException;
            if (typeLoadException != null)
            {
                var typeLoadExceptions = new List<Exception>();
                foreach (var loadException in typeLoadException.LoaderExceptions)
                {
                    typeLoadExceptions.AddRange(FlattenAndReverseExceptionTree(loadException));
                }

                typeLoadExceptions.Add(typeLoadException);
                return typeLoadExceptions;
            }

            var list = new List<Exception>();
            if (ex is AggregateException aggregateException)
            {
                list.Add(ex);
                foreach (var innerException in aggregateException.Flatten().InnerExceptions)
                {
                    list.Add(innerException);
                }
            }
            else
            {
                while (ex != null)
                {
                    list.Add(ex);
                    ex = ex.InnerException;
                }

                list.Reverse();
            }

            return list;
        }
    }
}
