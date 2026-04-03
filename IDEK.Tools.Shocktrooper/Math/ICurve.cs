namespace IDEK.Tools.Math;

public interface ICurve
{
    public interface IControlPoint
    {
        float Value { get; set; }
    }
        
    public IControlPoint[] Keys { get; }
    
    int Length { get; }
    
    public float Evaluate(float input);
}