namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false)]
internal sealed class CollectionBuilderAttribute(Type builderType, string methodName) : Attribute
{
    public Type BuilderType => builderType;
    public string MethodName => methodName;
}
