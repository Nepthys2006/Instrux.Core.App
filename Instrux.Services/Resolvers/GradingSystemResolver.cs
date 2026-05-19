using Instrux.Domain.Enums;
using Instrux.Domain.Models;

namespace Instrux.Services.Resolvers;

public static class GradingSystemResolver
{
    public static GradingConfig GetWeightsForSubject(Subject subject) => GradingConfig.FromSubject(subject);
}
