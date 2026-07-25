using SP.Application.Abstractions.Messaging;

namespace SP.Application.Configuration.Queries.GetAppConfig;

public sealed record GetAppConfigQuery : IQuery<AppConfigResponse>;
