// In-memory repository for Todo items
public class TodoRepository
{
    private readonly List<TodoItem> _todos = new();
    private int _nextId = 1;

    public IEnumerable<TodoItem> GetAll() => _todos;

    public TodoItem? GetById(int id) => _todos.FirstOrDefault(t => t.Id == id);

    public TodoItem Add(TodoItem todo)
    {
        todo.Id = _nextId++;
        _todos.Add(todo);
        return todo;
    }

    public bool Remove(int id)
    {
        var todo = GetById(id);
        if (todo == null) return false;
        
        _todos.Remove(todo);
        return true;
    }
}