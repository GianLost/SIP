using SIP.API.Domain.Enums;

namespace SIP.API.Domain.Helpers.StatusHelper;

/// <summary>
/// Fornece métodos auxiliares relacionados à conversão de valores do enum <see cref="ProtocolStatus"/>
/// em representações textuais amigáveis para exibição em interfaces de usuário ou respostas de API.
/// </summary>
/// <remarks>
/// Este helper é especialmente útil em cenários onde é necessário apresentar o status de um protocolo
/// de forma legível e compreensível para o usuário final, evitando a exposição direta dos nomes técnicos
/// definidos no enum <see cref="ProtocolStatus"/>.
/// </remarks>
public static class ProtocolStatusHelper
{
    /// <summary>
    /// Converte o valor de um <see cref="ProtocolStatus"/> em uma nomenclatura legível e amigável.
    /// </summary>
    /// <param name="status">
    /// O valor do enum <see cref="ProtocolStatus"/> que representa o estado atual de um protocolo.
    /// </param>
    /// <returns>
    /// Uma string contendo a descrição amigável correspondente ao valor do enum informado.
    /// Caso o valor não seja reconhecido, retorna "Desconhecido".
    /// </returns>
    /// <remarks>
    /// Este método é implementado como um método de extensão, permitindo sua utilização de forma
    /// direta sobre instâncias do enum.  
    /// <para>Exemplo de uso:</para>
    /// <code language="csharp">
    /// ProtocolStatus status = ProtocolStatus.SentForReview;
    /// string descricao = status.ToFriendlyName(); // "Enviado para Revisão"
    /// </code>
    /// </remarks>
    public static string ToFriendlyName(this ProtocolStatus status) => status switch
    {
        ProtocolStatus.Open => "Em Aberto",
        ProtocolStatus.SentForReview => "Enviado para Revisão",
        ProtocolStatus.Received => "Recebido",
        ProtocolStatus.UnderReview => "Em Análise",
        ProtocolStatus.Approved => "Aprovado",
        ProtocolStatus.Rejected => "Rejeitado",
        ProtocolStatus.CorrectionRequested => "Correção Solicitada",
        ProtocolStatus.Finalized => "Finalizado",
        _ => "Desconhecido"
    };
}