using System;

public class DefaultObjectBuilder<T> : IObjectBuilder
{
    public object Build()
    {
        return Activator.CreateInstance(typeof(T));
    }
}