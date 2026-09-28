namespace Tharga.Neurolito.Client.Contract;

public record LLModel
{
    /// <summary>
    /// Name of the model.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The model has been loaded into memory.
    /// </summary>
    public required bool Loaded { get; init; }

    public required ModelDetails Details { get; init; }
}