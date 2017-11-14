public interface IAccumulateableObject
{
    float CalculatePrio();
    float CurrentPrio { get; set; }
    byte[] GetData();
    short Id { get; }
}