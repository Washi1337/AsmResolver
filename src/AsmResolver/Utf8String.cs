using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using AsmResolver.Collections;
using AsmResolver.Shims;

namespace AsmResolver
{
    /// <summary>
    /// Represents an immutable UTF-8 encoded string. This class supports preserving invalid UTF-8 code sequences.
    /// </summary>
    [DebuggerDisplay("{DebugDisplay}")]
    public sealed class Utf8String :
        IEquatable<Utf8String>,
        IEquatable<string>,
        IEquatable<byte[]>,
        IComparable<Utf8String>,
        IEnumerable<char>
    {
        /// <summary>
        /// Represents the empty UTF-8 string.
        /// </summary>
        public static readonly Utf8String Empty = new(ArrayShim.Empty<byte>());

        private readonly byte[] _data;
        private string? _cachedString;

        /// <summary>
        /// Creates a new UTF-8 string from the provided raw data.
        /// </summary>
        /// <param name="data">The raw UTF-8 data.</param>
        public Utf8String(byte[] data)
            : this(data, 0, data.Length)
        {
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        /// <summary>
        /// Creates a new UTF-8 string from the provided raw data.
        /// </summary>
        /// <param name="data">The raw UTF-8 data.</param>
        public Utf8String(ReadOnlySpan<byte> data)
        {
            _data = data.ToArray();
        }

        /// <summary>
        /// Creates a new UTF-8 string from the provided <see cref="System.ReadOnlySpan{Char}"/>.
        /// </summary>
        /// <param name="value">The string value to encode as UTF-8.</param>
        public Utf8String(ReadOnlySpan<char> value)
        {
            _data = new byte[Encoding.UTF8.GetByteCount(value)];
            Encoding.UTF8.GetBytes(value, _data.AsSpan());
        }
#endif

        /// <summary>
        /// Creates a new UTF-8 string from the provided raw data.
        /// </summary>
        /// <param name="data">The raw UTF-8 data.</param>
        /// <param name="index">The starting index to read from.</param>
        /// <param name="count">The number of bytes to read..</param>
        public Utf8String(byte[] data, int index, int count)
        {
            // Copy data to enforce immutability.
            _data = new byte[count];
            Buffer.BlockCopy(data, index, _data,0, count);
        }

        /// <summary>
        /// Creates a new UTF-8 string from the provided <see cref="System.String"/>.
        /// </summary>
        /// <param name="value">The string value to encode as UTF-8.</param>
        public Utf8String(string value)
        {
            _data = Encoding.UTF8.GetBytes(value);
            _cachedString = value;
        }

        /// <summary>
        /// Gets the string value represented by the UTF-8 bytes.
        /// </summary>
        public string Value => _cachedString ??= Encoding.UTF8.GetString(_data);

        /// <summary>
        /// Gets the number of bytes used by the string.
        /// </summary>
        public int ByteCount => _data.Length;

        /// <summary>
        /// Gets the number of characters in the string.
        /// </summary>
        public int Length => _cachedString is null ? Encoding.UTF8.GetCharCount(_data) : Value.Length;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string DebugDisplay => Value.CreateEscapedString();

        /// <summary>
        /// Gets a single character in the string.
        /// </summary>
        /// <param name="index">The character index.</param>
        public char this[int index] => Value[index];

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        /// <summary>
        /// Creates a new read-only span over the string.
        /// </summary>
        /// <returns>The read-only span representation of the string.</returns>
        public ReadOnlySpan<byte> AsSpan()
        {
            return _data.AsSpan();
        }

        /// <summary>
        /// Creates a new read-only span over the string.
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <returns>The read-only span representation of the string.</returns>
        public ReadOnlySpan<byte> AsSpan(int start)
        {
            return _data.AsSpan(start);
        }

        /// <summary>
        /// Creates a new read-only span over the string.
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice.</param>
        /// <returns>The read-only span representation of the string.</returns>
        public ReadOnlySpan<byte> AsSpan(int start, int length)
        {
            return _data.AsSpan(start, length);
        }
#endif

        /// <summary>
        /// Gets the raw UTF-8 bytes of the string.
        /// </summary>
        /// <returns>The bytes.</returns>
        public byte[] GetBytes()
        {
            byte[] copy = new byte[_data.Length];
            GetBytes(copy, 0, copy.Length);
            return copy;
        }

        /// <summary>
        /// Obtains the raw UTF-8 bytes of the string, and writes it into the provided buffer.
        /// </summary>
        /// <param name="buffer">The output buffer to receive the bytes in.</param>
        /// <param name="index">The index into the output buffer to start writing at.</param>
        /// <param name="length">The number of bytes to write.</param>
        /// <returns>The actual number of bytes that were written.</returns>
        public int GetBytes(byte[] buffer, int index, int length)
        {
            length = Math.Min(length, ByteCount);
            Buffer.BlockCopy(_data, 0, buffer,index, length);
            return length;
        }

        /// <summary>
        /// Gets the underlying byte array of this string.
        /// </summary>
        /// <remarks>
        /// This method should only be used if performance is critical. Modifying the returning array
        /// <strong>will</strong> change the internal state of the string.
        /// </remarks>
        /// <returns>The bytes.</returns>
        public byte[] GetBytesUnsafe() => _data;

        /// <summary>
        /// Produces a new string that is the concatenation of the current string and the provided string.
        /// </summary>
        /// <param name="other">The other string to append..</param>
        /// <returns>The new string.</returns>
        public Utf8String Concat(Utf8String? other) => !IsNullOrEmpty(other)
            ? Concat(other._data)
            : this;

        /// <summary>
        /// Produces a new string that is the concatenation of the current string and the provided string.
        /// </summary>
        /// <param name="other">The other string to append..</param>
        /// <returns>The new string.</returns>
        public Utf8String Concat(string? other) => !string.IsNullOrEmpty(other)
            ? Concat(Encoding.UTF8.GetBytes(other!))
            : this;

        /// <summary>
        /// Produces a new string that is the concatenation of the current string and the provided byte array.
        /// </summary>
        /// <param name="other">The byte array to append.</param>
        /// <returns>The new string.</returns>
        public Utf8String Concat(byte[]? other)
        {
            if (other is null || other.Length == 0)
                return this;

            byte[] result = new byte[Length + other.Length];
            Buffer.BlockCopy(_data, 0, result, 0, _data.Length);
            Buffer.BlockCopy(other, 0, result, _data.Length, other.Length);
            return result;
        }

        /// <summary>
        /// Gets the zero-based index of the first occurrence of the provided character in the string.
        /// </summary>
        /// <param name="needle">The character to search.</param>
        /// <returns>The index, or -1 if the character is not present.</returns>
        public int IndexOf(char needle) => Value.IndexOf(needle);

        /// <summary>
        /// Gets the zero-based index of the first occurrence of the provided character in the string.
        /// </summary>
        /// <param name="needle">The character to search.</param>
        /// <param name="startIndex">The index to start searching at.</param>
        /// <returns>The index, or -1 if the character is not present.</returns>
        public int IndexOf(char needle, int startIndex) => Value.IndexOf(needle, startIndex);

        /// <summary>
        /// Gets the zero-based index of the first occurrence of the provided string in the string.
        /// </summary>
        /// <param name="needle">The string to search.</param>
        /// <returns>The index, or -1 if the string is not present.</returns>
        public int IndexOf(string needle) => Value.IndexOf(needle);

        /// <summary>
        /// Gets the zero-based index of the first occurrence of the provided string in the string.
        /// </summary>
        /// <param name="needle">The string to search.</param>
        /// <param name="startIndex">The index to start searching at.</param>
        /// <returns>The index, or -1 if the string is not present.</returns>
        public int IndexOf(string needle, int startIndex) => Value.IndexOf(needle, startIndex);

        /// <summary>
        /// Gets the zero-based index of the first occurrence of the provided string in the string.
        /// </summary>
        /// <param name="needle">The string to search.</param>
        /// <param name="comparison">The comparison algorithm to use.</param>
        /// <returns>The index, or -1 if the string is not present.</returns>
        public int IndexOf(string needle, StringComparison comparison) => Value.IndexOf(needle, comparison);

        /// <summary>
        /// Gets the zero-based index of the first occurrence of the provided string in the string.
        /// </summary>
        /// <param name="needle">The string to search.</param>
        /// <param name="startIndex">The index to start searching at.</param>
        /// <param name="comparison">The comparison algorithm to use.</param>
        /// <returns>The index, or -1 if the string is not present.</returns>
        public int IndexOf(string needle, int startIndex, StringComparison comparison) => Value.IndexOf(needle, startIndex, comparison);

        /// <summary>
        /// Gets the zero-based index of the last occurrence of the provided character in the string.
        /// </summary>
        /// <param name="needle">The character to search.</param>
        /// <returns>The index, or -1 if the character is not present.</returns>
        public int LastIndexOf(char needle) => Value.LastIndexOf(needle);

        /// <summary>
        /// Gets the zero-based index of the last occurrence of the provided character in the string.
        /// </summary>
        /// <param name="needle">The character to search.</param>
        /// <param name="startIndex">The index to start searching at.</param>
        /// <returns>The index, or -1 if the character is not present.</returns>
        public int LastIndexOf(char needle, int startIndex) => Value.LastIndexOf(needle, startIndex);

        /// <summary>
        /// Gets the zero-based index of the last occurrence of the provided string in the string.
        /// </summary>
        /// <param name="needle">The string to search.</param>
        /// <returns>The index, or -1 if the string is not present.</returns>
        public int LastIndexOf(string needle) => Value.LastIndexOf(needle);

        /// <summary>
        /// Gets the zero-based index of the first occurrence of the provided string in the string.
        /// </summary>
        /// <param name="needle">The string to search.</param>
        /// <param name="startIndex">The index to start searching at.</param>
        /// <returns>The index, or -1 if the string is not present.</returns>
        public int LastIndexOf(string needle, int startIndex) => Value.LastIndexOf(needle, startIndex);

        /// <summary>
        /// Gets the zero-based index of the last occurrence of the provided string in the string.
        /// </summary>
        /// <param name="needle">The string to search.</param>
        /// <param name="comparison">The comparison algorithm to use.</param>
        /// <returns>The index, or -1 if the string is not present.</returns>
        public int LastIndexOf(string needle, StringComparison comparison) => Value.LastIndexOf(needle, comparison);

        /// <summary>
        /// Gets the zero-based index of the last occurrence of the provided string in the string.
        /// </summary>
        /// <param name="needle">The string to search.</param>
        /// <param name="startIndex">The index to start searching at.</param>
        /// <param name="comparison">The comparison algorithm to use.</param>
        /// <returns>The index, or -1 if the string is not present.</returns>
        public int LastIndexOf(string needle, int startIndex, StringComparison comparison) => Value.LastIndexOf(needle, startIndex, comparison);

        /// <summary>
        /// Determines whether the string contains the provided string.
        /// </summary>
        /// <param name="needle">The string to search.</param>
        /// <returns><c>true</c> if the string is present, <c>false</c> otherwise.</returns>
        public bool Contains(string needle) => IndexOf(needle) >= 0;

        /// <summary>
        /// Determines whether the provided string is <c>null</c> or the empty string.
        /// </summary>
        /// <param name="value">The string to verify.</param>
        /// <returns><c>true</c> if the string is <c>null</c> or empty, <c>false</c> otherwise.</returns>
        public static bool IsNullOrEmpty([NotNullWhen(false)] Utf8String? value) =>
            value is null || value.ByteCount == 0;

        /// <summary>
        /// Determines whether two strings are considered equal.
        /// </summary>
        /// <param name="other">The other string.</param>
        /// <returns><c>true</c> if the strings are considered equal, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// This operation performs a byte-level comparison of the two strings.
        /// </remarks>
        public bool Equals(Utf8String? other) =>
            other is not null && ByteArrayEqualityComparer.Instance.Equals(_data, other._data);

        /// <inheritdoc />
        /// <remarks>
        /// This operation performs a byte-level comparison of the two strings.
        /// </remarks>
        public bool Equals(byte[]? other) => other is not null && ByteArrayEqualityComparer.Instance.Equals(_data, other);

        /// <inheritdoc />
        /// <remarks>
        /// This operation performs a byte-level comparison of the two strings.
        /// </remarks>
        public bool Equals(string? other) =>other is not null && Value.Equals(other);

        /// <inheritdoc />
        public int CompareTo(Utf8String? other) => other is not null
            ? ByteArrayEqualityComparer.Instance.Compare(_data, other._data)
            : 1;

        /// <inheritdoc />
        public IEnumerator<char> GetEnumerator() => Value.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        /// <remarks>
        /// This operation performs a byte-level comparison of the two strings.
        /// </remarks>
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Utf8String other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => ByteArrayEqualityComparer.Instance.GetHashCode(_data);

        /// <inheritdoc />
        public override string ToString() => Value;

        /// <summary>
        /// Converts a <see cref="Utf8String"/> into a <see cref="System.String"/>.
        /// </summary>
        /// <param name="value">The UTF-8 string value to convert.</param>
        /// <returns>The string.</returns>
        [return: NotNullIfNotNull("value")]
        public static implicit operator string?(Utf8String? value)
        {
            if (value is null)
                return null;
            if (value.ByteCount == 0)
                return string.Empty;
            return value.Value;
        }

        /// <summary>
        /// Converts a <see cref="System.String"/> into an <see cref="Utf8String"/>.
        /// </summary>
        /// <param name="value">The string value to convert.</param>
        /// <returns>The new UTF-8 encoded string.</returns>
        [return: NotNullIfNotNull("value")]
        public static implicit operator Utf8String?(string? value)
        {
            if (value is null)
                return null;
            if (value.Length == 0)
                return Empty;

            return new Utf8String(value);
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        /// <summary>
        /// Converts a raw sequence of bytes into an <see cref="Utf8String"/>.
        /// </summary>
        /// <param name="data">The raw data to convert.</param>
        /// <returns>The new UTF-8 encoded string.</returns>
        public static implicit operator Utf8String(ReadOnlySpan<byte> data)
        {
            if (data.IsEmpty)
                return Empty;

            return new Utf8String(data);
        }

        /// <summary>
        /// Converts a <see cref="System.ReadOnlySpan{Char}"/> into an <see cref="Utf8String"/>.
        /// </summary>
        /// <param name="data">The string value to convert.</param>
        /// <returns>The new UTF-8 encoded string.</returns>
        public static implicit operator Utf8String(ReadOnlySpan<char> data)
        {
            if (data.IsEmpty)
                return Empty;

            return new Utf8String(data);
        }

        /// <summary>
        /// Converts a <see cref="Utf8String"/> into a <see cref="System.ReadOnlySpan{Byte}"/>.
        /// </summary>
        /// <param name="value">The UTF-8 string value to convert.</param>
        /// <returns>The span.</returns>
        public static implicit operator ReadOnlySpan<byte>(Utf8String? value)
        {
            if (value is null)
                return ReadOnlySpan<byte>.Empty;

            return value._data;
        }

        /// <summary>
        /// Converts a <see cref="Utf8String"/> into a <see cref="System.ReadOnlySpan{Char}"/>.
        /// </summary>
        /// <param name="value">The UTF-8 string value to convert.</param>
        /// <returns>The span.</returns>
        public static implicit operator ReadOnlySpan<char>(Utf8String? value)
        {
            if (value is null)
                return ReadOnlySpan<char>.Empty;

            return value.Value;
        }
#endif

        /// <summary>
        /// Converts a raw sequence of bytes into an <see cref="Utf8String"/>.
        /// </summary>
        /// <param name="data">The raw data to convert.</param>
        /// <returns>The new UTF-8 encoded string.</returns>
        [return: NotNullIfNotNull("data")]
        public static implicit operator Utf8String?(byte[]? data)
        {
            if (data is null)
                return null;
            if (data.Length == 0)
                return Empty;

            return new Utf8String(data);
        }

        /// <summary>
        /// Determines whether two UTF-8 encoded strings are considered equal.
        /// </summary>
        /// <param name="a">The first string.</param>
        /// <param name="b">The second string.</param>
        /// <returns><c>true</c> if the strings are considered equal, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// This operation performs a byte-level comparison of the two strings.
        /// </remarks>
        public static bool operator ==(Utf8String? a, Utf8String? b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a is null || b is null)
                return false;
            return a.Equals(b);
        }

        /// <summary>
        /// Determines whether two UTF-8 encoded strings are not considered equal.
        /// </summary>
        /// <param name="a">The first string.</param>
        /// <param name="b">The second string.</param>
        /// <returns><c>true</c> if the strings are not considered equal, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// This operation performs a byte-level comparison of the two strings.
        /// </remarks>
        public static bool operator !=(Utf8String? a, Utf8String? b) => !(a == b);

        /// <summary>
        /// Determines whether an UTF-8 encoded string is considered equal to the provided <see cref="System.String"/>.
        /// </summary>
        /// <param name="a">The first string.</param>
        /// <param name="b">The second string.</param>
        /// <returns><c>true</c> if the strings are considered equal, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// This operation performs a string-level comparison.
        /// </remarks>
        public static bool operator ==(Utf8String? a, string? b)
        {
            if (a is null)
                return b is null;
            return b is not null && a.Equals(b);
        }

        /// <summary>
        /// Determines whether an UTF-8 encoded string is not equal to the provided <see cref="System.String"/>.
        /// </summary>
        /// <param name="a">The first string.</param>
        /// <param name="b">The second string.</param>
        /// <returns><c>true</c> if the strings are not considered equal, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// This operation performs a string-level comparison.
        /// </remarks>
        public static bool operator !=(Utf8String? a, string? b) => !(a == b);

        /// <summary>
        /// Determines whether the underlying bytes of an UTF-8 encoded string is equal to the provided byte array.
        /// </summary>
        /// <param name="a">The UTF-8 string.</param>
        /// <param name="b">The byte array.</param>
        /// <returns><c>true</c> if the byte arrays are considered equal, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// This operation performs a byte-level comparison.
        /// </remarks>
        public static bool operator ==(Utf8String? a, byte[]? b)
        {
            if (a is null)
                return b is null;
            return b is not null && a.Equals(b);
        }

        /// <summary>
        /// Determines whether the underlying bytes of an UTF-8 encoded string is not equal to the provided byte array.
        /// </summary>
        /// <param name="a">The UTF-8 string.</param>
        /// <param name="b">The byte array.</param>
        /// <returns><c>true</c> if the byte arrays are not considered equal, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// This operation performs a byte-level comparison.
        /// </remarks>
        public static bool operator !=(Utf8String? a, byte[]? b) => !(a == b);

        /// <summary>
        /// Concatenates two UTF-8 encoded strings together.
        /// </summary>
        /// <param name="a">The first string.</param>
        /// <param name="b">The second string.</param>
        /// <returns>The newly produced string.</returns>
        public static Utf8String operator +(Utf8String? a, Utf8String? b)
        {
            if (IsNullOrEmpty(a))
                return IsNullOrEmpty(b) ? Utf8String.Empty : b;

            return a.Concat(b);
        }

        /// <summary>
        /// Concatenates two UTF-8 encoded strings together.
        /// </summary>
        /// <param name="a">The first string.</param>
        /// <param name="b">The second string.</param>
        /// <returns>The newly produced string.</returns>
        public static Utf8String operator +(Utf8String? a, string? b)
        {
            if (IsNullOrEmpty(a))
                return string.IsNullOrEmpty(b) ? Empty : b!;

            return a.Concat(b);
        }

        /// <summary>
        /// Concatenates an UTF-8 encoded string together with a byte array.
        /// </summary>
        /// <param name="a">The string.</param>
        /// <param name="b">The byte array.</param>
        /// <returns>The newly produced string.</returns>
        public static Utf8String operator +(Utf8String? a, byte[]? b)
        {
            if (IsNullOrEmpty(a))
                return b is null || b.Length == 0 ? Empty : b;

            return a.Concat(b);
        }
    }
}
