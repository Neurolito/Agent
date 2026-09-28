# Tharga.Neurolito.Client

Sends prompts to [Neurolito](https://neurolito.com), which queues them and runs them on your team's agents.

## Register

```csharp
builder.Services.AddNeurolito(o =>
{
    o.ApiKey = builder.Configuration["Neurolito:ApiKey"];
});
```

`ServerAddress` defaults to `https://neurolito.com/api/`. The API key is created in Neurolito for your team.

## Queue a prompt

```csharp
var input = new InputBuilder()
    .Model("llama3.1:8b-instruct-q4_K_M")
    .System("Answer in one sentence.")
    .User("What is a large language model?")
    .Build();

var response = await neurolito.EnqueueAsync(input);
if (!response.Success) throw new InvalidOperationException(response.Message);

var status = await neurolito.GetStatusAsync(response.RequestId!.Value);
```

`INeurolitoService` also cancels a queued prompt (`CancelAsync`), votes on an answer (`VoteAsync`) and
checks the connection (`CanConnectAsync`). Pass a `ResponseInstruction` to have the answer delivered to
you when the job completes.

## Source and issues

[github.com/Neurolito/neurolito-agent](https://github.com/Neurolito/neurolito-agent)
