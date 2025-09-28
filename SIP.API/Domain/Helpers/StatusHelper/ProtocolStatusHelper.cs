using SIP.API.Domain.Enums;

namespace SIP.API.Domain.Helpers.StatusHelper;

public static class ProtocolStatusHelper
{
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