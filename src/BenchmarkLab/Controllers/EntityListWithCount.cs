namespace MeasureThat.Net.Controllers
{
    using System.Collections.Generic;

    public class EntityListWithCount<T>
    {
        public readonly IEnumerable<T> Entities;

        public readonly long Count;

        public EntityListWithCount(IEnumerable<T> entities, long count)
        {
            Entities = entities;
            Count = count;
        }
    }
}
