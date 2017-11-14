using System.Collections.Generic;

public class AccumulatedPriority : IComparer<IAccumulateableObject>
{
    public List<IAccumulateableObject> objects = new List<IAccumulateableObject>();

    public void Add(IAccumulateableObject obj)
    {
        objects.Add(obj);
    }

    public byte[] GetAndAccumulate(int count)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            objects[i].CurrentPrio += objects[i].CalculatePrio();
        }

        objects.Sort(this);

        List<byte> data = new List<byte>();
        for (int i = 0; i < count && i < objects.Count; i++)
        {
            objects[i].CurrentPrio = 0; // Could also set it to objects[i].CalculatePrio();
            data.AddRange(objects[i].GetData());
        }

        return data.ToArray();
    }

    public int Compare(IAccumulateableObject x, IAccumulateableObject y)
    {
        return -1 * x.CurrentPrio.CompareTo(y.CurrentPrio);
    }
}