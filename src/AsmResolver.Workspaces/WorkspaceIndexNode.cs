using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using AsmResolver.Workspaces.Collections;

namespace AsmResolver.Workspaces
{
    /// <summary>
    /// Represents a single node in a <see cref="WorkspaceIndex"/>.
    /// </summary>
    public class WorkspaceIndexNode
    {
        private List<object>? _data = null;

        /// <summary>
        /// Creates a new instance of the <see cref="WorkspaceIndexNode"/> class.
        /// </summary>
        /// <param name="subject">The subject that is indexed.</param>
        public WorkspaceIndexNode(object subject)
        {
            Subject = subject;
            IncomingEdges = new IncomingEdgesCollection(this);
            OutgoingEdges = new OutgoingEdgesCollection(this);
        }

        /// <summary>
        /// Gets the subject associated to the node.
        /// </summary>
        public object Subject
        {
            get;
        }

        public IncomingEdgesCollection IncomingEdges
        {
            get;
        }

        public OutgoingEdgesCollection OutgoingEdges
        {
            get;
        }

        private IList<object> Data
        {
            get
            {
                if (_data is null)
                    Interlocked.CompareExchange(ref _data, new(), null);
                return _data;
            }
        }

        /// <summary>
        /// Gets a stored data of type <see cref="T"/>.
        /// </summary>
        /// <returns>default value if data with type <see cref="T"/> is not stored, otherwise the data.</returns>
        /// <typeparam name="T">The type of data to obtain.</typeparam>
        public T? GetData<T>() => Data
            .OfType<T>()
            .FirstOrDefault();

        /// <summary>
        /// Gets a stored data of type <see cref="T"/>.
        /// </summary>
        /// <param name="data">The data</param>
        /// <returns>false if data with type <see cref="T"/> is not stored, otherwise true.</returns>
        /// <typeparam name="T">The type of data to obtain.</typeparam>
        public bool TryGetData<T>([NotNullWhen(true)] out T? data)
        {
            for (int i = 0; i < Data.Count; i++)
            {
                if (Data[i] is not T newData)
                    continue;
                data = newData;
                return true;
            }
            data = default;
            return false;
        }

        /// <summary>
        /// Gets a stored data or creates new data of type <see cref="T"/>.
        /// </summary>
        /// <param name="defaultValue">Default value of new data.</param>
        /// <returns>The data.</returns>
        /// <typeparam name="T">The type of data to obtain.</typeparam>
        public T GetOrCreate<T>(T defaultValue)
        {
            if (!TryGetData<T>(out var data))
            {
                data = defaultValue;
                Data.Add(data!);
            }
            return data;
        }

        /// <summary>
        /// Gets a stored data or creates new data of type <see cref="T"/>.
        /// </summary>
        /// <param name="factory">Function that generates new data.</param>
        /// <returns>The data.</returns>
        /// <typeparam name="T">The type of data to obtain.</typeparam>
        public T GetOrCreate<T>(Func<T> factory)
        {
            if (!TryGetData<T>(out var data))
            {
                data = factory();
                Data.Add(data!);
            }
            return data;
        }

        /// <summary>
        /// Gets a stored data or creates new data of type <see cref="T"/>.
        /// </summary>
        /// <returns>The data.</returns>
        /// <typeparam name="T">The type of data to obtain.</typeparam>
        public T GetOrCreate<T>() where T : new()
        {
            if (!TryGetData<T>(out var data))
            {
                data = new();
                Data.Add(data);
            }
            return data;
        }

        /// <summary>
        /// Stores the data of type <see cref="T"/>.
        /// </summary>
        /// <param name="data">The data to store</param>
        /// <typeparam name="T">The type of data to store.</typeparam>
        public void SetData<T>(T data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            if (TryGetData<T>(out var old))
                Data.Remove(old);
            Data.Add(data);
        }

        /// <summary>
        /// Removes data of type <see cref="T"/>.
        /// </summary>
        /// <returns>true if data with type <see cref="T"/> was removed, otherwise false.</returns>
        /// <typeparam name="T">The type of data to obtain.</typeparam>
        public bool RemoveData<T>()
            => TryGetData<T>(out var data)
               && Data.Remove(data);
    }
}
