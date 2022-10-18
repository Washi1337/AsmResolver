using System.Reflection;

namespace AsmResolver.DotNet.Dynamic
{
    internal static class FieldReader
    {
        /// <summary>
        /// Finds and gets value of non-public field.
        /// </summary>
        /// <exception cref="T:System.NullReferenceException"/>
        /// <exception cref="T:System.ArgumentNullException"/>
        /// <param name="instance">Object Instance</param>
        /// <param name="fieldName">Field Name</param>
        public static T? ReadField<T>(object instance, string fieldName) =>
            (T?) instance.GetType().GetField(fieldName, (BindingFlags) (-1))?.GetValue(instance);

        /// <summary>
        /// Tries to find and gets value of non-public field.
        /// </summary>
        /// <param name="instance">Object Instance</param>
        /// <param name="fieldName">Field Name</param>
        /// <param name="val">Returns result</param>
        public static bool TryReadField<T>(object instance, string fieldName, out T? val)
        {
            val = default;
            var field = instance.GetType().GetField(fieldName, (BindingFlags) (-1));
            if (field == null)
                return false;
            val = (T?) field.GetValue(instance);
            return true;
        }

        /// <summary>
        /// Checks if an object has a non-public field.
        /// </summary>
        /// <param name="instance">Object Instance</param>
        /// <param name="fieldName">Field Name</param>
        public static bool ExistsField(object instance, string fieldName) =>
            instance.GetType().GetField(fieldName, (BindingFlags) (-1)) != null;
    }
}
