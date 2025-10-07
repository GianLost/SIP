namespace SIP.API.Domain.Helpers.RegExpressionHelper;

/// <summary>
/// Contém expressões regulares constantes utilizadas em diferentes partes da aplicação
/// para validação e formatação de dados de entrada.
/// </summary>
/// <remarks>
/// Esta estrutura fornece um ponto centralizado para manutenção de padrões de expressões regulares (Regex),
/// garantindo consistência e facilidade de reutilização em validações de nomes, siglas, senhas e números de telefone.
/// </remarks>
public readonly struct ConstExpressions
{
    /// <summary>
    /// Expressão regular para validação de nomes contendo apenas letras (maiúsculas e minúsculas),
    /// incluindo caracteres acentuados e espaços.
    /// </summary>
    /// <remarks>
    /// Exemplo de correspondência válida: <c>"João da Silva"</c>  
    /// Exemplo de correspondência inválida: <c>"João123"</c>
    /// </remarks>
    public const string NameRegex = @"^[A-Za-zÀ-ú\s]+$";

    /// <summary>
    /// Expressão regular para validação de abreviações, códigos ou siglas,
    /// permitindo letras, números, sublinhados, pontos e hífens.
    /// </summary>
    /// <remarks>
    /// Exemplo de correspondência válida: <c>"abc_123.def"</c>  
    /// Exemplo de correspondência inválida: <c>"abc 123"</c>
    /// </remarks>
    public const string AbridgementRegex = @"^[a-zA-Z0-9_.-]+$";

    /// <summary>
    /// Expressão regular para validação de senhas fortes.
    /// </summary>
    /// <remarks>
    /// A senha deve conter:
    /// <list type="bullet">
    /// <item><description>Pelo menos uma letra minúscula.</description></item>
    /// <item><description>Pelo menos uma letra maiúscula.</description></item>
    /// <item><description>Pelo menos um dígito numérico.</description></item>
    /// <item><description>Pelo menos um caractere especial.</description></item>
    /// <item><description>Tamanho mínimo de 8 e máximo de 255 caracteres.</description></item>
    /// </list>
    /// Exemplo de correspondência válida: <c>"Senha@123"</c>  
    /// Exemplo de correspondência inválida: <c>"senha123"</c>
    /// </remarks>
    public const string StrongPasswordRegex = "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^a-zA-Z\\d]).{8,255}$";

    /// <summary>
    /// Expressão regular para validação básica de números de telefone contendo
    /// apenas dígitos, parênteses, espaços e hífens.
    /// </summary>
    /// <remarks>
    /// Exemplo de correspondência válida: <c>"(38) 9999-9999"</c>  
    /// Exemplo de correspondência inválida: <c>"+55 (38) 9999-9999"</c>
    /// </remarks>
    public const string PhoneNumberRegex = @"^[0-9()\s-]+$";

    /// <summary>
    /// Expressão regular para validação de formato padronizado de número de telefone
    /// no formato brasileiro de resposta: <c>(XX) XXXX-XXXX</c> ou <c>(XX) 9XXXX-XXXX</c>.
    /// </summary>
    /// <remarks>
    /// Exemplo de correspondência válida: <c>"(38) 98888-7777"</c>  
    /// Exemplo de correspondência inválida: <c>"(38)8888-7777"</c>
    /// </remarks>
    public const string PhoneNumberResponseRegex = @"^\(\d{2}\)\s(?:9\d{4}-\d{4}|\d{4}-\d{4})$";
}