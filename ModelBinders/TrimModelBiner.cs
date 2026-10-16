/*
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

public class TrimModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext modelBindingContext)
    {
        var Value = modelBindingContext.ValueProvider.GetValue(modelBindingContext.ModelName);
        if (Value == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }
        string? Result = Value.FirstValue;

        if (Result != null)
        {
            Result = Result.Trim();
        }
        modelBindingContext.Result = ModelBindingResult.Success(Result);
        return Task.CompletedTask;
    }
}
*/

using Microsoft.AspNetCore.Mvc.ModelBinding;
public class TrimModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext modelBindingContext)
    {
        var Value = modelBindingContext.ValueProvider.GetValue(modelBindingContext.ModelName);
        if (Value == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }
        string? Result = Value.FirstValue;

        if (Result != null)
        {
            Result = Result.Trim();
        }

        modelBindingContext.Result = ModelBindingResult.Success(Result);
        return Task.CompletedTask;
    }





}







