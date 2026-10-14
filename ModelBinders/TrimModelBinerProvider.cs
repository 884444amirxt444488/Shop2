/*
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
public class TrimModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext modelBinderProviderContext)
    {
        if (modelBinderProviderContext.Metadata.ModelType == typeof(string))
        {
            return new BinderTypeModelBinder(typeof(TrimModelBinder));
        }
        return null;
    }
}
*/

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

public class TrimModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext modelBinderProviderContext)
    {
        if (modelBinderProviderContext.Metadata.ModelType == typeof(string))
        {
            return new BinderTypeModelBinder(typeof(TrimModelBinder));
        }
        return null;
    }
}








