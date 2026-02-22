namespace PACommon.Extensions
{
    /// <summary>
    /// Object for storing extensions for <see cref="Type"/>.
    /// </summary>
    public static class TypeExtensions
    {
        /// <inheritdoc cref="Type.IsAssignableFrom(Type?)"/>
        public static bool IsGenericAssignableFromType(this Type genericType, Type givenType)
        {
            var interfaceTypes = givenType.GetInterfaces();
            if (interfaceTypes.Any(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == genericType))
            {
                return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }

            var baseType = givenType.BaseType;
            return baseType is not null && genericType.IsGenericAssignableFromType(baseType);
        }
    }
}
