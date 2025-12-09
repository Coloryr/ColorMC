namespace ColorMC.Core.Objs;

public record GameKeyObj(Guid Game, string? Data);

public record GameKeyDoubleDataObj(Guid Game, string? Data, string? Data1);
