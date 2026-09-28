namespace Tharga.Neurolito.Client.Contract;

public class InputBuilder
{
    private readonly List<Message> _rows = new();
    private string _model;

    public Input Build()
    {
        return new Input(_rows, _model);
    }

    public InputBuilder System(string content)
    {
        _rows.Add(new Message { Role = Role.System, Content = content });
        return this;
    }

    public InputBuilder Developer(string content)
    {
        _rows.Add(new Message { Role = Role.User, Content = content });
        return this;
    }

    public InputBuilder User(string content)
    {
        _rows.Add(new Message { Role = Role.User, Content = content });
        return this;
    }

    public InputBuilder Model(string model)
    {
        _model = model;
        return this;
    }

    public InputBuilder Rows(IEnumerable<Message> rows)
    {
        _rows.AddRange(rows);
        return this;
    }
}