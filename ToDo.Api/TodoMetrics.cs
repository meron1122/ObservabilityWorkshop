using System.Diagnostics.Metrics;

namespace Microsoft.Extensions.Hosting;

public class TodoMetrics
{
    private readonly Counter<int> _todosCreated;

    public TodoMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Todo.Api");
        _todosCreated = meter.CreateCounter<int>("todo.api.todos_created");
    }

    public void TodoCreated()
    {
        _todosCreated.Add(1);
    }
}
