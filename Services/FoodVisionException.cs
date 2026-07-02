namespace DiabetesHelper.Services;

public class FoodVisionException : Exception
{
    public FoodVisionException(string message) : base(message)
    {
    }

    public FoodVisionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
