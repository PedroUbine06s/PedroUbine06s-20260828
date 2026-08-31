using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace GestaoColaboradores.Application.Common;

/// <summary>
/// Marca uma propriedade que NÃO deve ser normalizada na desserialização.
/// Usado nos campos de senha: cortar espaços alteraria silenciosamente o que a pessoa digitou,
/// e passphrases com espaço são mais fortes, não mais fracas (NIST SP 800-63B recomenda
/// aceitar todo caractere imprimível, espaço inclusive).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class NaoNormalizarAttribute : Attribute;

/// <summary>Corta os espaços das pontas de uma string vinda do corpo da requisição.</summary>
public sealed class TrimmingStringConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetString()?.Trim();

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}

/// <summary>
/// Normalização de entrada na borda: toda string que chega pela API perde os espaços das
/// pontas, exceto onde houver <see cref="NaoNormalizarAttribute"/>.
///
/// Um JsonConverter&lt;string&gt; registrado globalmente não daria conta sozinho — ele enxerga
/// o valor, não a propriedade de onde ele veio, e portanto não teria como poupar a senha.
/// A customização de contrato resolve isso porque roda no momento em que o serializador monta
/// o mapa de cada tipo, quando as propriedades e seus atributos ainda são visíveis.
/// </summary>
public static class Normalizacao
{
    private static readonly TrimmingStringConverter Trim = new();

    public static JsonSerializerOptions Configurar(JsonSerializerOptions options)
    {
        options.TypeInfoResolver = (options.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver())
            .WithAddedModifier(AplicarTrimEmTextos);

        return options;
    }

    private static void AplicarTrimEmTextos(JsonTypeInfo typeInfo)
    {
        foreach (var propriedade in typeInfo.Properties)
        {
            if (propriedade.PropertyType != typeof(string))
                continue;

            var isento = propriedade.AttributeProvider?
                .GetCustomAttributes(typeof(NaoNormalizarAttribute), inherit: true)
                .Length > 0;

            if (isento)
                continue;

            propriedade.CustomConverter = Trim;
        }
    }
}
