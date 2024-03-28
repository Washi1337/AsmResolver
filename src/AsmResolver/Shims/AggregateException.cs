#if NET35
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using AsmResolver.Shims;

namespace System;

/// <summary>
/// Represents one or more errors that occur during application execution.
/// </summary>
public class AggregateException : Exception
{
    private const string AggregateException_ctor_DefaultMessage = "One or more errors occurred.";
    private const string AggregateException_InnerException = "(Inner Exception #{0}) ";
    private const string InnerExceptionPrefix = " ---> ";

    private readonly Exception[] _innerExceptions; // Complete set of exceptions.

    private ReadOnlyCollection<Exception>?
        _rocView; // separate from _innerExceptions to enable trimming if InnerExceptions isn't used


    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateException"/> class.
    /// </summary>
    public AggregateException()
        : this(AggregateException_ctor_DefaultMessage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateException"/> class with
    /// a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public AggregateException(string? message)
        : base(message ?? AggregateException_ctor_DefaultMessage)
    {
        _innerExceptions = ArrayShim.Empty<Exception>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateException"/> class with a specified error
    /// message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="innerException"/> argument
    /// is null.</exception>
    public AggregateException(string? message, Exception innerException)
        : base(message ?? AggregateException_ctor_DefaultMessage, innerException)
    {
        if (innerException is null)
            throw new ArgumentNullException(nameof(innerException));

        _innerExceptions = new[] { innerException };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateException"/> class with
    /// references to the inner exceptions that are the cause of this exception.
    /// </summary>
    /// <param name="innerExceptions">The exceptions that are the cause of the current exception.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="innerExceptions"/> argument
    /// is null.</exception>
    /// <exception cref="ArgumentException">An element of <paramref name="innerExceptions"/> is
    /// null.</exception>
    public AggregateException(IEnumerable<Exception> innerExceptions)
        :
        this(AggregateException_ctor_DefaultMessage,
            innerExceptions ?? throw new ArgumentNullException(nameof(innerExceptions)))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateException"/> class with
    /// references to the inner exceptions that are the cause of this exception.
    /// </summary>
    /// <param name="innerExceptions">The exceptions that are the cause of the current exception.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="innerExceptions"/> argument
    /// is null.</exception>
    /// <exception cref="ArgumentException">An element of <paramref name="innerExceptions"/> is
    /// null.</exception>
    public AggregateException(params Exception[] innerExceptions)
        :
        this(AggregateException_ctor_DefaultMessage,
            innerExceptions ?? throw new ArgumentNullException(nameof(innerExceptions)))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateException"/> class with a specified error
    /// message and references to the inner exceptions that are the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerExceptions">The exceptions that are the cause of the current exception.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="innerExceptions"/> argument
    /// is null.</exception>
    /// <exception cref="ArgumentException">An element of <paramref name="innerExceptions"/> is
    /// null.</exception>
    public AggregateException(string? message, IEnumerable<Exception> innerExceptions)
        : this(message,
            new List<Exception>(innerExceptions ?? throw new ArgumentNullException(nameof(innerExceptions))).ToArray(),
            cloneExceptions: false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateException"/> class with a specified error
    /// message and references to the inner exceptions that are the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerExceptions">The exceptions that are the cause of the current exception.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="innerExceptions"/> argument
    /// is null.</exception>
    /// <exception cref="ArgumentException">An element of <paramref name="innerExceptions"/> is
    /// null.</exception>
    public AggregateException(string? message, params Exception[] innerExceptions)
        :
        this(message, innerExceptions ?? throw new ArgumentNullException(nameof(innerExceptions)),
            cloneExceptions: true)
    {
    }

    private AggregateException(string? message, Exception[] innerExceptions, bool cloneExceptions)
        :
        base(message ?? AggregateException_ctor_DefaultMessage, innerExceptions.Length > 0 ? innerExceptions[0] : null)
    {
        _innerExceptions = cloneExceptions ? new Exception[innerExceptions.Length] : innerExceptions;

        for (int i = 0; i < _innerExceptions.Length; i++)
        {
            _innerExceptions[i] = innerExceptions[i];

            if (innerExceptions[i] == null)
            {
                throw new ArgumentException("An element of innerExceptions was null.");
            }
        }
    }

    /// <summary>Gets a message that describes the exception.</summary>
    public override string Message
    {
        get
        {
            if (_innerExceptions.Length == 0)
            {
                return base.Message;
            }

            var sb = new StringBuilder(256);
            sb.Append(base.Message);
            sb.Append(' ');
            for (int i = 0; i < _innerExceptions.Length; i++)
            {
                sb.Append('(');
                sb.Append(_innerExceptions[i].Message);
                sb.Append(") ");
            }

            sb.Length--;
            return sb.ToString();
        }
    }

    /// <summary>
    /// Returns the <see cref="AggregateException"/> that is the root cause of this exception.
    /// </summary>
    public override Exception GetBaseException()
    {
        // Returns the first inner AggregateException that contains more or less than one inner exception

        // Recursively traverse the inner exceptions as long as the inner exception of type AggregateException and has only one inner exception
        Exception? back = this;
        AggregateException? backAsAggregate = this;
        while (backAsAggregate is { InnerExceptions.Count: 1 })
        {
            back = back!.InnerException;
            backAsAggregate = back as AggregateException;
        }

        return back!;
    }

    /// <summary>
    /// Gets a read-only collection of the <see cref="Exception"/> instances that caused the
    /// current exception.
    /// </summary>
    public ReadOnlyCollection<Exception> InnerExceptions =>
        _rocView ??= new ReadOnlyCollection<Exception>(_innerExceptions);

    /// <summary>
    /// Creates and returns a string representation of the current <see cref="AggregateException"/>.
    /// </summary>
    /// <returns>A string representation of the current exception.</returns>
    public override string ToString()
    {
        StringBuilder text = new StringBuilder();
        text.Append(base.ToString());

        for (int i = 0; i < _innerExceptions.Length; i++)
        {
            if (_innerExceptions[i] == InnerException)
                continue; // Already logged in base.ToString()

            text.Append(Environment.NewLine + InnerExceptionPrefix);
            text.AppendFormat(CultureInfo.InvariantCulture, AggregateException_InnerException, i);
            text.Append(_innerExceptions[i]);
            text.Append("<---");
            text.AppendLine();
        }

        return text.ToString();
    }
}

#endif
