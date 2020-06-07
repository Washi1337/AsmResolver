using System.Reflection;

namespace AsmResolver.DotNet
{
    internal class FieldReader
    {
        /// <summary>
        /// Finds and gets value of non-public field.
        /// </summary>
        /// <param name="instance">Object Instance</param>
        /// <param name="fieldName">Field Name</param>
        public static T ReadField<T>(object instance,string fieldName)
        {
            return (T)instance.GetType().GetField(fieldName, (BindingFlags)(-1))?.GetValue(instance);
        }
        /// <summary>
        /// Checks if an object has a non-public field.
        /// </summary>
        /// <param name="instance">Object Instance</param>
        /// <param name="fieldName">Field Name</param>
        public static bool ExistsField(object instance,string fieldName)
        {
            return instance.GetType().GetField(fieldName, (BindingFlags)(-1)) != null;
        }
    }
}