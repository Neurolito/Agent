using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Tharga.Neurolito.Client;

public static class NeurolitoRegistration
{
    public static IServiceCollection AddNeurolito(this IServiceCollection services, Action<NeurolitoOptions> options = default)
    {
        var o = new NeurolitoOptions();
        options?.Invoke(o);

        if (string.IsNullOrWhiteSpace(o.ServerAddress)) throw new ArgumentException($"{nameof(NeurolitoOptions)}.{nameof(NeurolitoOptions.ServerAddress)} must be set.", nameof(options));

        services.AddSingleton(Options.Create(o));

        services.AddHttpClient(Constants.NeurolitoClient, (sp, client) =>
        {
            var opt = sp.GetRequiredService<IOptions<NeurolitoOptions>>().Value;
            var serverAddress = opt.ServerAddress.TrimEnd('/');
            if (!serverAddress.EndsWith("api", StringComparison.InvariantCultureIgnoreCase))
            {
                serverAddress = $"{serverAddress}/Api/";
            }
            else
            {
                serverAddress = $"{serverAddress}/";
            }

            client.BaseAddress = new Uri(serverAddress, UriKind.Absolute);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(ClientUserAgent.Value);
        });

        services.AddTransient<INeurolitoService, NeurolitoService>();

        return services;
    }
}